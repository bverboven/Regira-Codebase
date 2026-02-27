# Regira Entities — Services Agent

You are a specialized agent responsible for implementing the **service layer** of the Regira Entities framework. This includes `IEntityService`, `EntityRepository` configuration, QueryBuilders (Filter, Sort, Includes), Processors, Preppers, Primers, and custom wrapping services.

---

## Service Interface

```csharp
// Possible signatures (TKey defaults to int)
IEntityService<TEntity>
IEntityService<TEntity, TKey>
IEntityService<TEntity, TKey, TSearchObject>
IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>
IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
```

- Default implementation is `EntityRepository` backed by EF Core `DbContext`
- Replace via `e.UseEntityService<TService>()` for custom implementations

---

## Standard Service Methods

### Read (auto-persisted)

```csharp
Task<TEntity?> Details(TKey id)
Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null)
Task<IList<TEntity>> List(IList<TSearchObject?> so, IList<TSortBy> sortBy, TIncludes? includes = null, PagingInfo? pagingInfo = null)
Task<long> Count(TSearchObject? so)
Task<long> Count(IList<TSearchObject?> so)
```

### Write (do NOT auto-persist — must call `SaveChanges()`)

```csharp
Task Save(TEntity item)        // calls Add() or Modify() internally
Task Add(TEntity item)
Task<TEntity?> Modify(TEntity item)
Task Remove(TEntity item)
Task<int> SaveChanges(CancellationToken token = default)
```

> The base controller calls `SaveChanges()` automatically. Custom code must call it explicitly.

---

## Query Builders

### Filter Query Builder

Filters the `IQueryable<TEntity>` based on a `SearchObject`.

**Interface & base class:**

```csharp
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

public abstract class FilteredQueryBuilderBase<TEntity, TKey, TSearchObject>
    : IFilteredQueryBuilder<TEntity, TKey, TSearchObject>
    where TSearchObject : ISearchObject<TKey>
{
    public abstract IQueryable<TEntity> Build(IQueryable<TEntity> query, TSearchObject? so);
}
```

**Inline shortcut:**

```csharp
e.Filter((query, so) =>
{
    if (so?.CategoryId?.Any() == true)
        query = query.Where(x => so.CategoryId.Contains(x.CategoryId ?? 0));
    if (so?.MinPrice != null)
        query = query.Where(x => x.Price >= so.MinPrice);
    return query;
});
```

**Separate class:**

```csharp
public class ProductQueryBuilder : FilteredQueryBuilderBase<Product, int, ProductSearchObject>
{
    public override IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? so)
    {
        if (so == null) return query;
        if (so.CategoryId?.Any() == true)
            query = query.Where(x => so.CategoryId.Contains(x.CategoryId ?? 0));
        if (so.MinPrice.HasValue)
            query = query.Where(x => x.Price >= so.MinPrice.Value);
        return query;
    }
}

// Register with:
e.AddQueryFilter<ProductQueryBuilder>();
```

### Global Filter Query Builder

Applies to **all entities implementing an interface**. Registered on `EntityServiceCollectionOptions`.

```csharp
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

public abstract class GlobalFilteredQueryBuilderBase<TEntity> : GlobalFilteredQueryBuilderBase<TEntity, int>;
public abstract class GlobalFilteredQueryBuilderBase<TEntity, TKey>
    : FilteredQueryBuilderBase<TEntity, TKey, ISearchObject<TKey>>,
      IGlobalFilteredQueryBuilder<TEntity, TKey>
```

**Example:**

```csharp
public class FilterByTenantQueryBuilder : GlobalFilteredQueryBuilderBase<ITenantEntity, int>
{
    private readonly ITenantContext _tenantContext;
    public FilterByTenantQueryBuilder(ITenantContext tenantContext) => _tenantContext = tenantContext;

    public override IQueryable<ITenantEntity> Build(IQueryable<ITenantEntity> query, ISearchObject<int>? so)
        => query.Where(x => x.TenantId == _tenantContext.CurrentTenantId);
}

// Register globally:
options.AddGlobalFilterQueryBuilder<FilterByTenantQueryBuilder>();
```

**Built-in global filters (registered by `UseDefaults()`):**

| Class | Applies to | Filters on |
|-------|-----------|-----------|
| `FilterIdsQueryBuilder` | All entities | `Id`, `Ids`, `Exclude` |
| `FilterArchivablesQueryBuilder` | `IArchivable` | `IsArchived` |
| `FilterHasCreatedQueryBuilder` | `IHasCreated` | `MinCreated`, `MaxCreated` |
| `FilterHasLastModifiedQueryBuilder` | `IHasLastModified` | `MinLastModified`, `MaxLastModified` |
| `FilterHasNormalizedContentQueryBuilder` | `IHasNormalizedContent` | `Q` keyword search |

### Sort Query Builder

```csharp
// Interface
public interface ISortedQueryBuilder<TEntity, TKey, TSortBy>
    where TEntity : IEntity<TKey>
    where TSortBy : struct, Enum
{
    IQueryable<TEntity> SortBy(IQueryable<TEntity> query, TSortBy? sortBy = null);
}
```

**Inline with ThenBy support:**

```csharp
e.SortBy((query, sortBy) =>
{
    // Support ThenBy when query is already sorted
    if (typeof(IOrderedQueryable).IsAssignableFrom(query.Expression.Type)
        && query is IOrderedQueryable<Product> sorted)
    {
        return sortBy switch
        {
            ProductSortBy.Title => sorted.ThenBy(x => x.Title),
            ProductSortBy.Price => sorted.ThenBy(x => x.Price),
            _ => sorted.ThenByDescending(x => x.Created)
        };
    }
    return sortBy switch
    {
        ProductSortBy.Title    => query.OrderBy(x => x.Title),
        ProductSortBy.TitleDesc => query.OrderByDescending(x => x.Title),
        ProductSortBy.Price    => query.OrderBy(x => x.Price),
        ProductSortBy.PriceDesc => query.OrderByDescending(x => x.Price),
        _ => query.OrderByDescending(x => x.Created)
    };
});
```

### Include Query Builder

```csharp
// Interface
public interface IIncludableQueryBuilder<TEntity, TKey, TIncludes>
    where TEntity : IEntity<TKey>
    where TIncludes : struct, Enum
{
    IQueryable<TEntity> AddIncludes(IQueryable<TEntity> query, TIncludes? includes = null);
}
```

**Inline:**

```csharp
e.Includes((query, includes) =>
{
    if (includes?.HasFlag(ProductIncludes.Category) == true)
        query = query.Include(x => x.Category);
    if (includes?.HasFlag(ProductIncludes.Reviews) == true)
        query = query.Include(x => x.Reviews!.OrderBy(r => r.SortOrder));
    return query;
});
```

**Separate class:**

```csharp
e.Includes<ProductIncludableQueryBuilder>();
```

---

## Entity Processors

Modify/decorate entities **after fetching from the database**. Used to fill `[NotMapped]` properties.

```csharp
// Interface
public interface IEntityProcessor<TEntity, TIncludes>
    where TIncludes : struct, Enum
{
    Task Process(IList<TEntity> items, TIncludes? includes);
}
```

**Inline:**

```csharp
e.Process((items, includes) =>
{
    foreach (var item in items)
        item.DisplayPrice = $"€{item.Price:F2}";
    return Task.CompletedTask;
});
```

**Separate class (with DI):**

```csharp
using Regira.Entities.EFcore.Processing.Abstractions;

public class CategoryProcessor(YourDbContext dbContext) : IEntityProcessor<Category, CategoryIncludes>
{
    public async Task Process(IList<Category> items, CategoryIncludes? includes)
    {
        var ids = items.Select(i => i.Id).ToList();
        var counts = await dbContext.Products
            .Where(p => ids.Contains(p.CategoryId ?? 0))
            .GroupBy(p => p.CategoryId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        foreach (var item in items)
            item.ProductCount = counts.TryGetValue(item.Id, out var c) ? c : 0;
    }
}

// Register:
e.Process<CategoryProcessor>();
```

---

## Entity Preppers

Prepare entities **before saving**. Generate codes, calculate fields, set FKs.

```csharp
// Interface
public interface IEntityPrepper<in TEntity>
{
    Task Prepare(TEntity modified, TEntity? original);
}

// Base class
public abstract class EntityPrepperBase<TEntity> : IEntityPrepper<TEntity>
{
    public abstract Task Prepare(TEntity modified, TEntity? original);
}
```

**Inline — simple:**

```csharp
e.Prepare(item =>
{
    item.Slug ??= item.Title.ToLowerInvariant().Replace(' ', '-');
});
```

**Inline — with original (null when creating):**

```csharp
e.Prepare((modified, original) =>
{
    if (original == null)
        modified.CreatedBy = "system";
});
```

**Inline — with DbContext:**

```csharp
e.Prepare(async (item, dbContext) =>
{
    item.TotalAmount = item.OrderItems?.Sum(x => x.Quantity * x.UnitPrice) ?? 0;
    await Task.CompletedTask;
});
```

**Separate class:**

```csharp
using Regira.Entities.EFcore.Preppers.Abstractions;

public class ProductPrepper : EntityPrepperBase<Product>
{
    public override Task Prepare(Product modified, Product? original)
    {
        modified.Slug ??= modified.Title.ToLowerInvariant().Replace(' ', '-');
        return Task.CompletedTask;
    }
}

// Register:
e.Prepare<ProductPrepper>();
```

**Inline global (applies to all entities implementing interface):**

```csharp
options.AddPrepper<IHasAggregateKey>(x => x.AggregateKey ??= Guid.NewGuid());
```

### Child Collections (RelatedCollectionPrepper)

```csharp
// Shortcut registers a RelatedCollectionPrepper
e.Related(x => x.OrderItems, (order, _) => order.OrderItems?.SetSortOrder());

// Minimal (no extra preparation):
e.Related(x => x.OrderItems);
```

---

## Entity Primers

EF Core `SaveChangesInterceptors` — run when `DbContext.SaveChanges()` is called. Must be enabled in `AddDbContext` via `AddPrimerInterceptors(sp)`.

```csharp
// Interface
public interface IEntityPrimer<in T>
{
    Task PrepareAsync(T entity, EntityEntry entry);
    bool CanPrepare(T entity);
}

// Base class
public abstract class EntityPrimerBase<T> : IEntityPrimer<T>
{
    public abstract Task PrepareAsync(T entity, EntityEntry entry);
    public virtual bool CanPrepare(T? entity) => entity != null;
}
```

**Separate class:**

```csharp
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Primers.Abstractions;

public class ProductPrimer : EntityPrimerBase<Product>
{
    public override Task PrepareAsync(Product entity, EntityEntry entry)
    {
        if (entry.State == EntityState.Added)
            entity.Code ??= Guid.NewGuid().ToString("N")[..8].ToUpper();
        return Task.CompletedTask;
    }
}

// Register on entity:
e.AddPrimer<ProductPrimer>();

// Or register globally:
options.AddPrimer<GlobalPrimerClass>();
```

**Built-in primers (registered by `UseDefaults()`):**

| Class | Applies to | Behaviour |
|-------|-----------|-----------|
| `HasCreatedDbPrimer` | `IHasCreated` | Sets `Created` on insert |
| `HasLastModifiedDbPrimer` | `IHasLastModified` | Sets `LastModified` on update |
| `ArchivablePrimer` | `IArchivable` | Soft-delete: sets `IsArchived = true` |
| `AutoTruncatePrimer` | All entities | Truncates strings to `[MaxLength]` |

---

## Custom Wrapping Service (`EntityWrappingServiceBase`)

Use to add caching, authorization, auditing, or complex validation **at the service level**.

```csharp
using Regira.Entities.Services.Abstractions;

public interface IProductService
    : IEntityService<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes> { }

public class ProductService
    : EntityWrappingServiceBase<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes>,
      IProductService
{
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IEntityService<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes> inner,
        ILogger<ProductService> logger)
        : base(inner) => _logger = logger;

    public override async Task<Product?> Details(int id)
    {
        _logger.LogDebug("Fetching product {Id}", id);
        return await base.Details(id);
    }

    public override async Task Save(Product item)
    {
        if (item.Price < 0)
            throw new EntityInputException<Product>("Price cannot be negative")
            {
                Item = item,
                InputErrors = new Dictionary<string, string>
                {
                    [nameof(Product.Price)] = "Must be >= 0"
                }
            };
        await base.Save(item);
    }
}
```

**Registration:**

```csharp
e.AddTransient<IProductService, ProductService>();  // register interface (optional)
e.UseEntityService<ProductService>();               // replace default EntityRepository
```

**Inject by interface in other services:**

```csharp
public class SomeOtherService(IProductService productService)
{
    // IProductService resolves to ProductService (wrapping EntityRepository)
}
```

### Available Override Methods

All methods delegate to the inner service by default — override only what you need:

```csharp
// Read
Task<TEntity?> Details(TKey id)
Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null)
Task<IList<TEntity>> List(IList<TSearchObject?> so, IList<TSortBy> sortBy, TIncludes? includes, PagingInfo? pagingInfo)
Task<long> Count(TSearchObject? so)
Task<long> Count(IList<TSearchObject?> so)

// Write (do NOT auto-persist — caller must call SaveChanges())
Task Save(TEntity item)        // calls Add() or Modify()
Task Add(TEntity item)
Task<TEntity?> Modify(TEntity item)
Task Remove(TEntity item)
Task<int> SaveChanges(CancellationToken token = default)
```

### EntityInputException

Thrown inside services (e.g. wrapping services or preppers) to signal validation failures. The base controller catches it and returns a `400 Bad Request`.

```csharp
using Regira.Entities.Models; // EntityInputException<T>

public class EntityInputException<T>(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public T? Item { get; set; }
    public IDictionary<string, string> InputErrors { get; set; } = new Dictionary<string, string>();
}
```

Throw it like this:

```csharp
throw new EntityInputException<Product>("Validation failed")
{
    Item = item,
    InputErrors = new Dictionary<string, string>
    {
        [nameof(Product.Price)] = "Must be >= 0"
    }
};
```

---

## DI Registration — Full Example

### UseDefaults() Breakdown

`options.UseDefaults()` registers three groups of services in one call:

| Sub-method | Registers |
|------------|-----------|
| `AddDefaultPrimers()` | `ArchivablePrimer`, `HasCreatedDbPrimer`, `HasLastModifiedDbPrimer` |
| `AddDefaultGlobalQueryFilters()` | `FilterIdsQueryBuilder`, `FilterArchivablesQueryBuilder`, `FilterHasCreatedQueryBuilder`, `FilterHasLastModifiedQueryBuilder` |
| `AddDefaultEntityNormalizer()` | `DefaultNormalizer`, `ObjectNormalizer`, `DefaultEntityNormalizer`, `QKeywordHelper` |

```csharp
services.AddDbContext<MyDbContext>((sp, db) =>
{
    db.UseSqlite(connectionString)
        .AddPrimerInterceptors(sp)       // enables primers
        .AddNormalizerInterceptors(sp)   // enables normalizers
        .AddAutoTruncateInterceptors();  // prevents truncation exceptions
});

services
    .UseEntities<MyDbContext>(options =>
    {
        options.UseDefaults();
        options.UseMapsterMapping();
        options.AddGlobalFilterQueryBuilder<FilterByTenantQueryBuilder>();
        options.AddPrepper<IHasAggregateKey>(x => x.AggregateKey ??= Guid.NewGuid());
    })
    .For<Product, ProductSearchObject, ProductSortBy, ProductIncludes>(e =>
    {
        e.AddQueryFilter<ProductQueryBuilder>();
        e.SortBy((q, s) => /* ... */);
        e.Includes((q, i) => /* ... */);
        e.Process<ProductProcessor>();
        e.Prepare<ProductPrepper>();
        e.AddPrimer<ProductPrimer>();
        e.UseMapping<ProductDto, ProductInputDto>();
        e.Related(x => x.Reviews);
    });
```

### Registration Order Matters

1. **Global services** execute first (registered on `EntityServiceCollectionOptions`)
2. **Entity-specific services** execute next (registered on entity builder)

### Extension Method Pattern (Recommended)

```csharp
public static class ProductServiceCollectionExtensions
{
    public static IEntityServiceCollection<TContext> AddProducts<TContext>(
        this IEntityServiceCollection<TContext> services)
        where TContext : DbContext
    {
        services.For<Product, ProductSearchObject, ProductSortBy, ProductIncludes>(e =>
        {
            // configure here
        });
        return services;
    }
}

// Usage:
services.UseEntities<MyDbContext>(/* ... */)
    .AddProducts()
    .AddCategories();
```

---

## Built-in LINQ Query Extensions

```csharp
using Regira.Entities.EFcore.Extensions; // FilterId, FilterIds, FilterExclude, FilterCode, FilterTitle, FilterQ, FilterArchivable, FilterHasAttachment, SortQuery
using Regira.DAL.Paging;                 // PageQuery, PagingInfo

// ID filters
query.FilterId<TEntity, TKey>(TKey? id)
query.FilterIds<TEntity, TKey>(ICollection<TKey>? ids)
query.FilterExclude<TEntity, TKey>(ICollection<TKey>? ids)

// Property filters
query.FilterCode<TEntity>(string? code)
query.FilterTitle<TEntity>(ParsedKeywordCollection? keywords)
query.FilterNormalizedTitle<TEntity>(ParsedKeywordCollection? keywords)

// Date range filters
query.FilterCreated<TEntity>(DateTime? minDate, DateTime? maxDate)
query.FilterLastModified<TEntity>(DateTime? minDate, DateTime? maxDate)
query.FilterTimestamps<TEntity>(DateTime? minCreated, DateTime? maxCreated, DateTime? minModified, DateTime? maxModified)

// Keyword / archive / attachment
query.FilterQ<TEntity>(ParsedKeywordCollection? keywords)
query.FilterArchivable<TEntity>(bool? isArchived)
query.FilterHasAttachment<TEntity>(bool? hasAttachment)

// Default sort
query.SortQuery<TEntity, TKey>()

// Pagination
query.PageQuery<T>(PagingInfo? info)
query.PageQuery<T>(int pageSize, int page = 1)
```

---

## Common Patterns

### Master-Detail (Order + OrderItems)

```csharp
// Entities
public class Order : IEntityWithSerial, IHasTimestamps
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public ICollection<OrderItem>? OrderItems { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class OrderItem : IEntityWithSerial, ISortable
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int SortOrder { get; set; }
}

// DI configuration
.For<Order, OrderSearchObject, OrderSortBy, OrderIncludes>(e =>
{
    e.Related(x => x.OrderItems, (order, _) => order.OrderItems?.SetSortOrder());
    e.Prepare(item =>
    {
        item.TotalAmount = item.OrderItems?.Sum(x => x.Quantity * x.UnitPrice) ?? 0;
    });
});
```

### Soft Delete

```csharp
// Implement IArchivable on the entity
public class Product : IEntityWithSerial, IArchivable
{
    public int Id { get; set; }
    public bool IsArchived { get; set; }
}

// UseDefaults() registers ArchivablePrimer + FilterArchivablesQueryBuilder automatically.
// SearchObject.IsArchived filtering:
//   null  = only active (default)
//   false = only active
//   true  = only archived
```

### Audit Trail (Custom Primer)

```csharp
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Primers.Abstractions;

public interface IAuditable
{
    int CreatedBy { get; set; }
    int? ModifiedBy { get; set; }
}

public class UserTrackingPrimer(IHttpContextAccessor httpContextAccessor)
    : EntityPrimerBase<IAuditable>
{
    public override Task PrepareAsync(IAuditable entity, EntityEntry entry)
    {
        var userId = int.Parse(httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value ?? "0");
        if (entry.State == EntityState.Added)
            entity.CreatedBy = userId;
        else if (entry.State == EntityState.Modified)
            entity.ModifiedBy = userId;
        return Task.CompletedTask;
    }
}

// Register globally:
options.AddPrimer<UserTrackingPrimer>();
```

### Caching (EntityWrappingServiceBase)

```csharp
using Microsoft.Extensions.Caching.Memory;
using Regira.Entities.Services.Abstractions;

public class CachedProductService(
    IEntityService<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes> inner,
    IMemoryCache cache)
    : EntityWrappingServiceBase<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes>(inner)
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public override async Task<Product?> Details(int id)
    {
        var key = $"product:{id}";
        if (cache.TryGetValue(key, out Product? cached)) return cached;
        var result = await base.Details(id);
        if (result != null) cache.Set(key, result, CacheDuration);
        return result;
    }

    public override async Task Save(Product item)
    {
        await base.Save(item);
        cache.Remove($"product:{item.Id}");
    }
}

// Register:
e.UseEntityService<CachedProductService>();
```

### Hierarchical Data (Self-referencing)

```csharp
public class Category : IEntityWithSerial, IHasTitle
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category>? Children { get; set; }
}

// In query builder:
if (so?.ParentId != null)
    query = query.Where(x => x.ParentId == so.ParentId);
if (so?.RootOnly == true)
    query = query.Where(x => x.ParentId == null);
```

---

## Key Namespaces

```csharp
using Regira.Entities.Services.Abstractions;               // IEntityService<...>, EntityWrappingServiceBase<...>
using Regira.Entities.EFcore.QueryBuilders.Abstractions;   // FilteredQueryBuilderBase<...>, GlobalFilteredQueryBuilderBase<...>
using Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders; // FilterIdsQueryBuilder, FilterArchivablesQueryBuilder, etc.
using Regira.Entities.EFcore.Extensions;                         // QueryExtensions (FilterId, FilterIds, FilterTitle, FilterArchivable, etc.)
using Regira.Entities.Keywords.Abstractions;               // IQKeywordHelper
using Regira.Entities.Keywords;                            // QKeywordHelper
using Regira.Entities.EFcore.Processing.Abstractions;      // IEntityProcessor<TEntity, TIncludes>
using Regira.Entities.EFcore.Preppers.Abstractions;        // IEntityPrepper<TEntity>, EntityPrepperBase<TEntity>
using Regira.Entities.EFcore.Primers.Abstractions;         // EntityPrimerBase<T>, IEntityPrimer<T>
using Regira.Entities.EFcore.Primers;                      // ArchivablePrimer, HasCreatedDbPrimer, HasLastModifiedDbPrimer, AutoTruncatePrimer
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;   // UseEntities(), UseDefaults()
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions; // IEntityServiceCollection
using Regira.DAL.EFcore.Services;                          // AddAutoTruncateInterceptors()
using Regira.DAL.Paging;                                   // PagingInfo
using Regira.Entities.Models;                              // EntityInputException<T>
using Microsoft.EntityFrameworkCore;                       // DbContext, EntityState, Include()
using Microsoft.EntityFrameworkCore.ChangeTracking;        // EntityEntry (used in primers)
```
