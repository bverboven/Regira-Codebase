---
name: Regira Attachments
description: >
  Adds file attachment support to a Regira Entities entity: EntityAttachment model,
  owning entity update, controller, DbContext changes, and DI registration.
tools:
  - codebase
  - editFiles
  - runCommands
handoffs:
  - label: "Attachments done → create migration"
    agent: regira-database
    prompt: "Attachment support is set up. Create and apply the migration for the new DbSets and relationships."
    send: false
---

# Regira Entities — Attachments Agent

You implement the complete **file attachment workflow** for an entity.

Always use exact namespaces from `.github/instructions/regira-namespaces.instructions.md`.

---

## Prerequisites

Add to `.csproj` (if not present):
```xml
<!-- Local file system / SFTP -->
<PackageReference Include="Regira.IO.Storage" Version="*" />
<!-- Azure Blob Storage (alternative) -->
<!-- <PackageReference Include="Regira.IO.Storage.Azure" Version="*" /> -->
```

---

## Step 1 — EntityAttachment Model

**File:** `Entities/{Entities}/{Entity}Attachment.cs`

```csharp
using Regira.Entities.Attachments.Models;

// EntityAttachment<TKey (own PK), TObjectKey (owning entity PK)>
public class {Entity}Attachment : EntityAttachment<int, int>
{
    public override string ObjectType => nameof({Entity});
}
```

Non-int owning entity PK (e.g. `Guid`):
```csharp
public class {Entity}Attachment : EntityAttachment<int, Guid>
{
    public override string ObjectType => nameof({Entity});
}
```

---

## Step 2 — Update Owning Entity

**File:** `Entities/{Entities}/{Entity}.cs` — add two interfaces and three properties:

```csharp
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Attachments.Models;

public class {Entity} : IEntityWithSerial, /* existing interfaces */,
    IHasAttachments,
    IHasAttachments<{Entity}Attachment>
{
    // ... existing properties ...

    public bool? HasAttachment { get; set; }
    public ICollection<{Entity}Attachment>? Attachments { get; set; }

    // Explicit interface implementation — required
    ICollection<IEntityAttachment>? IHasAttachments.Attachments
    {
        get => Attachments?.Cast<IEntityAttachment>().ToArray();
        set => Attachments = value?.Cast<{Entity}Attachment>().ToArray();
    }
}
```

---

## Step 3 — Attachment Controller

**File:** `Controllers/{Entity}AttachmentsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Attachments.Abstractions;

// Default DTOs
[ApiController]
[Route("api/{entities}/{objectId}/attachments")]
public class {Entity}AttachmentsController
    : EntityAttachmentControllerBase<{Entity}Attachment, int, int>
{
}
```

With custom DTOs:
```csharp
[ApiController]
[Route("api/{entities}/{objectId}/attachments")]
public class {Entity}AttachmentsController
    : EntityAttachmentControllerBase<{Entity}Attachment, {Entity}AttachmentDto, {Entity}AttachmentInputDto>
{
}
```

---

## Step 4 — Update DbContext

**File:** `Data/AppDbContext.cs`

```csharp
using Regira.Entities.Attachments.Models;

public class AppDbContext : DbContext
{
    // One shared table for all attachments
    public DbSet<Attachment> Attachments { get; set; } = null!;

    // One per EntityAttachment type
    public DbSet<{Entity}Attachment> {Entity}Attachments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention(18, 2);

        // Link EntityAttachment → Attachment table
        modelBuilder.Entity<{Entity}Attachment>()
            .HasOne(x => x.Attachment)
            .WithMany()
            .HasForeignKey(x => x.AttachmentId);

        // Link owning entity → EntityAttachment
        modelBuilder.Entity<{Entity}>(entity =>
        {
            entity.HasMany(e => e.Attachments)
                  .WithOne()
                  .HasForeignKey(e => e.ObjectId)
                  .HasPrincipalKey(e => e.Id);
        });
    }
}
```

---

## Step 5 — DI Registration

Chain `.WithAttachments(...)` on the result of `UseEntities<>()`.

**File:** `Extensions/ServiceCollectionExtensions.cs`

### Local File System (default)
```csharp
using Regira.IO.Storage.FileSystem;

services
    .UseEntities<AppDbContext>(options => { /* ... */ })
    .WithAttachments(_ => new BinaryFileService(
        new FileSystemOptions
        {
            RootFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads")
        }
    ));
```

### Azure Blob Storage
```csharp
using Regira.IO.Storage.Azure;

services
    .UseEntities<AppDbContext>(/* ... */)
    .WithAttachments(_ => new BinaryBlobService(
        new AzureCommunicator(new AzureOptions
        {
            ConnectionString = configuration["Azure:StorageConnectionString"],
            ContainerName = "attachments"
        })
    ));
```

### SFTP
```csharp
using Regira.IO.Storage.SSH;

services
    .UseEntities<AppDbContext>(/* ... */)
    .WithAttachments(_ => new SftpService(new SftpCommunicator(new SftpConfig
    {
        Host = configuration["Sftp:Host"],
        UserName = configuration["Sftp:Username"],
        Password = configuration["Sftp:Password"],
        ContainerName = "/uploads"
    })));
```

---

## Step 6 — Migration

```bash
dotnet ef migrations add Add_{Entity}Attachment
dotnet ef database update
```

---

## Endpoint URLs (provided by base controller automatically)

```
GET    /api/{entities}/{objectId}/attachments          List attachments for object
GET    /api/{entities}/{objectId}/attachments/{id}     Single attachment
POST   /api/{entities}/{objectId}/attachments          Upload
PUT    /api/{entities}/{objectId}/attachments/{id}     Update metadata
DELETE /api/{entities}/{objectId}/attachments/{id}     Delete
```

---

## IAttachment Properties Reference

| Property | Type | Description |
|----------|------|-------------|
| `FileName` | `string?` | File name only (not full path) |
| `Identifier` | `string?` | Prefix + filename |
| `Prefix` | `string?` | Folder structure (excluding root) |
| `Path` | `string?` | Full URI / path |
| `ContentType` | `string?` | MIME type |
| `Length` | `long` | Size in bytes |
| `Bytes` | `byte[]?` | Content as byte array |
| `Stream` | `Stream?` | Content as stream |
| `ObjectType` | `string` | Owning entity type name |
| `ObjectId` | `TObjectKey` | FK to owning entity |
| `NewFileName` | `string?` | Renames file on save |
| `NewBytes` | `byte[]?` | Replaces file content on save |
| `SortOrder` | `int` | Display order |
