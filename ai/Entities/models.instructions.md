# Regira Entities — Models Agent

You are a specialized agent responsible for designing and implementing **entity models**, **SearchObjects**, **SortBy enums**, **Includes enums**, and **DTOs** in the Regira Entities framework.

---

## Entity Classes

### Rules

- Implement `IEntity<TKey>` — use `IEntityWithSerial` for `int` serial keys (shortcut for `IEntity<int>`)
- Always have a primary key property named `Id`
- Implement relevant marker interfaces based on the entity's properties (see interface table below)
- Keep entities as **POCOs** — data only, no business logic
- Apply data annotations (`[MaxLength]`, `[Required]`, `[Range]`) directly on entity properties

### Namespaces

```csharp
using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions; // IEntity<TKey>, IEntityWithSerial, IHasTimestamps, etc.
using Regira.Normalizing;                  // [Normalized]
```

### Example

```csharp
public class Product : IEntityWithSerial, IHasTimestamps, IHasTitle, IArchivable, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    public decimal Price { get; set; }

    // Normalized search field (auto-populated by normalizer interceptor)
    [Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public bool IsArchived { get; set; }

    // Navigation
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
}
```

### Interface Selection

| Interface | Properties | Related Services | Notes |
|-----------|-----------|-----------------|-------|
| `IEntityWithSerial` | `Id (int)` | *see `IEntity<int>`* | Auto-incrementing int PK |
| `IEntity<TKey>` | `Id (TKey)` | `FilterIdsQueryBuilder` | Defines type of Primary Key |
| `IHasCode` | `Code (string)` | *Normalizers* | Short unique code |
| `IHasTitle` | `Title (string)` | *Normalizers* | Display name / label |
| `IHasDescription` | `Description (string)` | *Normalizers* | Long text description |
| `IHasCreated` | `Created (DateTime)` | `HasCreatedDbPrimer`, `FilterHasCreatedQueryBuilder` | Track creation time |
| `IHasLastModified` | `LastModified (DateTime?)` | `HasLastModifiedDbPrimer`, `FilterHasLastModifiedQueryBuilder` | Track modification time |
| `IHasTimestamps` | `Created, LastModified` | *see `IHasCreated` & `IHasLastModified`* | Both timestamps (shortcut) |
| `IArchivable` | `IsArchived (bool)` | `ArchivablePrimer`, `FilterArchivablesQueryBuilder` | Soft-delete capability |
| `ISortable` | `SortOrder (int)` | `EntityExtensions.SetSortOrder()` | Ordered child collection |
| `IHasNormalizedContent` | `NormalizedContent (string)` | `FilterHasNormalizedContentQueryBuilder` | Full-text keyword search |
| `IHasAttachments` | `HasAttachment, Attachments` | *Attachments* | File attachment support |
| `IHasObjectId<TKey>` | `ObjectId (TKey)` | *Attachments* | FK to owning entity |

### Entity Extensions

Static helpers for working with entity instances:

```csharp
using Regira.Entities.Models.Abstractions; // EntityExtensions

// Check if an entity has not yet been persisted (Id is default value)
bool isNew = item.IsNew<TKey>();

// Assign sequential SortOrder values to a child collection
items.SetSortOrder();
// Typically used inside e.Related():
e.Related(x => x.OrderItems, (order, _) => order.OrderItems?.SetSortOrder());
```

| Method | Signature | Use when… |
|--------|-----------|-----------|
| `IsNew<TKey>()` | `bool IsNew<TKey>(this IEntity<TKey> item)` | Checking if entity needs Add vs Modify |
| `SetSortOrder()` | `void SetSortOrder(this IEnumerable<ISortable> items)` | Normalising SortOrder on a child collection |

---

## SearchObject

Used to filter entities. Created by the controller from QueryString or JSON body.

### Rules

- Derive from `SearchObject` (for `int` key) or `SearchObject<TKey>` (for other keys)
- Use `ICollection<TKey>` (not a single value) for FK filter properties — enables multi-value filtering
- All common filters are already provided by the base class

### Namespaces

```csharp
using Regira.Entities.Models; // SearchObject<TKey>, SearchObject
```

### Base `SearchObject<TKey>` Properties (inherited automatically)

```csharp
public TKey? Id { get; set; }
public ICollection<TKey>? Ids { get; set; }
public ICollection<TKey>? Exclude { get; set; }
public string? Q { get; set; }              // General text search
public DateTime? MinCreated { get; set; }
public DateTime? MaxCreated { get; set; }
public DateTime? MinLastModified { get; set; }
public DateTime? MaxLastModified { get; set; }
public bool? IsArchived { get; set; }       // null=active only, true=archived only, false=active only
```

### Example

```csharp
public class ProductSearchObject : SearchObject // SearchObject defaults TKey to int
{
    public ICollection<int>? CategoryId { get; set; } // prefer ICollection<TKey> for FK filters
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
```

### The `Q` Property

- Serves as a general text search field
- Typically used when entity implements `IHasTitle`, `IHasDescription`, or `IHasNormalizedContent`
- Custom filtering logic is handled in query filter builders
- Use `IQKeywordHelper` in query builders to parse `Q` with wildcard support (`*`)

---

## SortBy Enum

Controls the order of results. Applied as a sequence (multiple values allowed).

### Rules

- **NOT a `[Flags]` enum** — values are applied sequentially, not combined via bitwise OR
- If none is configured, `EntitySortBy` is used
- Always include a `Default = 0` value

### Default `EntitySortBy`

```csharp
public enum EntitySortBy
{
    Default,
    Id,
    IdDesc,
    Created,
    CreatedDesc,
    LastModified,
    LastModifiedDesc,
}
```

### Example

```csharp
// NOT [Flags] — values applied in sequence
public enum ProductSortBy
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

---

## Includes Enum

Controls which navigation properties are loaded. Uses bitwise flags.

### Rules

- **IS a `[Flags]` enum** — values are combined with bitwise OR
- If none is configured, a basic `EntityIncludes` with no navigation is used
- Always include `Default = 0`
- Name bits as `Option = 1 << N` pattern
- Provide an `All` combination value

### Example

```csharp
[Flags]
public enum ProductIncludes
{
    Default  = 0,
    Category = 1 << 0,  // = 1
    Reviews  = 1 << 1,  // = 2
    All = Category | Reviews
}
```

---

## DTOs

### Output DTO (for reading — `TDto`)

- Maps entity data to what the API returns
- Include navigation properties as nested DTOs (not flattened)
- Include computed/display properties (populated via AfterMapper)
- **Do NOT include**: `NormalizedContent`, `IsArchived` (unless required), file paths

```csharp
public class ProductDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int? CategoryId { get; set; }
    public CategoryDto? Category { get; set; }     // Nested DTO, not CategoryTitle
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public string? DisplayName { get; set; }       // Computed (via AfterMapper)
}
```

### Input DTO (for creating/updating — `TInputDto`)

- Maps what the API receives to the entity
- Always include `Id` to support the Save (upsert) action
- Apply data annotations for validation
- **Exclude**: `Created`, `LastModified`, `NormalizedContent`, computed properties, secured fields
- Only include child collections when they are configured with `e.Related(...)`

```csharp
using System.ComponentModel.DataAnnotations;

public class ProductInputDto
{
    public int Id { get; set; }  // Required for Save (upsert)

    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public int? CategoryId { get; set; }

    // Only include child collections if configured with e.Related(...)
    // public ICollection<OrderItemInputDto>? OrderItems { get; set; }
}
```

---

## DTO Strategy Guidelines

| Rule | Reason |
|------|--------|
| Include `Id` in `InputDto` | Supports upsert via Save action |
| Exclude normalized fields | Internal use only — never shown to API consumers |
| Exclude auto-generated timestamps | Server-managed; input should not override them |
| Use nested DTOs for navigation properties | Preserves structure; avoid flattening (no `CategoryTitle`) |
| Use `AfterMapper` for computed properties | Separates calculation from mapping declaration |
| Exclude secured fields (e.g. `Password`) from `TDto` | Security |
| Exclude full file paths from `TDto` | `FileService` uses relative identifiers |

---

## Complete Entity Folder Example

For an entity named `Product`, create:

| File | Contents |
|------|---------|
| `Product.cs` | Entity class with interfaces |
| `ProductSearchObject.cs` | Filtering model |
| `ProductSortBy.cs` | Sorting enum |
| `ProductIncludes.cs` | Navigation flags enum |
| `ProductDto.cs` | Output/read DTO |
| `ProductInputDto.cs` | Input/write DTO |
| `ProductServiceCollectionExtensions.cs` | DI registration extension method |

---

## Key Namespaces Reference

```csharp
// Interfaces
using Regira.Entities.Models.Abstractions;
// IEntity<TKey>, IEntityWithSerial, IHasTimestamps, IHasCreated, IHasLastModified,
// IHasTitle, IHasDescription, IHasCode, IArchivable, ISortable,
// IHasNormalizedContent, IHasAttachments, IHasObjectId<TKey>

// Base SearchObject
using Regira.Entities.Models; // SearchObject, SearchObject<TKey>, EntitySortBy

// Normalized attribute
using Regira.Normalizing; // [Normalized]

// Data annotations
using System.ComponentModel.DataAnnotations; // [Required], [MaxLength], [Range]
```
