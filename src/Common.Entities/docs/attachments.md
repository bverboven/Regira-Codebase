# Entity Attachments

The Attachments module contains 2 main components:
- Attachment: *represents a file stored in the system*
- EntityAttachment: *links an Attachment to an entity (e.g. Product, Article, ...)*

All `EntityAttachments` are linked to the same `Attachment`.

## Attachment

All attachments for all entities are stored in one table.

### Models

```csharp
public interface IAttachment : IBinaryFile, IHasTimestamps;
public interface IAttachment<TKey> : IAttachment, IEntity<TKey>;
```

The Attachment is based on `IBinaryFile` (Part of [Regira.IO](../../../Common.IO.Storage/README.md) module):
- `string? FileName` - The name of a file (not full path)
- `string? Identifier` - Identifier in a specific context (Prefix + Filename)
- `string? Prefix` - The folder structure, except the root folder
- `string? Path` - The full path/Uri for this file
- `string? ContentType` - MIME type of the file
- `long Length` - Size of the file in bytes
- `byte[]? Bytes` - Content as a byte array
- `Stream? Stream` - Content as a stream

### Services

The `AttachmentFileService` handles the physical file storage and retrieval for attachments.

```csharp
public class AttachmentFileService<TAttachment, TKey>(IFileService fileService) : IAttachmentFileService<TAttachment, TKey>
{
    public async Task<byte[]?> GetBytes(TAttachment item, CancellationToken token = default)
    public async Task SaveFile(TAttachment item, CancellationToken token = default)
    public async Task RemoveFile(TAttachment item, CancellationToken token = default)

    public string GetIdentifier(string fileName)
    public string GetRelativeFolder(TAttachment item)
}
```
It uses an underlying `IFileService` to perform the actual file operations.
Useful IFileService implementations:
- `BinaryFileService`: *Local File System*
- `BinaryBlobService`: *Azure Blob Storage*
- `SftpService`: *SFTP/SSH*


## EntityAttachment

```csharp
// (simplified)
public interface IEntityAttachment<TKey, TObjectKey> : IEntityAttachment<TKey, TObjectKey, int, Attachment>;
public interface IEntityAttachment<TKey, TObjectKey, TAttachmentKey> : IEntityAttachment<TKey, TObjectKey, TAttachmentKey, Attachment<TAttachmentKey>>;
public interface IEntityAttachment<TKey, TObjectKey, TAttachmentKey, TAttachment> : IEntity<TKey>, IHasObjectId<TObjectKey>, IEntityAttachment, ISortable
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
{
    string? ObjectType { get; } // Name of owning entity type (e.g. Product, Article, ...)

    // properties used to update existing attachment values
    string? NewFileName { get; set; }
    string? NewContentType { get; set; }
    byte[]? NewBytes { get; set; }

    TAttachmentKey AttachmentId { get; set; }
    new TAttachment? Attachment { get; set; }
}
```

## Implementation

### Owning Entity

After defining the model of the EntityAttachment, 2 interfaces have to be implemented on the Owning Entity:
- `IHasAttachments`
- `IHasAttachments<TEntityAttachment>`

```csharp
// other properties and interfaces are omitted
public class OwningEntity: IHasAttachments, IHasAttachments<MyEntityAttachment>
{
    // ...

    // Add these 3 properties
    public bool? HasAttachment { get; set; }
    public ICollection<MyEntityAttachment>? Attachments { get; set; }
    // implicit interface implementation
    ICollection<IEntityAttachment>? IHasAttachments.Attachments
    {
        get => Attachments?.Cast<IEntityAttachment>().ToArray();
        set => Attachments = value?.Cast<MyEntityAttachment>().ToArray();
    }
}
```

### DbContext

```csharp   
    // Add a DbSet for each EntityAttachment type
    public DbSet<MyEntityAttachment> MyEntityAttachments { get; set; } = null!;

    // Update OnModelCreating
    modelBuilder.Entity<OwningEntity>(entity =>
    {
        entity.HasMany(e => e.Attachments)
            .WithOne()
            .HasForeignKey(e => e.ObjectId)
            .HasPrincipalKey(e => e.Id);
    });
```

### Controllers

The custom EntityAttachmentController must derive from `EntityAttachmentControllerBase`.

```csharp
// using default DTOs (EntityAttachmentDto & EntityAttachmentInputDto))
public class MyEntityAttachmentController : EntityAttachmentControllerBase<MyEntityAttachment>;
// or using custom DTOs
public class MyEntityAttachmentController : EntityAttachmentControllerBase<MyEntityAttachment, MyEntityAttachmentDto, MyEntityAttachmentInputDto>;
```

### Dependency Injection

```csharp
builder.Services
    .UseEntities<MyDbContext>(/*...*/)
    .WithAttachments(_ => new BinaryFileService(
        new FileSystemOptions { 
            RootFolder = ApiConfiguration.AttachmentsDirectory
        }
    ));
```

## Overview

1. [Index](../README.md) — Overview of Regira Entities
1. [Entity Models](models.md) — Creating and structuring entity models
1. [Services](services.md) — Implementing entity services and repositories
1. [Mapping](mapping.md) — Mapping Entities to and from DTOs
1. [Controllers](controllers.md) — Creating Web API controllers
1. [Normalizing](normalizing.md) — Data normalization techniques
1. **[Attachments](attachments.md)** — Managing file attachments
1. [Built-in Features](built-in-features.md) — Ready to use components
1. [Checklist](checklist.md) — Step-by-step guide for common tasks
1. [Practical Examples](examples.md) — Complete implementation examples
