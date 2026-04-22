# IO.Storage — Evaluation Report

**Date:** 2026-04-23  
**Scope:** `Common.IO.Storage`, `IO.Storage.Azure`, `IO.Storage.SSH`, `IO.Storage.GitHub`, `IO.Storage.SimpleTCP`  
**Reviewer:** Claude Sonnet 4.6

---

## Summary

The abstraction is sound. `IFileService` gives a clean, uniform surface for swapping backends, and the `Root` / `Identifier` / `Prefix` addressing model is coherent. The main concerns are two exploitable security vulnerabilities (path traversal and Zip Slip), a handful of correctness bugs, and structural duplication that keeps diverging across implementations.

---

## Security Risks

### 1. Path Traversal — `BinaryFileService` *(Critical)*

`FileNameUtility.GetAbsoluteUri` (local) only checks whether the identifier already starts with the root. It never verifies that the resolved path stays *within* the root after combining.

```csharp
// identifier = "../../windows/system32/config/SAM"
// Path.Combine(root, identifier) silently escapes the root
public static string GetAbsoluteUri(string filename, string? root)
{
    var path = string.IsNullOrWhiteSpace(root) || filename.StartsWith(root, ...)
        ? filename
        : Combine(root, filename);   // no containment check
    return ConvertForwardSlashes(path);
}
```

Any caller that accepts user-controlled input for `GetBytes`, `Save`, or `Delete` can read or write arbitrary files on disk.

**Fix:** Canonicalize and assert containment after combining.

```csharp
var full = Path.GetFullPath(Path.Combine(root, identifier));
if (!full.StartsWith(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase))
    throw new UnauthorizedAccessException("Path traversal detected.");
```

---

### 2. Zip Slip — `ZipUtility.ExtractFiles` *(High)*

```csharp
var fullPath = Path.Combine(targetDirectory, entry.FullName.TrimEnd('/'));
```

A crafted archive with entries like `"../../etc/shadow"` will write files outside `targetDirectory`. No validation checks that `fullPath` remains within the target.

**Fix:** Same canonicalize-and-assert pattern as above, applied to each entry before writing.

---

### 3. GitHub PAT Token in Identifiers *(High)*

`GitHubExtensions.ToTokenUri()` appends `?token=<PAT>` to identifiers returned by `List()`. Those token-bearing identifiers flow into `ExportHelper`, logs, and error messages. Tokens embedded in URLs are routinely captured by access logs and monitoring tools.

**Fix:** Strip the token from identifiers returned by `List()`. Pass the token only in HTTP request headers (already done for direct API calls via `Authorization: Bearer`).

---

### 4. Blocking `.Wait()` — Async-Over-Sync *(High)*

`BinaryBlobService.GetAbsoluteUri`, `GetIdentifier`, and `GetRelativeFolder` all call `communicator.Open().Wait()`. `SftpCommunicator.Dispose()` calls `Close().Wait()`. Both can deadlock under ASP.NET Classic or any `SynchronizationContext` that requires thread affinity.

**Affected files:**
- `src/IO.Storage.Azure/BinaryBlobService.cs` — `GetAbsoluteUri`, `GetIdentifier`, `GetRelativeFolder`
- `src/IO.Storage.SSH/SftpCommunicator.cs` — `Dispose()`

**Fix:** Make the sync helpers truly sync (don't call `Open()` inside them — require `Open()` to have been awaited beforehand), or use `.GetAwaiter().GetResult()` with a note on the constraint.

---

## Bugs

### 5. `SftpService.List` Returns Nothing for `FileEntryTypes.All` *(Medium)*

```csharp
.Where(f => so.Type == FileEntryTypes.Files && f.IsRegularFile
         || so.Type == FileEntryTypes.Directories && f.IsDirectory)
```

When `Type == FileEntryTypes.All`, neither branch matches — the result is always an empty list. The default value of `FileSearchObject.Type` is `All`, so the default call `List()` returns nothing on SFTP.

**Fix:** Add `|| so.Type == FileEntryTypes.All` to both conditions, or restructure the filter to match both when `All` is set.

---

### 6. `Save()` Return Value Inconsistent with Interface Contract *(Medium)*

`IFileService.Save` is documented to return "the final identifier used." In practice:

| Implementation | Returns |
|----------------|---------|
| `BinaryFileService` | Absolute disk path |
| `BinaryBlobService` | Full Azure blob URI |
| `SftpService` | Absolute SFTP path |

None return a relative identifier. Code that feeds the return value back into `GetBytes` or `Exists` will break when switching backends.

---

### 7. `ZipFileService.Exists` vs `GetStream` Separator Mismatch *(Medium)*

```csharp
// Exists — replaces forward slashes before searching
Task<bool> Exists(string identifier) 
    => Task.FromResult(ZipArchive.Find(identifier.Replace('/', '\\')) != null);

// GetStream — searches with original separators
Task<Stream?> GetStream(string identifier) 
{
    var entry = ZipArchive.Find(identifier);   // no replacement
    ...
}
```

The same identifier can return `true` from `Exists` and `null` from `GetStream`.

---

### 8. Non-Atomic `Move` in `BinaryBlobService` *(Medium)*

```csharp
await targetBlob.StartCopyFromUriAsync(GetBlobUri(sourceIdentifier));
await sourceBlob.DeleteIfExistsAsync();   // copy may not be complete
```

`StartCopyFromUriAsync` is asynchronous on Azure's side. The source is deleted immediately after the copy is *started*, not after it *completes*. This can cause data loss if the copy is still in progress.

**Fix:** Poll or await copy completion (e.g., check `CopyStatus` or use a server-side copy with `WaitUntil.Completed`) before deleting the source.

---

### 9. Race Condition in `AzureCommunicator.Open()` *(Medium)*

```csharp
public async Task Open()
{
    if (IsOpened) return;        // not guarded by a lock
    ...
    IsOpened = true;
}
```

Two concurrent awaits can both read `IsOpened == false` and both attempt to create the container. A `SemaphoreSlim(1, 1)` or `Interlocked`-based guard would fix this.

---

## Design & Maintainability

### 10. Four Copies of `FileNameUtility` *(Low)*

`Common.IO.Storage`, `IO.Storage.Azure`, `IO.Storage.SSH`, and `IO.Storage.GitHub` each contain their own `FileNameUtility` with subtly different behavior (backslash vs. forward-slash defaults, `Combine` logic, `GetRelativeFolder` separator choices). This is where bugs like #7 originate. A single shared, well-tested utility in `Common` would eliminate the duplication.

---

### 11. `Task.WaitAll` in `FileProcessor` *(Low)*

```csharp
Task.WaitAll(handleFilesFuncs);   // blocks thread; deadlock-prone
```

The method is already `async`, so the fix is a one-liner:

```csharp
await Task.WhenAll(handleFilesFuncs);
```

---

### 12. `HttpClient` Created Per Call in `GitHubService` *(Low)*

Each `Exists`, `GetBytes`, `GetStream`, and `List` call instantiates and disposes a new `HttpClient`. Under any meaningful load this exhausts socket connections (TIME_WAIT port exhaustion). The service should accept an `IHttpClientFactory` or hold a singleton `HttpClient`.

---

### 13. No `CancellationToken` Support on `IFileService` *(Low)*

Long-running remote operations (Azure upload, SFTP transfer, GitHub listing) cannot be cancelled. Adding `CancellationToken ct = default` to every method is a breaking API change but is the right call before the interface stabilises.

---

### 14. `TCPService` Violates the Interface Contract *(Low)*

`TCPService` is a write-only channel dressed as an `IFileService`. `Root` throws `NotSupportedException` (a property getter that throws), and eleven of the twelve interface methods throw. The inconsistency between `NotSupportedException` and `NotImplementedException` across these methods adds confusion. A purpose-built `ISendOnlyService` interface would be a better fit.

---

### 15. Silent Container Auto-Creation in `AzureCommunicator` *(Low)*

```csharp
if (!await Container.ExistsAsync())
    await Container.CreateAsync();
```

If the configured container name is a typo or points to the wrong environment, a new empty container is silently created rather than failing visibly. An opt-in `AutoCreate` flag (defaulting to `false`) would make the behavior explicit.

---

## Priority Summary

| # | Finding | Severity |
|---|---------|----------|
| 1 | Path traversal in `BinaryFileService` | **Critical** |
| 2 | Zip Slip in `ZipUtility.ExtractFiles` | **High** |
| 3 | PAT token leaked in GitHub identifiers | **High** |
| 4 | `.Wait()` deadlock risk (Azure, SSH) | **High** |
| 5 | `SftpService.List(All)` returns nothing | **Medium** |
| 6 | `Save()` return value inconsistency | **Medium** |
| 7 | `Exists`/`GetStream` separator mismatch in `ZipFileService` | **Medium** |
| 8 | Non-atomic `Move` in `BinaryBlobService` | **Medium** |
| 9 | Race condition in `AzureCommunicator.Open()` | **Medium** |
| 10 | Duplicated `FileNameUtility` across projects | Low |
| 11 | `Task.WaitAll` in `FileProcessor` | Low |
| 12 | `HttpClient` per-call in `GitHubService` | Low |
| 13 | No `CancellationToken` support | Low |
| 14 | `TCPService` violates `IFileService` contract | Low |
| 15 | Silent container auto-creation in Azure | Low |

**Recommended first actions:** fix #1 (path traversal) and #2 (Zip Slip) — both are reachable with user-controlled input and have no existing mitigations.
