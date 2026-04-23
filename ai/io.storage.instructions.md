# Regira IO.Storage AI Agent Instructions

You are an expert .NET developer working with the `Regira.IO.Storage` packages.
Your role is to help read, write, and manage files across multiple storage backends using the exact public API described here.

🚨 CRITICAL RULE — READ BEFORE EVERY METHOD USE:
If the exact signature is not listed in this file, STOP.
DO NOT invent. DO NOT combine patterns. ASK the user.

---

## Installation

```xml
<!-- Local file system (also ships the shared abstractions) -->
<PackageReference Include="Regira.IO.Storage" Version="5.*" />

<!-- Azure Blob Storage -->
<PackageReference Include="Regira.IO.Storage.Azure" Version="5.*" />

<!-- SSH / SFTP -->
<PackageReference Include="Regira.IO.Storage.SSH" Version="5.*" />

<!-- GitHub (read-only) -->
<PackageReference Include="Regira.IO.Storage.GitHub" Version="5.*" />
```

> Add the Regira feed to `NuGet.Config`:
> ```xml
> <add key="Regira" value="https://packages.regira.com/v3/index.json" />
> ```

---

## Key Concept: Identifier vs. Path vs. URI

All `IFileService` methods use **identifiers** — paths relative to the storage root.

```
Root        →  /var/app/storage/
Prefix      →                   invoices/2024/
FileName    →                                 inv-001.pdf
Identifier  →                   invoices/2024/inv-001.pdf   ← use this in all API calls
Path        →  /var/app/storage/invoices/2024/inv-001.pdf
```

| Concept | Description |
|---------|-------------|
| `Root` | Backend-specific base address (local path, Azure container URL, SFTP base dir) |
| `Identifier` | Relative key — `Prefix + FileName` — portable across backend swaps |
| `Path` | `Root + Identifier` — full absolute address |

---

## IFileService

All backends implement this single interface.

### Read

```csharp
Task<bool>                Exists(string identifier)
Task<byte[]?>             GetBytes(string identifier)
Task<Stream?>             GetStream(string identifier)
Task<IEnumerable<string>> List(FileSearchObject? so = null)
```

### Write

```csharp
Task<string> Save(string identifier, byte[] bytes,  string? contentType = null)
Task<string> Save(string identifier, Stream stream, string? contentType = null)
Task         Move(string sourceIdentifier, string targetIdentifier)
Task         Delete(string identifier)
```

> `Save` returns the final identifier — it may differ if the backend renames on conflict.

### URI Helpers

```csharp
string  Root { get; }
string  GetAbsoluteUri(string identifier)   // relative → absolute
string  GetIdentifier(string uri)           // absolute → relative
string? GetRelativeFolder(string identifier) // extract parent folder
```

---

## FileSearchObject

Filter parameter for `List()`.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `FolderUri` | `string?` | `null` | Restrict to this folder |
| `Extensions` | `ICollection<string>?` | `null` | Filter by extension — e.g. `[".jpg", ".png"]` |
| `Recursive` | `bool` | `false` | Include subdirectories |
| `Type` | `FileEntryTypes` | `All` | `Files`, `Directories`, or `All` |

```csharp
var images = await storage.List(new FileSearchObject
{
    FolderUri  = "products/",
    Extensions = [".jpg", ".webp"],
    Recursive  = true,
    Type       = FileEntryTypes.Files
});
```

---

## Implementations

### Local File System — `BinaryFileService`

**Package:** `Regira.IO.Storage`

```csharp
var service = new BinaryFileService(new FileSystemOptions { RootFolder = "/var/app/storage" });
```

**Text files** — wrap any `IFileService` with `DefaultTextFileService`:

```csharp
var text = new DefaultTextFileService(anyFileService, Encoding.UTF8);
string? content = await text.GetContents("config/app.json");
await text.Save("config/app.json", jsonString);
```

---

### Azure Blob Storage — `BinaryBlobService`

**Package:** `Regira.IO.Storage.Azure`

```csharp
var communicator = new AzureCommunicator(new AzureOptions
{
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=…",
    ContainerName    = "my-container"
});
await communicator.Open();   // idempotent — safe to call multiple times

var service = new BinaryBlobService(communicator);
```

| Option | Type | Description |
|--------|------|-------------|
| `ConnectionString` | `string` | Azure Storage connection string |
| `ContainerName` | `string` | Blob container name |

---

### SSH / SFTP — `SftpService`

**Package:** `Regira.IO.Storage.SSH`

```csharp
var communicator = new SftpCommunicator(new SftpConfig
{
    Host          = "sftp.example.com",
    Port          = 22,
    UserName      = "deploy",
    Password      = "s3cr3t",
    ContainerName = "/home/deploy/files"
});

var service = new SftpService(communicator);
```

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Host` | `string` | *(required)* | SSH server hostname |
| `Port` | `int` | `22` | SSH port |
| `UserName` | `string` | *(required)* | Login username |
| `Password` | `string?` | `null` | Login password |
| `ContainerName` | `string` | `"/"` | Remote base directory |

> `SftpCommunicator` holds a persistent connection. Dispose it on application shutdown.

---

### GitHub — `GitHubService` (read-only)

**Package:** `Regira.IO.Storage.GitHub`

```csharp
var service = new GitHubService(
    new GitHubOptions
    {
        Uri       = "https://api.github.com/repos/owner/repo/contents/",
        Key       = "ghp_xxxxxxxxxxxx",
        UserAgent = "MyApp/1.0"
    },
    jsonSerializer
);
```

| Option | Type | Description |
|--------|------|-------------|
| `Uri` | `string` | GitHub API contents endpoint (must end with `/contents/`) |
| `Key` | `string?` | Personal Access Token |
| `UserAgent` | `string?` | `User-Agent` header — GitHub requires a non-empty value |

**Supported:** `Exists`, `GetBytes`, `GetStream`, `List`
**Not supported:** `Save`, `Move`, `Delete` — throws `NotImplementedException`

---

## ZIP / Compression

### `ZipFileService` — browse an archive via IFileService

```csharp
// Open an existing zip
using var zipService = new ZipFileService(new ZipFileCommunicator { SourceFile = existingZip });
var entries = await zipService.List();
var bytes   = await zipService.GetBytes("report.pdf");

// Start a new empty archive
using var newZip = new ZipFileService(new ZipFileCommunicator());
await newZip.Save("data.csv", csvBytes);
```

| `ZipFileCommunicator` | Type | Description |
|-----------------------|------|-------------|
| `SourceFile` | `IMemoryFile?` | Existing zip to open — omit to start empty |
| `Password` | `string?` | Archive password (optional) |

### `ZipBuilder` — create archives

```csharp
IMemoryFile zip = await new ZipBuilder()
    .For([new BinaryFileItem { Name = "report.pdf", Bytes = pdfBytes },
          new BinaryFileItem { Name = "data.csv",   Bytes = csvBytes }])
    .Build();
```

### `ZipUtility` — extension methods

```csharp
IMemoryFile archive        = files.Zip();
IMemoryFile archive        = paths.Zip(baseFolder: "/var/exports");
BinaryFileCollection items = existingZip.Unzip();
string[] extracted         = existingZip.Unzip(targetDirectory: "/tmp/out");
```

---

## Helpers

### `FileProcessor` — recursive processing

```csharp
await new FileProcessor(fileService).ProcessFiles(
    new FileSearchObject { FolderUri = "exports/", Recursive = true },
    async (identifier, svc) => { /* process each file */ }
);
```

### `FileNameHelper` — unique filenames

```csharp
var helper = new FileNameHelper(fileService);
string safe = await helper.NextAvailableFileName("invoices/report.pdf");
// → "invoices/report-(1).pdf" when original already exists
```

Customise: `new FileNameHelper.Options { NumberPattern = " ({0})" }`

### `ExportHelper` — copy between services

```csharp
await new ExportHelper(source, target)
    .Export(new FileSearchObject { FolderUri = "backups/", Recursive = true });
```

### `FileNameUtility` — path helpers

```csharp
FileNameUtility.GetAbsoluteUri("folder/file.txt", root)
FileNameUtility.GetRelativeUri(absolutePath, root)
FileNameUtility.GetCleanFileName("folder/sub/file.txt")  // → "file.txt"
FileNameUtility.Combine("folder", "sub", "file.txt")
FileNameUtility.SanitizeFilename("con.txt")              // avoids Windows reserved names
```

---

## DI Registration

```csharp
// Local file system
services.AddSingleton<IFileService>(_ =>
    new BinaryFileService(new FileSystemOptions { RootFolder = "/var/app/uploads" }));

// Azure Blob
services.AddSingleton<IFileService>(sp =>
{
    var communicator = new AzureCommunicator(new AzureOptions
    {
        ConnectionString = configuration["Azure:Storage"],
        ContainerName    = "uploads"
    });
    communicator.Open().GetAwaiter().GetResult();
    return new BinaryBlobService(communicator);
});
```

---

## Backend Comparison

| Backend | Package | Write | Listing | Notes |
|---------|---------|-------|---------|-------|
| `BinaryFileService` | `Regira.IO.Storage` | ✓ | ✓ | Local disk |
| `BinaryBlobService` | `Regira.IO.Storage.Azure` | ✓ | ✓ | Azure Blob |
| `SftpService` | `Regira.IO.Storage.SSH` | ✓ | ✓ | Remote SSH/SFTP |
| `GitHubService` | `Regira.IO.Storage.GitHub` | — | ✓ | Read-only GitHub |
| `ZipFileService` | `Regira.IO.Storage` | ✓ | ✓ | In-memory ZIP archive |

**Load these instructions when** the user asks to store, retrieve, list, or manage files; work with Azure Blob, SFTP, GitHub, or ZIP storage; swap storage backends; or use `IFileService`.
