# Regira Entities Framework - API Signatures Reference

This document provides a comprehensive reference of all interfaces, classes, and extension methods used in the Regira Entities framework.

---

## Table of Contents

1. [Entity Interfaces](#entity-interfaces)
2. [Service Interfaces](#service-interfaces)
3. [Controller Base Classes](#controller-base-classes)
4. [Search and Filter Objects](#search-and-filter-objects)
5. [Extension Methods](#extension-methods)
6. [Service Builders](#service-builders)
7. [Mapping and Processing](#mapping-and-processing)
8. [Response Types](#response-types)
9. [Attachments](#attachments)
10. [Exceptions](#exceptions)
11. [Supporting Types](#supporting-types)

---

## Entity Interfaces

### Core Entity Interfaces

**Namespace:** `Regira.Entities.Models.Abstractions`

```csharp
// Base entity marker interface
public interface IEntity;

// Entity with typed primary key
public interface IEntity<TKey> : IEntity
{
    TKey Id { get; set; }
}

// Shortcut for entities with int primary key
public interface IEntityWithSerial : IEntity<int>
{
    // Inherits: int Id { get; set; }
}
```

### Property Interfaces

**Namespace:** `Regira.Entities.Models.Abstractions`

```csharp
// Code property
public interface IHasCode
{
    string? Code { get; set; }
}

// Title property (read-only)
public interface IHasTitle
{
    string? Title { get; }
}

// Normalized title
public interface IHasNormalizedTitle : IHasTitle
{
    string? NormalizedTitle { get; set; }
}

// Description property
public interface IHasDescription
{
    string? Description { get; set; }
}

// Normalized content for search
public interface IHasNormalizedContent
{
    string? NormalizedContent { get; set; }
}

// Normalized content with tracking
public interface IHasLastNormalized : IHasNormalizedContent, IHasLastModified
{
    DateTime? LastNormalized { get; set; }
}

// Sort order for child collections
public interface ISortable
{
    int SortOrder { get; set; }
}
```

### Timestamp Interfaces

**Namespace:** `Regira.Entities.Models.Abstractions`

```csharp
// Creation timestamp
public interface IHasCreated
{
    DateTime Created { get; set; }
}

// Last modified timestamp
public interface IHasLastModified
{
    DateTime? LastModified { get; set; }
}

// Combined timestamps
public interface IHasTimestamps : IHasCreated, IHasLastModified;
```

### Lifecycle Interfaces

**Namespace:** `Regira.Entities.Models.Abstractions`

```csharp
// Soft-delete support
public interface IArchivable
{
    bool IsArchived { get; set; }
}
```

### Attachment Interfaces

**Namespace:** `Regira.Entities.Attachments.Abstractions`

```csharp
// Basic attachments support
public interface IHasAttachments
{
    ICollection<IEntityAttachment>? Attachments { get; set; }
    bool? HasAttachment { get; set; }
}

// Typed attachments (with int keys)
public interface IHasAttachments<TEntityAttachment> 
    : IHasAttachments<TEntityAttachment, int, int, int, Attachment>
    where TEntityAttachment : IEntityAttachment<int, int, int, Attachment>;

// Fully typed attachments
public interface IHasAttachments<TEntityAttachment, TKey, TObjectKey, TAttachmentKey, TAttachment>
    where TEntityAttachment : IEntityAttachment<TKey, TObjectKey, TAttachmentKey, TAttachment>
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
{
    ICollection<TEntityAttachment>? Attachments { get; set; }
    bool? HasAttachment { get; set; }
}
```

---

## Service Interfaces

### Read Service Interfaces

**Namespace:** `Regira.Entities.Services.Abstractions`

```csharp
// Basic read operations
public interface IEntityReadService<TEntity, in TKey>
{
    Task<TEntity?> Details(TKey id);
    Task<IList<TEntity>> List(object? so = null, PagingInfo? pagingInfo = null);
    Task<long> Count(object? so);
}

// With typed SearchObject
public interface IEntityReadService<TEntity, in TKey, in TSearchObject> 
    : IEntityReadService<TEntity, TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
{
    Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null);
    Task<long> Count(TSearchObject? so);
}
```

// Full-featured (with sorting, includes)
public interface IEntityReadService<TEntity, in TKey, TSearchObject, TSortBy, TIncludes> 
    : IEntityReadService<TEntity, TKey, TSearchObject>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    Task<IList<TEntity>> List(
        IList<TSearchObject?> so, 
        IList<TSortBy> sortBy, 
        TIncludes? includes = null, 
        PagingInfo? pagingInfo = null);
    Task<long> Count(IList<TSearchObject?> so);
}
```

### Write Service Interfaces

**Namespace:** `Regira.Entities.Services.Abstractions`

```csharp
// Write operations
public interface IEntityWriteService<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    Task Add(TEntity item);
    Task<TEntity?> Modify(TEntity item);
    Task Save(TEntity item);           // Upsert: Add or Modify
    Task Remove(TEntity item);
    Task<int> SaveChanges(CancellationToken token = default);
}
```

### Combined Service Interfaces

**Namespace:** `Regira.Entities.Services.Abstractions`

```csharp
// Basic combined service (with TKey)
public interface IEntityService<TEntity, TKey> 
    : IEntityReadService<TEntity, TKey>, 
      IEntityWriteService<TEntity, TKey>
    where TEntity : class, IEntity<TKey>;

// With SearchObject
public interface IEntityService<TEntity, TKey, in TSearchObject>
    : IEntityReadService<TEntity, TKey, TSearchObject>, 
      IEntityService<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new();

// Full-featured
public interface IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    : IEntityReadService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, 
      IEntityService<TEntity, TKey, TSearchObject>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;

// Shortcut variants (int key)
public interface IEntityService<TEntity> 
    : IEntityService<TEntity, int>
    where TEntity : class, IEntity<int>;

public interface IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>
    : IEntityService<TEntity, int, TSearchObject, TSortBy, TIncludes>, 
      IEntityService<TEntity>
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;
```

### IEntityRepository Interfaces

**Namespace:** `Regira.Entities.Services.Abstractions`

```csharp
public interface IEntityRepository<TEntity> 
    : IEntityRepository<TEntity, int>, IEntityService<TEntity>
    where TEntity : class, IEntity<int>;

public interface IEntityRepository<TEntity, TKey> 
    : IEntityService<TEntity, TKey>
    where TEntity : class, IEntity<TKey>;

public interface IEntityRepository<TEntity, TKey, in TSearchObject>
    : IEntityService<TEntity, TKey, TSearchObject>, IEntityRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new();

// Shortcut (int key, full-featured)
public interface IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>
    : IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>,
      IEntityRepository<TEntity, int, TSearchObject, TSortBy, TIncludes>,
      IEntityRepository<TEntity>
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;

public interface IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    : IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>,
      IEntityRepository<TEntity, TKey, TSearchObject>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;
```

### IEntityManager Interfaces

**Namespace:** `Regira.Entities.Services.Abstractions`

```csharp
public interface IEntityManager<TEntity> 
    : IEntityManager<TEntity, int>, IEntityService<TEntity>
    where TEntity : class, IEntity<int>;

public interface IEntityManager<TEntity, TKey> 
    : IEntityService<TEntity, TKey>
    where TEntity : class, IEntity<TKey>;

public interface IEntityManager<TEntity, TKey, in TSearchObject>
    : IEntityService<TEntity, TKey, TSearchObject>, IEntityManager<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new();

// Shortcut (int key, full-featured)
public interface IEntityManager<TEntity, TSearchObject, TSortBy, TIncludes>
    : IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>,
      IEntityManager<TEntity, int, TSearchObject, TSortBy, TIncludes>,
      IEntityManager<TEntity>
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;

public interface IEntityManager<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    : IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>,
      IEntityManager<TEntity, TKey, TSearchObject>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;
```

### Wrapping Service Base Classes

**Namespace:** `Regira.Entities.Services.Abstractions`

```csharp
// Basic wrapper (int key)
public abstract class EntityWrappingServiceBase<TEntity>(IEntityService<TEntity, int, SearchObject<int>> service)
    : EntityWrappingServiceBase<TEntity, int, SearchObject<int>>(service), 
      IEntityService<TEntity>
    where TEntity : class, IEntity<int>;

// With TKey
public abstract class EntityWrappingServiceBase<TEntity, TKey>(
    IEntityService<TEntity, TKey, SearchObject<TKey>> service)
    : EntityWrappingServiceBase<TEntity, TKey, SearchObject<TKey>>(service), 
      IEntityService<TEntity, TKey>
    where TEntity : class, IEntity<TKey>;

// With SearchObject
public abstract class EntityWrappingServiceBase<TEntity, TKey, TSearchObject>(
    IEntityService<TEntity, TKey, TSearchObject> service) 
    : IEntityService<TEntity, TKey, TSearchObject>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
{
    protected readonly IEntityService<TEntity, TKey, TSearchObject> Service = service;

    // Read operations
    public virtual Task<TEntity?> Details(TKey id);
    public virtual Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null);
    public virtual Task<IList<TEntity>> List(object? so, PagingInfo? pagingInfo);
    public virtual Task<long> Count(TSearchObject? so);
    public virtual Task<long> Count(object? so);

    // Write operations
    public virtual Task Add(TEntity item);
    public virtual Task<TEntity?> Modify(TEntity item);
    public virtual Task Save(TEntity item);
    public virtual Task Remove(TEntity item);
    public virtual Task<int> SaveChanges(CancellationToken token = default);
}

// Full-featured wrapper (int key)
public abstract class EntityWrappingServiceBase<TEntity, TSearchObject, TSortBy, TIncludes>(
    IEntityService<TEntity, int, TSearchObject, TSortBy, TIncludes> service)
    : EntityWrappingServiceBase<TEntity, int, TSearchObject, TSortBy, TIncludes>(service)
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;

// Full-featured wrapper (with TKey)
public abstract class EntityWrappingServiceBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes>(
    IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes> service)
    : IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    // Read operations
    public virtual Task<TEntity?> Details(TKey id);
    public virtual Task<IList<TEntity>> List(object? so = null, PagingInfo? pagingInfo = null);
    public Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null);
    public virtual Task<IList<TEntity>> List(
        IList<TSearchObject?> so, 
        IList<TSortBy> sortBy, 
        TIncludes? includes = null, 
        PagingInfo? pagingInfo = null);
    public virtual Task<long> Count(object? so);
    public Task<long> Count(TSearchObject? so);
    public virtual Task<long> Count(IList<TSearchObject?> so);

    // Write operations
    public virtual Task Add(TEntity item);
    public virtual Task<TEntity?> Modify(TEntity item);
    public virtual Task Save(TEntity item);
    public virtual Task Remove(TEntity item);
    public virtual Task<int> SaveChanges(CancellationToken token = default);

    // Utilities
    public virtual TSearchObject? Convert(object? so);
}
```

---

## Controller Base Classes

**Namespace:** `Regira.Entities.Web.Controllers.Abstractions`

### Basic Controllers (No DTO)

```csharp
// Minimal (int key, no DTO)
public abstract class EntityControllerBase<TEntity> 
    : EntityControllerBase<TEntity, SearchObject, TEntity, TEntity>
    where TEntity : class, IEntity<int>;

// With TKey (no DTO)
public abstract class EntityControllerBase<TEntity, TKey> 
    : EntityControllerBase<TEntity, TKey, SearchObject<TKey>, TEntity, TEntity>
    where TEntity : class, IEntity<TKey>;
```

### Standard Controllers (With DTO)

```csharp
// Basic with DTO (int key)
public abstract class EntityControllerBase<TEntity, TDto, TInputDto> 
    : EntityControllerBase<TEntity, SearchObject, TDto, TInputDto>
    where TEntity : class, IEntity<int>
    where TDto : class
    where TInputDto : class;

// With SearchObject and DTO (int key)
public abstract class EntityControllerBase<TEntity, TSearchObject, TDto, TInputDto> 
    : EntityControllerBase<TEntity, int, TSearchObject, TDto, TInputDto>
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>
    where TDto : class
    where TInputDto : class;

// With TKey, SearchObject, and DTO
[ApiController]
public abstract class EntityControllerBase<TEntity, TKey, TSearchObject, TDto, TInputDto> 
    : ControllerBase
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>
    where TDto : class
    where TInputDto : class
{
    // Details
    [HttpGet("{id}")]
    public virtual Task<ActionResult<DetailsResult<TDto>>> Details([FromRoute] TKey id);

    // List
    [HttpGet]
    public virtual Task<ActionResult<ListResult<TDto>>> List(
        [FromQuery] TSearchObject so, 
        [FromQuery] PagingInfo pagingInfo);

    // Save (upsert)
    [HttpPost("save")]
    public virtual Task<ActionResult<SaveResult<TDto>>> Save([FromBody] TInputDto model);

    // Create
    [HttpPost]
    public virtual Task<ActionResult<SaveResult<TDto>>> Create([FromBody] TInputDto model);

    // Modify
    [HttpPut("{id}")]
    public virtual Task<ActionResult<SaveResult<TDto>>> Modify(
        [FromRoute] TKey id, 
        [FromBody] TInputDto model);

    // Delete
    [HttpDelete("{id}")]
    public virtual Task<ActionResult<DeleteResult<TDto>>?> Delete([FromRoute] TKey id);
}
```

### Full-Featured Controllers

```csharp
// With sorting, includes (int key)
public abstract class EntityControllerBase<TEntity, TSo, TSortBy, TIncludes, TDto, TInputDto> 
    : EntityControllerBase<TEntity, int, TSo, TSortBy, TIncludes, TDto, TInputDto>
    where TEntity : class, IEntity<int>
    where TSo : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
    where TDto : class
    where TInputDto : class;

// With TKey, sorting, includes
[ApiController]
public abstract class EntityControllerBase<TEntity, TKey, TSo, TSortBy, TIncludes, TDto, TInputDto> 
    : ControllerBase
    where TEntity : class, IEntity<TKey>
    where TSo : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
    where TDto : class
    where TInputDto : class
{
    // Details
    [HttpGet("{id}")]
    public virtual Task<ActionResult<DetailsResult<TDto>>> Details([FromRoute] TKey id);

    // List (GET)
    [HttpGet]
    public virtual Task<ActionResult<ListResult<TDto>>> List(
        [FromQuery] TSo so,
        [FromQuery] PagingInfo pagingInfo, 
        [FromQuery] TIncludes[] includes, 
        [FromQuery] TSortBy[] sortBy);

    // List (POST)
    [HttpPost("list")]
    public virtual Task<ActionResult<ListResult<TDto>>> List(
        [FromBody] TSo[] so,
        [FromQuery] PagingInfo pagingInfo, 
        [FromQuery] TIncludes[] includes, 
        [FromQuery] TSortBy[] sortBy);

    // Search (GET)
    [HttpGet("search")]
    public virtual Task<ActionResult<SearchResult<TDto>>> Search(
        [FromQuery] TSo so,
        [FromQuery] PagingInfo pagingInfo, 
        [FromQuery] TIncludes[] includes,
        [FromQuery] TSortBy[] sortBy);

    // Search (POST)
    [HttpPost("search")]
    public virtual Task<ActionResult<SearchResult<TDto>>> Search(
        [FromBody] TSo[] so,
        [FromQuery] PagingInfo pagingInfo, 
        [FromQuery] TIncludes[] includes, 
        [FromQuery] TSortBy[] sortBy);

    // Save (upsert)
    [HttpPost("save")]
    public virtual Task<ActionResult<SaveResult<TDto>>> Save([FromBody] TInputDto model);

    // Create
    [HttpPost]
    public virtual Task<ActionResult<SaveResult<TDto>>> Create([FromBody] TInputDto model);

    // Modify
    [HttpPut("{id}")]
    public virtual Task<ActionResult<SaveResult<TDto>>> Modify(
        [FromRoute] TKey id, 
        [FromBody] TInputDto model);

    // Delete
    [HttpDelete("{id}")]
    public virtual Task<ActionResult<DeleteResult<TDto>>?> Delete([FromRoute] TKey id);
}
```

---

## Search and Filter Objects

### ISearchObject Interface

**Namespace:** `Regira.Entities.Models.Abstractions`

```csharp
// Base search object interface
public interface ISearchObject
{
    string? Q { get; set; }

    DateTime? MinCreated { get; set; }
    DateTime? MaxCreated { get; set; }
    DateTime? MinLastModified { get; set; }
    DateTime? MaxLastModified { get; set; }

    bool? IsArchived { get; set; }
}

// With typed ID
public interface ISearchObject<TKey> : ISearchObject
{
    TKey? Id { get; set; }
    ICollection<TKey>? Ids { get; set; }
    ICollection<TKey>? Exclude { get; set; }
}
```

### SearchObject Implementation

**Namespace:** `Regira.Entities.Models`

```csharp
// Default SearchObject (int key)
public class SearchObject : SearchObject<int>;

// Generic SearchObject
public class SearchObject<TKey> : ISearchObject<TKey>
{
    public TKey? Id { get; set; }
    public ICollection<TKey>? Ids { get; set; }
    public ICollection<TKey>? Exclude { get; set; }
    public string? Q { get; set; }

    public DateTime? MinCreated { get; set; }
    public DateTime? MaxCreated { get; set; }
    public DateTime? MinLastModified { get; set; }
    public DateTime? MaxLastModified { get; set; }

    public bool? IsArchived { get; set; }
}
```

---

## Extension Methods

### Entity Extensions

**Namespace:** `Regira.Entities.Extensions`  
**Class:** `EntityExtensions`

```csharp
public static class EntityExtensions
{
    // Check if entity is new (Id is default)
    public static bool IsNew<TKey>(this IEntity<TKey> item);

    // Adjust IDs for EF Core (negative IDs → 0)
    public static void AdjustIdForEfCore(this IEnumerable<IEntity<int>> items);

    // Set sort order for sortable items
    public static void SetSortOrder(this IEnumerable<ISortable> items);
}
```

### Query Extensions

**Namespace:** `Regira.Entities.EFcore.Extensions`  
**Class:** `QueryExtensions`

> **Interface constraints:** Every `QueryExtensions` method enforces a compile-time `where TEntity : <Interface>` constraint.
> Your entity **must implement the listed interface** for the method to be available.
> If the entity does not implement the interface, use inline LINQ instead — e.g.:
> - `query.FilterCode(so.Code)` → requires `IHasCode`; alternative: `query.Where(x => x.Code == so.Code)`
> - `query.FilterTitle(keywords)` → requires `IHasTitle`; alternative: `query.Where(x => x.Title!.Contains(term))`
> - `query.FilterQ(keywords)` → requires `IHasNormalizedContent`; alternative: `query.Where(x => EF.Functions.Like(x.NormalizedContent, kw))`

```csharp
public static class QueryExtensions
{
    // Filter by single ID
    public static IQueryable<TEntity> FilterId<TEntity, TKey>(
        this IQueryable<TEntity> query, 
        TKey? id)
        where TEntity : IEntity<TKey>;

    // Filter by multiple IDs
    public static IQueryable<TEntity> FilterIds<TEntity, TKey>(
        this IQueryable<TEntity> query, 
        ICollection<TKey>? ids)
        where TEntity : IEntity<TKey>;

    // Exclude specific IDs
    public static IQueryable<TEntity> FilterExclude<TEntity, TKey>(
        this IQueryable<TEntity> query, 
        ICollection<TKey>? ids)
        where TEntity : IEntity<TKey>;

    // Filter by Code
    public static IQueryable<TEntity> FilterCode<TEntity>(
        this IQueryable<TEntity> query, 
        string? code)
        where TEntity : IHasCode;

    // Filter by Title (with keywords)
    public static IQueryable<TEntity> FilterTitle<TEntity>(
        this IQueryable<TEntity> query, 
        ParsedKeywordCollection? keywords)
        where TEntity : IHasTitle;

    // Filter by Normalized Title
    public static IQueryable<TEntity> FilterNormalizedTitle<TEntity>(
        this IQueryable<TEntity> query, 
        ParsedKeywordCollection? keywords)
        where TEntity : IHasNormalizedTitle;

    // Filter by Created date range
    public static IQueryable<TEntity> FilterCreated<TEntity>(
        this IQueryable<TEntity> query, 
        DateTime? minDate, 
        DateTime? maxDate)
        where TEntity : IHasCreated;

    // Filter by LastModified date range
    public static IQueryable<TEntity> FilterLastModified<TEntity>(
        this IQueryable<TEntity> query, 
        DateTime? minDate, 
        DateTime? maxDate)
        where TEntity : IHasLastModified;

    // Filter by both timestamp ranges
    public static IQueryable<TEntity> FilterTimestamps<TEntity>(
        this IQueryable<TEntity> query, 
        DateTime? minCreated, 
        DateTime? maxCreated, 
        DateTime? minModified, 
        DateTime? maxModified)
        where TEntity : IHasTimestamps;

    // Filter by Q (full-text search on NormalizedContent)
    public static IQueryable<TEntity> FilterQ<TEntity>(
        this IQueryable<TEntity> query, 
        ParsedKeywordCollection? keywords)
        where TEntity : IHasNormalizedContent;

    // Filter archived items
    public static IQueryable<TEntity> FilterArchivable<TEntity>(
        this IQueryable<TEntity> query, 
        bool? isArchived)
        where TEntity : IArchivable;

    // Filter by attachment presence
    public static IQueryable<TEntity> FilterHasAttachment<TEntity>(
        this IQueryable<TEntity> query, 
        bool? hasAttachment)
        where TEntity : IHasAttachments;

    // Apply default sorting
    public static IQueryable<TEntity> SortQuery<TEntity, TKey>(
        this IQueryable<TEntity> query)
        where TEntity : IEntity<TKey>;
}
```

### Controller Extensions

**Namespace:** `Regira.Entities.Web.Controllers`  
**Class:** `ControllerExtensions`

```csharp
public static class ControllerExtensions
{
    // Details helpers
    public static OkObjectResult DetailsResult<TDto>(
        this ControllerBase _, 
        TDto item, 
        long? duration = null);

    public static Task<ActionResult<DetailsResult<TDto>>?> Details<TEntity, TDto>(
        this ControllerBase ctrl, 
        int id)
        where TEntity : class, IEntity<int>;

    public static Task<ActionResult<DetailsResult<TDto>>?> Details<TEntity, TKey, TDto>(
        this ControllerBase ctrl, 
        TKey id)
        where TEntity : class, IEntity<TKey>;

    // List helpers
    public static OkObjectResult ListResult<TDto>(
        this ControllerBase _, 
        IList<TDto> items, 
        long? duration = null);

    // Simple list
    public static Task<ActionResult<ListResult<TDto>>> List<TEntity, TKey, TSearchObject, TDto>(
        this ControllerBase ctrl, 
        TSearchObject? so = null, 
        PagingInfo? pagingInfo = null)
        where TEntity : class, IEntity<TKey>
        where TSearchObject : class, ISearchObject<TKey>;

    // Complex list (with sorting and includes)
    public static Task<ActionResult<ListResult<TDto>>> List<TEntity, TKey, TSo, TSortBy, TIncludes, TDto>(
        this ControllerBase ctrl,
        TSo[] so, 
        PagingInfo pagingInfo, 
        TIncludes[] includes, 
        TSortBy[] sortBy)
        where TEntity : class, IEntity<TKey>
        where TSo : class, ISearchObject<TKey>, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum;

    // Search helpers
    public static OkObjectResult SearchResult<TDto>(
        this ControllerBase _, 
        IList<TDto> items, 
        long count, 
        long? duration = null);

    // Simple search
    public static Task<ActionResult<SearchResult<TDto>>> Search<TEntity, TKey, TDto>(
        this ControllerBase ctrl, 
        SearchObject<TKey>? so = null, 
        PagingInfo? pagingInfo = null)
        where TEntity : class, IEntity<TKey>;

    // Complex search (with sorting and includes)
    public static Task<ActionResult<SearchResult<TDto>>> Search<TEntity, TKey, TSo, TSortBy, TIncludes, TDto>(
        this ControllerBase ctrl,
        TSo[] so, 
        PagingInfo pagingInfo, 
        TIncludes[] includes, 
        TSortBy[] sortBy)
        where TEntity : class, IEntity<TKey>
        where TSo : class, ISearchObject<TKey>, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum;

    // Save helpers
    public static OkObjectResult SaveResult<TDto>(
        this ControllerBase _, 
        TDto item, 
        int affected,
        bool isNew, 
        long? duration = null);

    public static Task<ActionResult<SaveResult<TDto>>?> Save<TEntity, TKey, TDto, TInputDto>(
        this ControllerBase ctrl, 
        TInputDto model, 
        TKey? id = default)
        where TEntity : class, IEntity<TKey>
        where TInputDto : class;

    // Delete helpers
    public static OkObjectResult DeleteResult<TDto>(
        this ControllerBase _, 
        TDto item, 
        long? duration = null);

    public static Task<ActionResult<DeleteResult<TDto>>?> Delete<TEntity, TKey, TDto>(
        this ControllerBase ctrl, 
        TKey id)
        where TEntity : class, IEntity<TKey>;
}
```

---

## Service Builders

### Top-Level DI Entry Points

**Namespace:** `Regira.Entities.DependencyInjection.ServiceBuilders.Extensions`

```csharp
// Entry point: creates an EntityServiceCollection for fluent entity registration
public static EntityServiceCollection<TContext> UseEntities<TContext>(
    this IServiceCollection services,
    Action<EntityServiceCollectionOptions>? configure = null)
    where TContext : DbContext;
```

---

### EntityServiceCollectionOptions Extension Methods

The `EntityServiceCollectionOptions` object is passed to the `UseEntities` configure lambda. All methods below are **extension methods** on `EntityServiceCollectionOptions` from the listed namespaces.

#### Setup Helpers

**Namespace:** `Regira.Entities.DependencyInjection.ServiceBuilders.Extensions`

```csharp
// Convenience method: registers default primers, normalizer services,
// FilterHasNormalizedContentQueryBuilder, and all default global query filters.
// Equivalent to calling AddDefaultPrimers() + AddDefaultEntityNormalizer()
//   + AddGlobalFilterQueryBuilder<FilterHasNormalizedContentQueryBuilder>()
//   + AddDefaultGlobalQueryFilters()
public static EntityServiceCollectionOptions UseDefaults(
    this EntityServiceCollectionOptions options,
    Action<EntityDefaultNormalizingOptions>? configure = null);

// Registers only the default normalizer services (no primers, no query filters).
// Use when you want fine-grained control instead of UseDefaults().
public static EntityServiceCollectionOptions UseNormalizerDefaults(
    this EntityServiceCollectionOptions options,
    Action<EntityDefaultNormalizingOptions>? configure = null);
```

#### Mapping

**Namespace:** `Regira.Entities.Mapping.Mapster` (package: `Regira.Entities.Mapping.Mapster`)

```csharp
// Registers Mapster as the IEntityMapper. Default mapping choice.
public static EntityServiceCollectionOptions UseMapsterMapping(
    this EntityServiceCollectionOptions options,
    Action<TypeAdapterConfig>? configure = null);
```

**Namespace:** `Regira.Entities.Mapping.AutoMapper` (package: `Regira.Entities.Mapping.AutoMapper`)

```csharp
// Registers AutoMapper as the IEntityMapper.
public static EntityServiceCollectionOptions UseAutoMapper(
    this EntityServiceCollectionOptions options,
    Action<IServiceProvider, IMapperConfigurationExpression>? configure = null);
```

**Namespace:** `Regira.Entities.DependencyInjection.Mapping`

```csharp
// Register a global after-mapper class (runs after every Entity→DTO mapping
// where CanMap() returns true).
public static EntityServiceCollectionOptions AddAfterMapper<TAfterMapper>(
    this EntityServiceCollectionOptions options)
    where TAfterMapper : class, IEntityAfterMapper;

// Register a global inline after-mapper for a specific source/target pair.
public static EntityServiceCollectionOptions AfterMap<TSource, TTarget>(
    this EntityServiceCollectionOptions options,
    Action<TSource, TTarget> afterMapAction);

public static EntityServiceCollectionOptions AfterMap<TSource, TTarget>(
    this EntityServiceCollectionOptions options,
    Func<IServiceProvider, Action<TSource, TTarget>> afterMapAction);
```

#### Preppers (global, run before SaveChanges for matching entities)

**Namespace:** `Regira.Entities.DependencyInjection.Preppers`

```csharp
// Register a global prepper class — runs for every entity that passes CanPrepare().
public static EntityServiceCollectionOptions AddPrepper<TImplementation>(
    this EntityServiceCollectionOptions options)
    where TImplementation : class, IEntityPrepper;

// Register a global prepper class scoped to a specific entity interface/type.
public static EntityServiceCollectionOptions AddPrepper<TImplementation, TKey>(
    this EntityServiceCollectionOptions options)
    where TImplementation : class, IEntityPrepper<TKey>;

// Register an inline prepper for entities implementing a specific interface/type.
// TEntity can be an interface (e.g. IHasAggregateKey) — runs for all entities that implement it.
public static EntityServiceCollectionOptions AddPrepper<TEntity>(
    this EntityServiceCollectionOptions options,
    Action<TEntity> prepareFunc)
    where TEntity : class;

// Register an inline async prepper that receives the DbContext.
public static EntityServiceCollectionOptions AddPrepper<TContext, TEntity, TKey>(
    this EntityServiceCollectionOptions options,
    Func<TEntity, TContext, Task> prepareFunc)
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>;
```

#### Normalizers (global, run on save for matching entities)

**Namespace:** `Regira.Entities.DependencyInjection.Normalizers`

```csharp
// Register a global entity normalizer class.
public static EntityServiceCollectionOptions AddNormalizer<TNormalizer>(
    this EntityServiceCollectionOptions options)
    where TNormalizer : class, IEntityNormalizer;

// Register a global entity normalizer scoped to a specific entity interface/type.
public static EntityServiceCollectionOptions AddNormalizer<TEntity, TNormalizer>(
    this EntityServiceCollectionOptions options)
    where TNormalizer : class, IEntityNormalizer<TEntity>;

// Register the default normalizer stack (DefaultNormalizer, ObjectNormalizer,
// DefaultEntityNormalizer, QKeywordHelper). Called automatically by UseDefaults().
public static EntityServiceCollectionOptions AddDefaultEntityNormalizer(
    this EntityServiceCollectionOptions options,
    Action<NormalizeOptions>? configure = null);
```

#### Primers (global, run inside EF Core SaveChanges interceptor)

**Namespace:** `Regira.Entities.DependencyInjection.Primers`

```csharp
// Register a global primer class.
public static EntityServiceCollectionOptions AddPrimer<TPrimer>(
    this EntityServiceCollectionOptions options)
    where TPrimer : class, IEntityPrimer;

// Register a global primer scoped to a specific entity interface/type.
public static EntityServiceCollectionOptions AddPrimer<TPrimer, TKey>(
    this EntityServiceCollectionOptions options)
    where TPrimer : class, IEntityPrimer<TKey>;

// Register the default primer set (ArchivablePrimer, HasCreatedDbPrimer,
// HasLastModifiedDbPrimer). Called automatically by UseDefaults().
public static EntityServiceCollectionOptions AddDefaultPrimers(
    this EntityServiceCollectionOptions options);
```

#### Global Filter Query Builders

**Namespace:** `Regira.Entities.DependencyInjection.QueryBuilders`

```csharp
// Register a global filter query builder — applies to all entities where the builder
// can handle the entity type (determined by IGlobalFilteredQueryBuilder.CanBuild()).
public static EntityServiceCollectionOptions AddGlobalFilterQueryBuilder<TImplementation>(
    this EntityServiceCollectionOptions options)
    where TImplementation : class, IGlobalFilteredQueryBuilder;

// Typed variant (also registers as IGlobalFilteredQueryBuilder<TKey>).
public static EntityServiceCollectionOptions AddGlobalFilterQueryBuilder<TImplementation, TKey>(
    this EntityServiceCollectionOptions options)
    where TImplementation : class, IGlobalFilteredQueryBuilder<TKey>;

// Register the default set of global filters (FilterIdsQueryBuilder, FilterArchivablesQueryBuilder,
// FilterHasCreatedQueryBuilder, FilterHasLastModifiedQueryBuilder).
// Called automatically by UseDefaults().
public static EntityServiceCollectionOptions AddDefaultGlobalQueryFilters(
    this EntityServiceCollectionOptions options);
```

---

### EntityServiceCollection\<TContext\>

The fluent builder returned by `UseEntities<TContext>()`. Use `.For<>()` to register each entity.

**Namespace:** `Regira.Entities.DependencyInjection.ServiceBuilders`

```csharp
public class EntityServiceCollection<TContext>(EntityServiceCollectionOptions options)
    where TContext : DbContext
{
    // Simple entity (int key, default SearchObject)
    EntityServiceCollection<TContext> For<TEntity>(
        Action<EntityIntServiceBuilder<TContext, TEntity>>? configure = null)
        where TEntity : class, IEntity<int>;

    // Entity with custom TKey
    EntityServiceCollection<TContext> For<TEntity, TKey>(
        Action<EntityServiceBuilder<TContext, TEntity, TKey>>? configure = null)
        where TEntity : class, IEntity<TKey>;

    // Entity with custom TKey + SearchObject
    EntityServiceCollection<TContext> For<TEntity, TKey, TSearchObject>(
        Action<EntitySearchObjectServiceBuilder<TContext, TEntity, TKey, TSearchObject>>? configure = null)
        where TEntity : class, IEntity<TKey>
        where TSearchObject : class, ISearchObject<TKey>, new();

    // Full-featured (int key)
    EntityServiceCollection<TContext> For<TEntity, TSearchObject, TSortBy, TIncludes>(
        Action<ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes>>? configure = null)
        where TEntity : class, IEntity<int>
        where TSearchObject : class, ISearchObject<int>, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum;

    // Full-featured (with TKey)
    EntityServiceCollection<TContext> For<TEntity, TKey, TSearchObject, TSortBy, TIncludes>(
        Action<ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>>? configure = null)
        where TEntity : class, IEntity<TKey>
        where TSearchObject : class, ISearchObject<TKey>, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum;

    // Attachment support for the default Attachment entity
    EntityServiceCollection<TContext> WithAttachments(
        Func<IServiceProvider, IFileService> factory,
        Action<EntitySearchObjectServiceBuilder<TContext, Attachment, int, AttachmentSearchObject<int>>>? configure = null);

    // Attachment support for a custom attachment entity
    EntityServiceCollection<TContext> WithAttachments<TAttachment, TAttachmentKey, TAttachmentSearchObject>(
        Func<IServiceProvider, IFileService> fileServiceFactory,
        Action<EntitySearchObjectServiceBuilder<TContext, TAttachment, TAttachmentKey, TAttachmentSearchObject>>? configure = null)
        where TAttachment : class, IAttachment<TAttachmentKey>, new()
        where TAttachmentSearchObject : AttachmentSearchObject<TAttachmentKey>, new();

    // Typed attachment query service
    EntityServiceCollection<TContext> ConfigureTypedAttachmentService(
        Func<TContext, IList<IAttachmentQuerySetDescriptor>> queryFactory);

    EntityServiceCollection<TContext> ConfigureTypedAttachmentService<TService>()
        where TService : class, ITypedAttachmentService;

    // Generic service registration helpers
    EntityServiceCollection<TContext> AddTransient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;

    EntityServiceCollection<TContext> AddTransient<TService>(
        Func<IServiceProvider, TService> factory)
        where TService : class;
}
```

### Entity Service Builder

**Namespace:** `Regira.Entities.DependencyInjection.ServiceBuilders`  
**Class:** `EntityServiceBuilder<TContext, TEntity, TKey>`

```csharp
public partial class EntityServiceBuilder<TContext, TEntity, TKey>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
{
    // Service checks
    bool HasEntityService();
    bool HasService<TService>();

    // Search object configuration
    EntitySearchObjectServiceBuilder<TContext, TEntity, TKey, TSearchObject> 
        WithSearchObject<TSearchObject>()
        where TSearchObject : class, ISearchObject<TKey>, new();

    // Mapping
    MappedEntityServiceBuilder<TContext, TEntity, TKey, TDto, TInputDto> 
        UseMapping<TDto, TInputDto>(Action<IEntityMapConfigurator>? mapAction = null);

    MappedEntityServiceBuilder<TContext, TEntity, TKey> 
        UseMapping(Type dto, Type inputDto, Action<IEntityMapConfigurator>? mapAction = null);

    EntityServiceBuilder<TContext, TEntity, TKey> 
        AddMapping<TSource, TTarget>();

    EntityServiceBuilder<TContext, TEntity, TKey> 
        AddMapping(Type sourceType, Type targetType);

    // Entity service registration
    EntityServiceBuilder<TContext, TEntity, TKey> 
        AddDefaultService();

    EntityServiceBuilder<TContext, TEntity, TKey> 
        UseEntityService<TService>()
        where TService : class, IEntityService<TEntity, TKey>, IEntityService<TEntity, TKey, SearchObject<TKey>>;

    EntityServiceBuilder<TContext, TEntity, TKey> 
        UseEntityService<TService>(Func<IServiceProvider, TService> factory)
        where TService : class, IEntityService<TEntity, TKey, SearchObject<TKey>>;

    // Read/Write services
    EntityServiceBuilder<TContext, TEntity, TKey> 
        UseReadService<TService>()
        where TService : class, IEntityReadService<TEntity, TKey, SearchObject<TKey>>;

    EntityServiceBuilder<TContext, TEntity, TKey> 
        UseWriteService<TService>()
        where TService : class, IEntityWriteService<TEntity, TKey>;

    // Repository
    EntityServiceBuilder<TContext, TEntity, TKey> 
        HasRepository<TService>()
        where TService : class, IEntityRepository<TEntity, TKey, SearchObject<TKey>>;

    EntityServiceBuilder<TContext, TEntity, TKey> 
        HasRepository<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityRepository<TEntity, TKey>, IEntityRepository<TEntity, TKey, SearchObject<TKey>>;

    // Manager
    EntityServiceBuilder<TContext, TEntity, TKey> 
        HasManager<TService>()
        where TService : class, IEntityManager<TEntity, TKey>, IEntityManager<TEntity, TKey, SearchObject<TKey>>;

    EntityServiceBuilder<TContext, TEntity, TKey> 
        HasManager<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityManager<TEntity, TKey>, IEntityManager<TEntity, TKey, SearchObject<TKey>>;

    // Query builders
    EntityServiceBuilder<TContext, TEntity, TKey> 
        AddDefaultQueryBuilder();

    EntityServiceBuilder<TContext, TEntity, TKey> 
        UseQueryBuilder<TImplementation>()
        where TImplementation : class, IQueryBuilder<TEntity, TKey, SearchObject<TKey>, EntitySortBy, EntityIncludes>;

    EntityServiceBuilder<TContext, TEntity, TKey> 
        UseQueryBuilder<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IQueryBuilder<TEntity, TKey, SearchObject<TKey>, EntitySortBy, EntityIncludes>;

    // Query filters
    EntityServiceBuilder<TContext, TEntity, TKey> 
        AddQueryFilter<TImplementation>()
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, SearchObject<TKey>>;

    EntityServiceBuilder<TContext, TEntity, TKey> 
        AddQueryFilter<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, SearchObject<TKey>>;

    EntityServiceBuilder<TContext, TEntity, TKey> 
        Filter(Func<IQueryable<TEntity>, SearchObject<TKey>?, IQueryable<TEntity>> filterFunc);

    // Sorting and includes
    EntityServiceBuilder<TContext, TEntity, TKey> 
        SortBy(Func<IQueryable<TEntity>, IQueryable<TEntity>> sortBy);

    EntityServiceBuilder<TContext, TEntity, TKey> 
        Includes(Func<IQueryable<TEntity>, EntityIncludes?, IQueryable<TEntity>> addIncludes);

    // Primers
    EntityServiceBuilder<TContext, TEntity, TKey> 
        AddPrimer<TPrimer>()
        where TPrimer : class, IEntityPrimer<TEntity>;

    // Normalizers
    EntityServiceBuilder<TContext, TEntity, TKey> 
        AddNormalizer<TNormalizer>()
        where TNormalizer : class, IEntityNormalizer<TEntity>;

    // Processors
    EntityServiceBuilder<TContext, TEntity, TKey> 
        Process(Func<IList<TEntity>, EntityIncludes?, Task> process);

    EntityServiceBuilder<TContext, TEntity, TKey> 
        Process(Action<TEntity, EntityIncludes?> process);

    EntityServiceBuilder<TContext, TEntity, TKey> 
        Process<TProcessor>()
        where TProcessor : class, IEntityProcessor<TEntity, EntityIncludes>;

    // Preppers
    EntityServiceBuilder<TContext, TEntity, TKey> 
        Prepare(Action<TEntity> prepareFunc);

    EntityServiceBuilder<TContext, TEntity, TKey> 
        Prepare(Func<TEntity, TContext, Task> prepareFunc);

    // Related collections — manages a child collection via RelatedCollectionPrepper
    EntityServiceBuilder<TContext, TEntity, TKey> 
        Related<TRelated, TRelatedKey>(
            Expression<Func<TEntity, ICollection<TRelated>?>> navigationExpression,
            Action<TEntity>? prepareFunc = null)
        where TRelated : class, IEntity<TRelatedKey>;

    // Build
    void Build();
}
```

### Int-Key Variants

```csharp
// Shortcut for entities with int keys
public partial class EntityIntServiceBuilder<TContext, TEntity>(EntityServiceCollectionOptions options)
    : EntityServiceBuilder<TContext, TEntity, int>(options),
      IEntityServiceBuilder<TContext, TEntity>
    where TContext : DbContext
    where TEntity : class, IEntity<int>
{
    EntityIntServiceBuilder<TContext, TEntity, TSearchObject> 
        WithSearchObject<TSearchObject>()
        where TSearchObject : class, ISearchObject<int>, new();

    void Build();
}
```

### Complex Service Builder

```csharp
public partial class ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    // Inherits all methods from EntityServiceBuilder
    // Plus specialized methods for TSortBy and TIncludes

    void Build();
}
```

### MappedEntityServiceBuilder

Returned by `UseMapping<TDto, TInputDto>()`. Inherits all `EntityServiceBuilder` methods.

**Namespace:** `Regira.Entities.DependencyInjection.Mapping`

```csharp
// Base variant — After mappers for any source/target
public class MappedEntityServiceBuilder<TContext, TEntity, TKey>(EntityServiceCollectionOptions options)
    : EntityServiceBuilder<TContext, TEntity, TKey>(options)
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
{
    // Register a typed after-mapper class
    MappedEntityServiceBuilder<TContext, TEntity, TKey> After<TImplementation>()
        where TImplementation : class, IEntityAfterMapper;

    MappedEntityServiceBuilder<TContext, TEntity, TKey> After<TImplementation>(
        Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityAfterMapper;

    // Register an inline after-mapper (Entity -> DTO)
    MappedEntityServiceBuilder<TContext, TEntity, TKey> After<TSource, TTarget>(
        Action<TSource, TTarget> afterMapAction);

    MappedEntityServiceBuilder<TContext, TEntity, TKey> After<TSource, TTarget>(
        Func<IServiceProvider, Action<TSource, TTarget>> afterMapActionFactory);
}

// Typed variant — DTO and InputDto are known; shortcut After/AfterInput methods
public class MappedEntityServiceBuilder<TContext, TEntity, TKey, TDto, TInputDto>(EntityServiceCollectionOptions options)
    : MappedEntityServiceBuilder<TContext, TEntity, TKey>(options)
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
{
    // Shortcut: Entity -> TDto after-mapper
    MappedEntityServiceBuilder<TContext, TEntity, TKey, TDto, TInputDto> After(
        Action<TEntity, TDto> afterMapAction);

    // Shortcut: TInputDto -> Entity after-mapper
    MappedEntityServiceBuilder<TContext, TEntity, TKey, TDto, TInputDto> AfterInput(
        Action<TInputDto, TEntity> afterMapAction);
}
```

---

## Mapping and Processing

### Mapping

**Namespace:** `Regira.Entities.Mapping.Abstractions`

```csharp
public interface IEntityMapper
{
    TTarget Map<TTarget>(object source);
    TTarget Map<TSource, TTarget>(TSource source, TTarget target);
}

public abstract class EntityMapperBase(IEnumerable<IEntityAfterMapper>? afterMappers = null) 
    : IEntityMapper
{
    public virtual TTarget Map<TTarget>(object source);
    public virtual TTarget Map<TSource, TTarget>(TSource source, TTarget target);
    public virtual void ApplyAfterMappings<TSource, TTarget>(TSource source, TTarget target);
}
```

### ServiceCollectionMappingExtensions

Extension methods on `EntityServiceCollectionOptions` for global mapping setup.

**Namespace:** `Regira.Entities.DependencyInjection.Mapping`

```csharp
public static class ServiceCollectionMappingExtensions
{
    // Configure mapping library (MapConfigurator + IEntityMapper implementation)
    public static EntityServiceCollectionOptions AddMapping(
        this EntityServiceCollectionOptions options,
        Action<MappingConfigurator> configure);

    public static EntityServiceCollectionOptions AddMapping<TEntityMapper>(
        this EntityServiceCollectionOptions options,
        Func<IServiceCollection, IEntityMapConfigurator> configFactory)
        where TEntityMapper : class, IEntityMapper;

    // Register a global after-mapper class
    public static EntityServiceCollectionOptions AddAfterMapper<TAfterMapper>(
        this EntityServiceCollectionOptions options)
        where TAfterMapper : class, IEntityAfterMapper;

    // Register a global inline after-mapper
    public static EntityServiceCollectionOptions AfterMap<TSource, TTarget>(
        this EntityServiceCollectionOptions options,
        Action<TSource, TTarget> afterMapAction);

    public static EntityServiceCollectionOptions AfterMap<TSource, TTarget>(
        this EntityServiceCollectionOptions options,
        Func<IServiceProvider, Action<TSource, TTarget>> afterMapAction);
}
```

### After Mappers

**Namespace:** `Regira.Entities.Mapping.Abstractions`

```csharp
// Non-generic interface
public interface IEntityAfterMapper
{
    bool CanMap(object source);
    void AfterMap(object source, object target);
}

// Typed interface
public interface IEntityAfterMapper<in TSource, in TTarget> : IEntityAfterMapper
{
    void AfterMap(TSource source, TTarget target);
}

// Abstract base class (inherit to create a custom after-mapper)
public abstract class EntityAfterMapperBase<TSource, TTarget> : IEntityAfterMapper<TSource, TTarget>
{
    public abstract void AfterMap(TSource source, TTarget target);
    public bool CanMap(object source);
    void IEntityAfterMapper.AfterMap(object source, object target);
}

// Concrete helper — wraps an Action
public class EntityAfterMapper<TSource, TTarget>(Action<TSource, TTarget> afterMapAction)
    : EntityAfterMapperBase<TSource, TTarget>
{
    public override void AfterMap(TSource source, TTarget target);
}
```

### Query Builders

**Namespace:** `Regira.Entities.EFcore.QueryBuilders.Abstractions`

```csharp
public interface IQueryBuilder<TEntity, in TKey, in TSearchObject, TSortBy, TIncludes>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    IQueryable<TEntity> BuildQuery(
        IQueryable<TEntity> query, 
        IList<TSearchObject?> so, 
        IList<TSortBy> sortBy, 
        TIncludes? includes);
}

public interface IFilteredQueryBuilder<TEntity, in TKey, in TSearchObject>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
{
    IQueryable<TEntity> Filter(IQueryable<TEntity> query, TSearchObject? so);
}

public interface ISortedQueryBuilder<TEntity, in TKey>
    where TEntity : class, IEntity<TKey>
{
    IQueryable<TEntity> Sort(IQueryable<TEntity> query);
}

public interface IIncludableQueryBuilder<TEntity, in TKey>
    where TEntity : class, IEntity<TKey>
{
    IQueryable<TEntity> Include(IQueryable<TEntity> query, object? includes);
}
```

### Processors

**Namespace:** `Regira.Entities.EFcore.Processing.Abstractions`

```csharp
public interface IEntityProcessor<TEntity, in TIncludes>
    where TIncludes : struct, Enum
{
    Task Process(IList<TEntity> items, TIncludes? includes);
}
```

### Preppers

**Namespace:** `Regira.Entities.EFcore.Preppers.Abstractions`

```csharp
// Non-generic interface
public interface IEntityPrepper
{
    Task Prepare(object modified, object? original);
}

// Typed interface
public interface IEntityPrepper<in TEntity> : IEntityPrepper
{
    Task Prepare(TEntity modified, TEntity? original);
}
```

### Primers

**Namespace:** `Regira.Entities.EFcore.Primers.Abstractions`

```csharp
// Non-generic interface (EF Core interceptor-level)
public interface IEntityPrimer
{
    Task PrepareManyAsync(IList<EntityEntry> entries);
    Task PrepareAsync(object entity, EntityEntry entry);
    bool CanPrepare(object entity);
}

// Typed interface
public interface IEntityPrimer<in T> : IEntityPrimer
{
    Task PrepareAsync(T entity, EntityEntry entry);
    bool CanPrepare(T entity);
}
```

### Normalizers

**Namespace:** `Regira.Entities.EFcore.Normalizing.Abstractions`

```csharp
// Non-generic interface
public interface IEntityNormalizer
{
    bool IsExclusive { get; }
    Task HandleNormalize(object item);
    Task HandleNormalizeMany(IEnumerable<object> items);
}

// Typed interface
public interface IEntityNormalizer<in T> : IEntityNormalizer
{
    Task HandleNormalize(T item);
    Task HandleNormalizeMany(IEnumerable<T> items);
}
```

---

## Response Types

**Namespace:** `Regira.Entities.Web.Models`

```csharp
// Details response
public class DetailsResult<TDto>
{
    public TDto Item { get; set; }
    public long? Duration { get; set; }
}

// List response
public class ListResult<TDto>
{
    public IList<TDto> Items { get; set; }
    public long? Duration { get; set; }
}

// Search response (with total count)
public class SearchResult<TDto>
{
    public IList<TDto> Items { get; set; }
    public long Count { get; set; }
    public long? Duration { get; set; }
}

// Save response
public class SaveResult<TDto>
{
    public TDto Item { get; set; }
    public bool IsNew { get; set; }
    public int Affected { get; set; }
    public long? Duration { get; set; }
}

// Delete response
public class DeleteResult<TDto>
{
    public TDto Item { get; set; }
    public long? Duration { get; set; }
}
```

---

## Attachments

**Namespace:** `Regira.Entities.Attachments.Abstractions`

### Attachment Interfaces

```csharp
public interface IAttachment<TKey>
{
    TKey Id { get; set; }
    string? Filename { get; set; }
    string? Uri { get; set; }
    long? Size { get; set; }
}

public interface IEntityAttachment;

public interface IEntityAttachment<TKey, TObjectKey, TAttachmentKey, TAttachment> 
    : IEntityAttachment, IEntity<TKey>, IHasObjectId<TObjectKey>
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
{
    TAttachmentKey AttachmentId { get; set; }
    TAttachment? Attachment { get; set; }
}
```

### Attachment Controller

**Namespace:** `Regira.Entities.Web.Attachments.Abstractions`

```csharp
// Simplest variant (uses EntityAttachmentDto / EntityAttachmentInputDto)
public abstract class EntityAttachmentControllerBase<TEntity>
    : EntityAttachmentControllerBase<TEntity, EntityAttachmentDto, EntityAttachmentInputDto>
    where TEntity : class, IEntityAttachment<int, int, int, Attachment>, IEntity<int>;

// Standard variant
public abstract class EntityAttachmentControllerBase<TEntity, TDto, TInputDto>
    : ControllerBase
    where TEntity : class, IEntityAttachment<int, int, int, Attachment>, IEntity<int>
    where TInputDto : class, IEntityAttachmentInput
{
    // GET attachments/{id}
    [HttpGet("attachments/{id}")]
    public virtual Task<ActionResult<DetailsResult<TDto>>> Details([FromRoute] int id);

    // GET attachments
    [HttpGet("attachments")]
    public virtual Task<ActionResult<ListResult<TDto>>> List(
        [FromQuery] EntityAttachmentSearchObject so,
        [FromQuery] PagingInfo? pagingInfo = null);

    // GET {objectId}/attachments
    [HttpGet("{objectId}/attachments")]
    public virtual Task<ActionResult<ListResult<TDto>>> List(
        [FromRoute] int objectId,
        [FromQuery] EntityAttachmentSearchObject so,
        [FromQuery] PagingInfo? pagingInfo = null);

    // PUT {objectId}/attachments/{id}
    [HttpPut("{objectId}/attachments/{id}")]
    public virtual Task<ActionResult<SaveResult<TDto>>?> Update(
        [FromRoute] int objectId,
        [FromRoute] int id,
        [FromBody] TInputDto model);

    // DELETE attachments/{id}
    [HttpDelete("attachments/{id}")]
    public virtual Task<ActionResult<DeleteResult<TDto>>?> Delete([FromRoute] int id);

    // GET files/{id} — download by attachment ID
    [HttpGet("files/{id}")]
    public virtual Task<IActionResult> GetFile([FromRoute] int id, bool inline = true);

    // GET {objectId}/files/{fileName} — download by object + filename
    [HttpGet("{objectId}/files/{fileName}")]
    public virtual Task<IActionResult> GetFile(
        [FromRoute] int objectId,
        [FromRoute] string fileName,
        bool inline = true);

    // POST {objectId}/files — upload
    [HttpPost("{objectId}/files")]
    public virtual Task<ActionResult<SaveResult<TDto>>> Add(
        [FromRoute] int objectId,
        IFormFile file,
        [FromForm] TInputDto model);
}
```

---

## Exceptions

**Namespace:** `Regira.Entities.Models`

```csharp
public class EntityInputException<T>(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public T? Item { get; set; }
    public IDictionary<string, string> InputErrors { get; set; } = new Dictionary<string, string>();
}
```

---

## Supporting Types

**Namespace:** `Regira.DAL.Paging`

```csharp
public class PagingInfo
{
    public int PageSize { get; set; }
    public int Page { get; set; } = 1;
}
```

**Namespace:** `Regira.Entities.DependencyInjection.ServiceBuilders.Models`

```csharp
public class EntityServiceCollectionOptions(IServiceCollection services)
{
    public IServiceCollection Services { get; }
    public Func<IServiceCollection, IEntityMapConfigurator>? EntityMapConfiguratorFactory { get; set; }
}
```

---

## Notes

### Generic Type Parameter Conventions

- `TEntity` - The entity class
- `TKey` - Primary key type (default: `int`)
- `TSearchObject` - Filter/search criteria class
- `TSortBy` - Enum for sorting options
- `TIncludes` - Enum (flags) for navigation properties
- `TDto` - Data Transfer Object for reading
- `TInputDto` - Data Transfer Object for writing
- `TContext` - EF Core DbContext type

### Common Patterns

1. **Service registration**: Use `.For<TEntity>()` to start entity configuration
2. **Pipeline order**: Global services → Entity-specific services
3. **SaveChanges**: Must be called explicitly when using `IEntityService` directly
4. **Soft delete**: `IsArchived = null` filters to active-only by default
5. **Navigation loading**: Use `Includes` enum with `[Flags]` attribute

### Version Information

This reference is current as of the latest framework version. Check the official documentation for updates.

---

## See Also

- [Entities Instructions](./entities.instructions.md) - Complete framework guide
- [Entities Examples](./entities.examples.md) - Code examples and patterns
- [Entities Namespaces](./entities.namespaces.md) - Namespace reference
