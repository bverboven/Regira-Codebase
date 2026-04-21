# Regira IO.Storage

Regira IO.Storage provides a **unified abstraction** for file storage operations across multiple backends. All implementations share the same `IFileService` interface, making storage backends interchangeable in consuming code.

## Projects

| Project | Package | Backend | File services |
|---------|---------|---------|-------|
| `Common.IO.Storage` | `Regira.IO.Storage` | Local file system | `BinaryFileService` |
|  | | Zip file system | `ZipFileService` |
| `IO.Storage.Azure` | `Regira.IO.Storage.Azure` | Azure Blob Storage | `BinaryBlobService` |
| `IO.Storage.SSH` | `Regira.IO.Storage.SSH` | SFTP / SSH server | `SftpService` |
| `IO.Storage.GitHub` | `Regira.IO.Storage.GitHub` | GitHub repository | `GitHubService` (Read-only) |

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

## Quick Start

```csharp
services.AddSingleton<IFileService>(_ =>
    new BinaryFileService(new FileSystemOptions { RootFolder = "/var/app/uploads" }));

// In a service
var bytes = await storage.GetBytes("invoices/2024/inv-001.pdf");
await storage.Save("exports/report.pdf", pdfBytes);
```

## IFileService

All backends implement this interface. **Identifiers** are relative paths within the storage root (e.g. `"folder/file.pdf"`). **URIs** are the absolute addresses returned by `GetAbsoluteUri`.

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

> `Save` returns the final identifier used — it may differ if the backend renames on conflict.

### URI helpers

```csharp
string  Root { get; }                        // storage root URI / path
string  GetAbsoluteUri(string identifier)    // relative → absolute
string  GetIdentifier(string uri)            // absolute → relative
string? GetRelativeFolder(string identifier) // extract parent folder
```

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

## Implementations

### File System (`BinaryFileService`)

Stores files on the local disk.

**Package:** `Regira.IO.Storage`

```csharp
var service = new BinaryFileService(new FileSystemOptions { RootFolder = "/var/app/storage" });
```

**Text files** — use `TextFileService` directly, or wrap any `IFileService` with the `DefaultTextFileService` decorator:

```csharp
var text = new DefaultTextFileService(anyFileService, Encoding.UTF8);
string? content = await text.GetContents("config/app.json");
await text.Save("config/app.json", jsonString);
```

---

### Azure Blob Storage (`BinaryBlobService`)

**Package:** `Regira.IO.Storage.Azure` — **NuGet dependency:** `Azure.Storage.Blobs`

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

### SSH / SFTP (`SftpService`)

**Package:** `Regira.IO.Storage.SSH` — **NuGet dependency:** `SSH.NET`

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

> `SftpCommunicator` holds a single persistent connection. Dispose it on application shutdown.

---

### GitHub — read-only (`GitHubService`)

**Package:** `Regira.IO.Storage.GitHub` — **NuGet dependency:** none (uses `HttpClient`)

```csharp
var service = new GitHubService(
    new GitHubOptions
    {
        Uri       = "https://api.github.com/repos/owner/repo/contents/",
        Key       = "ghp_xxxxxxxxxxxx",   // PAT — optional for public repos
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

## ZIP / Compression

### ZipFileService — browse an archive via IFileService

`ZipFileService` implements `IFileService` and `IDisposable`. Construct it with a `ZipFileCommunicator` that points to an existing archive or starts a fresh one:

```csharp
// Open an existing zip file
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

> `ZipFileServiceFactory` is a convenience wrapper: `new ZipFileServiceFactory().Create(sourceFile, password)` is equivalent to constructing `ZipFileService` directly.

### ZipBuilder — create archives

```csharp
IMemoryFile zip = await new ZipBuilder()
    .For([new BinaryFileItem { Name = "report.pdf", Bytes = pdfBytes },
          new BinaryFileItem { Name = "data.csv",   Bytes = csvBytes }])
    .Build();
```

### ZipUtility — extension methods

```csharp
IMemoryFile archive         = files.Zip();                             // collection → zip
IMemoryFile archive         = paths.Zip(baseFolder: "/var/exports");   // paths → zip
BinaryFileCollection items  = existingZip.Unzip();                     // zip → collection
string[] extracted          = existingZip.Unzip(targetDirectory: "/tmp/out");
```

## Helpers

### FileProcessor — recursive processing

```csharp
await new FileProcessor(fileService).ProcessFiles(
    new FileSearchObject { FolderUri = "exports/", Recursive = true },
    async (identifier, svc) => { /* process each file */ }
);
```

### FileNameHelper — unique filenames

```csharp
var helper = new FileNameHelper(fileService);
string safe = await helper.NextAvailableFileName("invoices/report.pdf");
// → "invoices/report-(1).pdf" when "invoices/report.pdf" already exists
```

Customise the pattern: `new FileNameHelper.Options { NumberPattern = " ({0})" }`

### ExportHelper — copy between services

```csharp
await new ExportHelper(source, target)
    .Export(new FileSearchObject { FolderUri = "backups/", Recursive = true });
```

### FileNameUtility — path helpers

```csharp
FileNameUtility.GetAbsoluteUri("folder/file.txt", root)
FileNameUtility.GetRelativeUri(absolutePath, root)
FileNameUtility.GetCleanFileName("folder/sub/file.txt")  // → "file.txt"
FileNameUtility.Combine("folder", "sub", "file.txt")
FileNameUtility.SanitizeFilename("con.txt")              // avoids Windows reserved names
```

## Overview

1. **[Index](01-index.md)** — Overview, interface, and implementation reference
1. [Examples](02-examples.md) — Backend swap, transform & re-upload, GitHub→Azure mirror, ZIP export, safe upload
