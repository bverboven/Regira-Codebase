# Regira Entities — Attachments Agent

You are a specialized agent responsible for implementing **file attachments** in the Regira Entities framework. This includes defining `Attachment` models, `EntityAttachment` models, configuring file storage, updating owning entities, and setting up attachment controllers and DI.

---

## Core Concepts

The Attachments module has two linked components:

| Model | Purpose |
|-------|---------|
| `Attachment` | Represents a stored file (metadata + file bytes) |
| `EntityAttachment` | Links an `Attachment` to a specific entity (e.g. `Product`, `Article`) |

- All attachments for all entities share **one `Attachment` table**
- Each `EntityAttachment` record links an `Attachment` to a specific owning entity row

---

## Attachment Model

### Interface

```csharp
public interface IAttachment : IBinaryFile, IHasTimestamps;
public interface IAttachment<TKey> : IAttachment, IEntity<TKey>;
```

### `IBinaryFile` Properties (auto-populated by `AttachmentFileService`)

| Property | Type | Description |
|----------|------|-------------|
| `FileName` | `string?` | File name only (not full path) |
| `Identifier` | `string?` | Identifier in context: `Prefix + FileName` |
| `Prefix` | `string?` | Folder structure excluding root |
| `Path` | `string?` | Full path or URI |
| `ContentType` | `string?` | MIME type |
| `Length` | `long` | File size in bytes |
| `Bytes` | `byte[]?` | File content as byte array |
| `Stream` | `Stream?` | File content as stream |

---

## EntityAttachment Model

### Step 1: Define the EntityAttachment class

```csharp
using Regira.Entities.Attachments.Models;

public class ProductAttachment : EntityAttachment<int, int>
// EntityAttachment<TKey, TObjectKey>
// TKey = PK of this join record (int)
// TObjectKey = PK type of the owning entity (int)
{
    public override string ObjectType => nameof(Product);
}
```

**Interface (simplified):**

```csharp
public interface IEntityAttachment<TKey, TObjectKey, TAttachmentKey, TAttachment>
    : IEntity<TKey>, IHasObjectId<TObjectKey>, ISortable
{
    string? ObjectType { get; }         // Name of owning entity type
    string? NewFileName { get; set; }   // For updating file name
    string? NewContentType { get; set; }
    byte[]? NewBytes { get; set; }      // For uploading new file content
    TAttachmentKey AttachmentId { get; set; }
    TAttachment? Attachment { get; set; }
}
```

---

## Owning Entity

### Step 2: Update the owning entity

Implement both `IHasAttachments` and `IHasAttachments<TEntityAttachment>`:

```csharp
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Attachments.Models;

public class Product : IEntityWithSerial, IHasAttachments, IHasAttachments<ProductAttachment>
{
    public int Id { get; set; }
    // ... other properties ...

    // Required by IHasAttachments
    public bool? HasAttachment { get; set; }
    public ICollection<ProductAttachment>? Attachments { get; set; }

    // Explicit interface implementation (required)
    ICollection<IEntityAttachment>? IHasAttachments.Attachments
    {
        get => Attachments?.Cast<IEntityAttachment>().ToArray();
        set => Attachments = value?.Cast<ProductAttachment>().ToArray();
    }
}
```

---

## DbContext

### Step 3: Add DbSets and configure relationships

```csharp
using Regira.Entities.Attachments.Models;

public class YourDbContext : DbContext
{
    public DbSet<Attachment> Attachments { get; set; } = null!;
    public DbSet<ProductAttachment> ProductAttachments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductAttachment>()
            .HasOne(x => x.Attachment)
            .WithMany()
            .HasForeignKey(x => x.AttachmentId);

        modelBuilder.Entity<Product>(entity =>
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

## Attachment Controller

### Step 4: Create the controller

```csharp
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Attachments.Abstractions;

// Using default DTOs (EntityAttachmentDto & EntityAttachmentInputDto)
[ApiController]
[Route("api/products/{objectId}/attachments")]
public class ProductAttachmentsController
    : EntityAttachmentControllerBase<ProductAttachment, int, int>
{
}

// OR with custom DTOs
[ApiController]
[Route("api/products/{objectId}/attachments")]
public class ProductAttachmentsController
    : EntityAttachmentControllerBase<ProductAttachment, ProductAttachmentDto, ProductAttachmentInputDto>
{
}
```

The attachment controller exposes endpoints to upload, list, and delete attachments for a specific owning entity instance (`objectId`).

---

## Dependency Injection

### Step 5: Configure the file storage backend

Choose the appropriate `IFileService` implementation:

#### Local File System (default)

```csharp
using Regira.IO.Storage.FileSystem;

services.UseEntities<YourDbContext>(/* ... */)
    .WithAttachments(_ => new BinaryFileService(
        new FileSystemOptions
        {
            RootFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads")
        }
    ));
```

#### Azure Blob Storage

```csharp
using Regira.IO.Storage.Azure;

services.UseEntities<YourDbContext>(/* ... */)
    .WithAttachments(_ => new BinaryBlobService(
        new AzureCommunicator(new AzureOptions
        {
            ConnectionString = configuration["Azure:StorageConnectionString"],
            ContainerName = "attachments"
        })
    ));
```

**Required package:** `Regira.IO.Storage.Azure`

#### SFTP

```csharp
using Regira.IO.Storage.SSH;

services.UseEntities<YourDbContext>(/* ... */)
    .WithAttachments(_ => new SftpService(
        new SftpCommunicator(new SftpConfig
        {
            Host = "sftp.example.com",
            UserName = "user",
            Password = "pass",
            ContainerName = "/uploads"
        })
    ));
```

**Required package:** `Regira.IO.Storage.SSH`

---

## AttachmentFileService

Handles physical file operations. Uses an underlying `IFileService`:

```csharp
public class AttachmentFileService<TAttachment, TKey>(IFileService fileService)
{
    Task<byte[]?> GetBytes(TAttachment item)
    Task SaveFile(TAttachment item)
    Task RemoveFile(TAttachment item)
    string GetIdentifier(string fileName)
    string GetRelativeFolder(TAttachment item)
}
```

---

## Implementation Checklist

| Step | Task |
|------|------|
| 1 | Define `ProductAttachment : EntityAttachment<int, int>` |
| 2 | Add `IHasAttachments` + `IHasAttachments<ProductAttachment>` to `Product` |
| 3 | Add `DbSet<Attachment>` and `DbSet<ProductAttachment>` to `DbContext` |
| 4 | Configure relationships in `OnModelCreating` |
| 5 | Create `ProductAttachmentsController : EntityAttachmentControllerBase<...>` |
| 6 | Call `.WithAttachments(...)` with the chosen `IFileService` in DI |
| 7 | Run migration for new `DbSet` |

---

## Guidelines

| Rule | Reason |
|------|--------|
| Always add `DbSet<Attachment>` even if not the first attachment type | Shared table across all entity attachment types |
| Use `IHasAttachments` on the entity for automatic attachment queries | Built-in filter `HasAttachment` becomes available |
| Never include file `Path` in DTOs | Use relative `Identifier` — the file service resolves the full path |
| Use `NewBytes` / `NewFileName` on `IEntityAttachment` for updates | These fields trigger file replacement in the storage backend |

---

## Key Namespaces

```csharp
using Regira.Entities.Attachments.Models;           // EntityAttachment<TKey, TObjectKey>, Attachment
using Regira.Entities.Models.Abstractions;          // IHasAttachments, IHasAttachments<T>
using Regira.Entities.Web.Attachments.Abstractions; // EntityAttachmentControllerBase
using Regira.IO.Storage.FileSystem;                 // BinaryFileService, FileSystemOptions
using Regira.IO.Storage.Abstractions;               // IFileService
using Regira.IO.Storage.Azure;                      // BinaryBlobService, AzureOptions, AzureCommunicator
using Regira.IO.Storage.SSH;                        // SftpService, SftpCommunicator, SftpConfig
```
