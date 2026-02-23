---
name: Regira Models
description: >
  Creates and updates entity models, SearchObjects, SortBy enums,
  Includes enums, output DTOs, and input DTOs for Regira Entities.
tools:
  - codebase
  - editFiles
handoffs:
  - label: "Models ready → configure services"
    agent: regira-services
    prompt: "The model files are ready. Configure the DI registration (.For<>) and service helpers."
    send: false
---

# Regira Entities — Models Agent

You create and maintain all **model files**: entity classes, SearchObjects,
SortBy enums, Includes enums, and DTOs.

Always use exact namespaces from `.github/instructions/regira-namespaces.instructions.md`.

---

## File Location (Per-Entity Structure)

```
Entities/
└── {EntityName}s/
    ├── {Entity}.cs
    ├── {Entity}SearchObject.cs
    ├── {Entity}SortBy.cs        (optional)
    ├── {Entity}Includes.cs      (optional)
    ├── {Entity}Dto.cs
    └── {Entity}InputDto.cs
```

---

## 1 — Entity Model

```csharp
using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;                    // only when [Normalized] is used

public class {Entity} : IEntityWithSerial,   // int PK (auto-increment)
    IHasTimestamps,                          // audit timestamps
    IHasTitle,                               // display name
    IHasDescription,                         // description field
    IArchivable,                             // soft-delete
    IHasNormalizedContent                    // full-text search support
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    // Populated automatically by normalizer interceptor
    [Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    // Populated automatically by HasCreatedDbPrimer / HasLastModifiedDbPrimer
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    // Set to true by ArchivablePrimer instead of deleting the row
    public bool IsArchived { get; set; }

    // FK + Navigation (add when related entities exist)
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
}
```

### Interface Selection

| Interface | Add when… | Properties required |
|-----------|-----------|-------------------|
| `IEntityWithSerial` | int PK — default | `int Id` |
| `IEntity<TKey>` | non-int PK (e.g. `Guid`) | `TKey Id` |
| `IHasTimestamps` | need Created + LastModified | both datetime fields |
| `IHasCreated` | Created only | `DateTime Created` |
| `IHasLastModified` | LastModified only | `DateTime? LastModified` |
| `IArchivable` | soft-delete | `bool IsArchived` |
| `IHasTitle` | entity has a display name | `string Title` |
| `IHasDescription` | entity has a description | `string? Description` |
| `IHasCode` | entity has a short code | `string? Code` |
| `IHasNormalizedContent` | full-text search | `string? NormalizedContent` |
| `ISortable` | sortable child collection | `int SortOrder` |
| `IHasAttachments` | file attachments | `bool? HasAttachment`, `ICollection<T>? Attachments` |

### [Normalized] Attribute Options

```csharp
// Single source
[Normalized(SourceProperty = nameof(Title))]
public string? NormalizedTitle { get; set; }

// Multiple sources (joined with a space)
[Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
public string? NormalizedContent { get; set; }

// Custom normalizer type
[Normalized(SourceProperty = nameof(Phone), Normalizer = typeof(PhoneNormalizer))]
public string? NormalizedPhone { get; set; }
```

---

## 2 — SearchObject

```csharp
using Regira.Entities.Models;  // SearchObject defaults TKey to int

public class {Entity}SearchObject : SearchObject
{
    // Use ICollection<TKey> for FK filters — enables multi-value filtering
    public ICollection<int>? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    // Inherited from SearchObject<int>:
    //   int? Id
    //   ICollection<int>? Ids
    //   ICollection<int>? Exclude
    //   string? Q              — general text / keyword search
    //   DateTime? MinCreated, MaxCreated
    //   DateTime? MinLastModified, MaxLastModified
    //   bool? IsArchived       — null=active, true=archived only, false=active only
}
```

Use `SearchObject<TKey>` when the entity uses a non-int PK:
```csharp
public class {Entity}SearchObject : SearchObject<Guid> { }
```

Skip SearchObject when only ID-based lookups are needed.

---

## 3 — SortBy Enum

```csharp
// NOT [Flags] — values applied sequentially, not combined
public enum {Entity}SortBy
{
    Default = 0,
    Title,
    TitleDesc,
    Price,
    PriceDesc,
    Created,
    CreatedDesc
}
```

Skip when the default `EntitySortBy` (Id, Created, LastModified) is sufficient.

---

## 4 — Includes Enum

```csharp
// IS [Flags] — combine values with bitwise OR
[Flags]
public enum {Entity}Includes
{
    Default  = 0,
    Category = 1 << 0,
    Reviews  = 1 << 1,
    All      = Category | Reviews
}
```

Skip when the entity has no optional navigation properties.

---

## 5 — Output DTO

```csharp
public class {Entity}Dto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int? CategoryId { get; set; }

    // Prefer navigation properties over flattening — keeps structure rich for clients
    // Use Category.Title on the client side rather than a flat CategoryTitle property
    public CategoryDto? Category { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
```

Rules:
- Include all fields the API consumer needs to display
- **Prefer navigation objects** (`CategoryDto? Category`) over flattened strings (`string? CategoryTitle`)
- Exclude `NormalizedContent` — internal use only
- Exclude `IsArchived` unless consumers need to see the archived state explicitly
- Use `AfterMapper` for computed properties (URLs, display names) that cannot be mapped by Mapster

---

## 6 — Input DTO

```csharp
using System.ComponentModel.DataAnnotations;

public class {Entity}InputDto
{
    // Required for Save (upsert): Id = 0 → create, Id > 0 → update
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public int? CategoryId { get; set; }

    // Include child collections ONLY when configured with e.Related(...)
    // public ICollection<OrderItemInputDto>? OrderItems { get; set; }

    // NEVER include: Created, LastModified, NormalizedContent, IsArchived
}
```

---

## Child Entity Model (for Related collections)

```csharp
using Regira.Entities.Models.Abstractions;

public class OrderItem : IEntityWithSerial, ISortable
{
    public int Id { get; set; }
    public int OrderId { get; set; }     // FK to parent — set in Prepper
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int SortOrder { get; set; }   // required by ISortable
}
```
