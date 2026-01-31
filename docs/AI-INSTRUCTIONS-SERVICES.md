# Entity Services - AI Agent Instructions

## Service Layer Architecture

The service layer provides business logic and data access. The framework offers multiple service types depending on complexity needs.

## Service Interface Selection

### Choose the Right Interface

```csharp
// 1. Basic CRUD with int primary key
IEntityService<Product>

// 2. Custom primary key type
IEntityService<Product, Guid>

// 3. Add filtering with SearchObject
IEntityService<Product, int, ProductSearchObject>

// 4. Full featured: filtering, sorting, includes
IEntityService<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes>

// 5. Alternative: Skip key type, keep other features
IEntityService<Product, ProductSearchObject, ProductSortBy, ProductIncludes>
```

## Standard Service Methods

### Read Operations

```csharp
// Get single entity details by ID
Task<TEntity?> Details(TKey id);

// List with custom SearchObject (enhanced filtering)
Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null);

// List with sorting and includes (complex filtering)
Task<IList<TEntity>> List(
    IList<TSearchObject?> so, 
    IList<TSortBy> sortBy, 
    TIncludes? includes = null, 
    PagingInfo? pagingInfo = null);

// Count with custom SearchObject
Task<long> Count(TSearchObject? so);

// Count with multiple SearchObjects
Task<long> Count(IList<TSearchObject?> so);
```

**Key Points:**
- `Details(id)` returns a single entity (not `GetAsync`)
- `List()` returns `IList<TEntity>` not `IEnumerable<TEntity>`
- SearchObject is used for **enhanced filtering** - basic filtering uses `object?`
- `Count()` returns `long` not `int`
- PagingInfo supports offset/limit patterns

### Write Operations

```csharp
// Add new entity (does not call SaveChanges)
Task Add(TEntity item);

// Modify existing entity (does not call SaveChanges)
Task<TEntity?> Modify(TEntity item);

// Save entity (add or modify based on ID, does not call SaveChanges)
Task Save(TEntity item);

// Remove entity (does not call SaveChanges)
Task Remove(TEntity item);

// Persist all changes to database
Task<int> SaveChanges(CancellationToken token = default);
```

**Important Notes:**
- Write methods (`Add`, `Modify`, `Save`, `Remove`) **do NOT automatically persist changes**
- You **must call `SaveChanges()`** to commit changes to the database
- `Save()` internally determines whether to Add or Modify based on entity state
- `Modify()` returns `Task<TEntity?>` to allow for refresh after modification
- `SaveChanges()` returns the number of affected rows

### Pagination with PagingInfo

```csharp
// PagingInfo class
public class PagingInfo
{
    public int PageSize { get; set; }   // Number of items per page
    public int Page { get; set; } = 1;  // Page number (1-based, first page is 1)
}

// Usage example
var paging = new PagingInfo { PageSize = 20, Page = 1 };  // First page, 20 items
var items = await service.List(searchObject, paging);
var total = await service.Count(searchObject);

// Calculate total pages
var totalPages = (int)Math.Ceiling((double)total / paging.PageSize);
```

**Pagination Pattern:**
- `Page` is **1-based** (first page is 1, not 0)
- `PageSize` determines how many items per page
- If `PageSize` is 0 or less, no paging is applied (returns all results)
- Always call `Count()` separately to get total for pagination UI
- SearchObject does NOT contain paging properties - use PagingInfo parameter

## Default Implementation: EntityRepository

When no custom EntityService is provided, the **EntityServiceBuilder automatically registers a generic `EntityRepository`**:

```csharp
// Automatic registration via UseEntities().For<T>()
builder.Services.UseEntities()
    .For<Product>()
    .Configure(options => { /* ... */ });

// This automatically registers:
// IEntityService<Product> -> EntityRepository<Product>
```

**Key Points:**
- EntityRepository is registered **automatically** by the framework
- No manual registration needed for standard CRUD operations
- Configuration is done via `For<TEntity>()` (QueryBuilders, Preppers, Processors, etc.)
- See [DI Instructions](AI-INSTRUCTIONS-DI.md) for configuration details

## Custom Service Implementation

### Best Practices: Avoid Custom Repositories

**IMPORTANT:** Most cases do NOT need custom repositories. Use the built-in configuration instead:

✅ **Recommended Approach** - Configure Entity Processing:
```csharp
// Global configuration (applies to all entities)
services.UseEntities<AppDbContext>(o =>
{
    o.UseDefaults();  // Adds default primers, normalizers, and global filters
    o.AddGlobalFilterQueryBuilder<TenantFilterQueryBuilder>();  // Custom global filter
    o.AddPrimer<CustomGlobalPrimer>();  // Custom global primer
    o.AddPrepper<IHasAggregateKey>(x => x.AggregateKey ??= Guid.NewGuid());  // Global prepper for interface
});

// Per-entity configuration
services
    .UseEntities<AppDbContext>()
    .For<Product, int, ProductSearchObject>(e =>
    {
        // Option 1: Register custom classes
        e.AddQueryFilter<ProductQueryFilter>();
        e.AddProcessor<ProductProcessor>();
        e.AddPrepper<ProductPrepper>();
        e.AddNormalizer<ProductNormalizer>();
        e.SortBy<ProductSortBuilder>();       // Custom sort class
        e.Includes<ProductIncludeBuilder>();  // Custom include class
        
        // Option 2: Use inline shortcuts (registers generic service behind the scenes)
        e.Filter((query, so) => 
        {
            if (!string.IsNullOrWhiteSpace(so?.Q))
                query = query.Where(x => x.Title.Contains(so.Q));
            return query;
        });
        
        e.Process((item, includes) => 
        {
            item.HasAttachment = item.Attachments?.Any() ?? false;
        });
        
        e.Prepare(item => 
        {
            item.AggregateKey ??= Guid.NewGuid();
        });
        
        e.SortBy(query => query.OrderBy(x => x.Title));  // Inline sort
        e.Includes(query => query.Include(x => x.Category));  // Inline include
    });
```

**Available Configuration Methods:**

| Method | Shortcut | Purpose |
|--------|----------|---------|
| `e.AddQueryFilter<T>()` | `e.Filter((query, so) => ...)` | Custom query filtering logic |
| `e.AddProcessor<T>()` | `e.Process((item, includes) => ...)` | Post-fetch entity processing |
| `e.AddPrepper<T>()` | `e.Prepare(item => ...)` | Pre-save entity processing |
| `e.AddNormalizer<T>()` | *(no shortcut)* | Entity normalization before save |
| `e.AddPrimer<T>()` | *(no shortcut)* | DbContext save interceptor |
| `e.SortBy<T>()` | `e.SortBy((query, sortBy) => ...)` | Sorting logic (class or inline) |
| `e.Includes<T>()` | `e.Includes((query, includes) => ...)` | Default includes (class or inline) |
| `e.HasManager<T>()` | *(no shortcut)* | Register custom EntityManager |
| `e.UseMapping<TDto, TInput>()` | *(no shortcut)* | Configure AutoMapper/Mapster |
| `e.Related(x => x.Collection)` | *(no shortcut)* | Manage child collections |

**Shortcuts vs Classes:**
- **Use shortcuts** for simple inline logic (filtering, processing one-liners)
- **Use classes** for complex logic, reusability, or when needing dependency injection
- Shortcuts automatically register a generic wrapper service behind the scenes

❌ **Avoid This** - Custom Repository for Basic Logic:
```csharp
// Don't create custom repositories just for filtering/validation
public class ProductRepository : EntityRepository<Product> { /* ... */ }
```

### When to Create Custom Services

Only create custom services when you need:
- **Complex business logic** spanning multiple entities
- **External service integration** (APIs, third-party services)
- **Cross-entity transactions** or orchestration
- **Custom methods** beyond standard CRUD (e.g., `CalculateDiscount()`)

**Not** for: filtering, sorting, validation, or simple data manipulation (use configuration methods instead).

### EntityManager: Extending Functionality

Use **EntityManager** to wrap the default service and add custom logic:

```csharp
public class ProductManager(
    IEntityService<Product, int, ProductSearchObject> service,
    IExternalApiService apiService) 
    : EntityManager<Product, int, ProductSearchObject>(service)
{
    // Add custom methods (inherited CRUD methods work automatically)
    public async Task<decimal> CalculatePriceWithTax(int productId)
    {
        var items = await List(new { Id = productId });
        var item = items.FirstOrDefault();
        return item != null ? item.Price * 1.21m : 0;
    }
    
    // Override to add extra logic
    public override async Task Add(Product item)
    {
        // Add security check
        await ValidateProductAsync(item);
        
        // Call base implementation
        await base.Add(item);
        
        // Notify external service
        await apiService.NotifyProductCreated(item);
    }
}

// Register via configuration
services
    .UseEntities<AppDbContext>()
    .For<Product, int, ProductSearchObject>(e =>
    {
        e.HasManager<ProductManager>();  // Registers as IEntityService and IEntityManager
    });
```

**Key Points:**
- EntityManager wraps the automatically registered EntityRepository
- Inherit all CRUD methods, override as needed
- Add custom business methods
- Register using `e.HasManager<T>()` in entity configuration

### Wrapped Pipeline Pattern

Custom EntityServices can be **added to the wrapped pipeline** for cross-cutting concerns. However, **this is rarely needed** - most cross-cutting logic should use Preppers, Processors, or configuration methods.

```csharp
// Example: Security layer
public class SecureProductService(IEntityService<Product, int, ProductSearchObject> service) : EntityWrappingServiceBase<Product, int, ProductSearchObject>(service)
{
    public override async Task<IList<Product>> List(object? searchObject = null, PagingInfo? pagingInfo = null)
    {
        // Check permissions before listing
        await CheckPermissionsAsync();
        return await base.List(searchObject, pagingInfo);
    }
}

// Manual registration (not typical)
services.AddScoped<IEntityService<Product, int, ProductSearchObject>>(sp =>
{
    var repository = sp.GetRequiredService<IEntityRepository<Product, int, ProductSearchObject>>();
    return new SecureProductService(repository);
});
```

## Service Components

### 1. Query Builders

Query builders modify IQueryable before execution.

#### Filter Query Builders

```csharp
public class ProductQueryFilter(IQKeywordHelper qHelper) : IFilteredQueryBuilder<Product, ProductSearchObject>
{
    public IQueryable<Product> Build(IQueryable<Product> query, ProductSearchObject? search)
    {
        if (search == null) return query;
        
        // General text search using Q property (supports wildcards)
        if (!string.IsNullOrWhiteSpace(search.Q))
        {
            var keywords = qHelper.Parse(search.Q);  // Parse returns ParsedKeywordCollection
            foreach (var keyword in keywords)
            {
                // Use keyword.QW for "contains" search (wildcards on both sides)
                query = query.Where(x => 
                    EF.Functions.Like(x.Title, keyword.QW) ||
                    (x.Description != null && EF.Functions.Like(x.Description, keyword.QW)));
            }
        }
        
        if (!string.IsNullOrWhiteSpace(search.Title))
        {
            query = query.Where(x => x.Title.Contains(search.Title));
        }
        
        // Foreign key filter using ICollection<int> pattern
        if (search.CategoryId?.Any() == true)
        {
            query = query.Where(x => search.CategoryId.Contains(x.CategoryId));
        }
        
        if (search.MinPrice.HasValue)
        {
            query = query.Where(x => x.Price >= search.MinPrice.Value);
        }
        
        if (search.MaxPrice.HasValue)
        {
            query = query.Where(x => x.Price <= search.MaxPrice.Value);
        }
        
        return query;
    }
}
```

**Key Points**:
- Foreign key properties in SearchObject should use `ICollection<TKey>` (e.g., `ICollection<int>? CategoryId`) for flexibility. Use `.Any()` to check if collection has values and `.Contains()` to filter.
- Use `Q` property for general text search across multiple fields (typically IHasTitle/IHasDescription)
- **QKeywordHelper** provides wildcard support:
  - `Parse(string)` - Splits input by spaces and returns `ParsedKeywordCollection` of `QKeyword` objects
  - `ParseKeyword(string)` - Parses a single keyword and returns a `QKeyword` object
  - Wildcard character defaults: `*` in input, `%` in output (for SQL LIKE)
  - Each `QKeyword` provides:
    - `QW` - Normalized keyword with wildcards on both sides (use for "contains" searches)
    - `Q` - Normalized keyword with wildcards only if provided in input
    - `StartsWith` - Normalized keyword with wildcard at end only
    - `EndsWith` - Normalized keyword with wildcard at start only
    - `Normalized` - Normalized keyword without wildcards
  - Inject `IQKeywordHelper` into your filter class to use these methods
  - Use `EF.Functions.Like()` for SQL LIKE queries with wildcard keywords

#### Global Filter Query Builders

Global filters apply to all entities implementing an interface and are **registered globally** (not per entity):

```csharp
// Built-in global filters (registered via UseDefaults()):
// - FilterIdsQueryBuilder: Filters by IDs from SearchObject
// - FilterArchivablesQueryBuilder: Excludes archived items (IArchivable)
// - FilterHasCreatedQueryBuilder: Filters by creation date (IHasCreated)
// - FilterHasLastModifiedQueryBuilder: Filters by modification date (IHasLastModified)

// Custom global filter example:
public class TenantFilterQueryBuilder(ITenantProvider tenantProvider) : IGlobalFilteredQueryBuilder
{
    public IQueryable<TEntity> Build<TEntity>(IQueryable<TEntity> query, object? search) 
        where TEntity : class
    {
        if (typeof(IHasTenantId).IsAssignableFrom(typeof(TEntity)))
        {
            var tenantId = tenantProvider.GetCurrentTenantId();
            query = query.Where(x => ((IHasTenantId)x).TenantId == tenantId);
        }
        return query;
    }
}

// Register globally in UseEntities configuration:
services.UseEntities<AppDbContext>(o =>
{
    o.AddGlobalFilterQueryBuilder<TenantFilterQueryBuilder>();
    // Or use defaults which includes built-in global filters:
    // o.UseDefaults();
});
```

#### Sort Query Builder

```csharp
public class ProductSortedQueryBuilder : ISortedQueryBuilder<Product, ProductSortBy>
{
    public IQueryable<Product> Build(IQueryable<Product> query, ProductSortBy? sortBy)
    {
        return sortBy switch
        {
            ProductSortBy.Title => query.OrderBy(x => x.Title),
            ProductSortBy.Price => query.OrderBy(x => x.Price),
            ProductSortBy.Created => query.OrderByDescending(x => x.Created),
            _ => query.OrderBy(x => x.Id)
        };
    }
}
```

#### Include Query Builder

```csharp
public class ProductIncludableQueryBuilder : IIncludableQueryBuilder<Product, ProductIncludes>
{
    public IQueryable<Product> Build(IQueryable<Product> query, ProductIncludes? include)
    {
        if (include == null || include == ProductIncludes.None)
            return query;
        
        if (include.Value.HasFlag(ProductIncludes.Category))
        {
            query = query.Include(x => x.Category);
        }
        
        if (include.Value.HasFlag(ProductIncludes.Reviews))
        {
            query = query.Include(x => x.Reviews);
        }
        
        if (include.Value.HasFlag(ProductIncludes.Images))
        {
            query = query.Include(x => x.Images)
                .ThenInclude(i => i.File);
        }
        
        return query;
    }
}
```

### 2. Entity Processors

Processors modify/decorate entities after fetching from database:

```csharp
public class ProductProcessor(IImageService imageService) : IEntityProcessor<Product>
{
    public Task Process(IList<Product> items, EntityIncludes? includes)
    {
        foreach (var entity in items)
        {
            // Add computed properties or decorations
            if (!string.IsNullOrEmpty(entity.ImagePath))
            {
                entity.ImageUrl = imageService.GetUrl(entity.ImagePath);
            }
        }
        
        return Task.CompletedTask;
    }
}
```

### 3. After Mappers

After mappers (IAfterMapper) decorate DTOs after the mapping engine completes:

```csharp
public class ProductAfterMapper(IPromotionService promotionService) : IAfterMapper<Product, ProductDto>
{
    public void AfterMap(Product source, ProductDto destination)
    {
        // Decorate DTO with additional data after mapping
        destination.IsOnSale = promotionService.IsOnSale(source.Id);
        destination.DiscountPercentage = promotionService.GetDiscount(source.Id);
    }
}
```

**Note**: IAfterMapper runs after the mapping engine (AutoMapper/Mapster) completes, allowing you to enrich DTOs with additional data.

### 4. Entity Preppers

Preppers prepare entities before saving. They are executed by the WriteRepository before entities are passed to DbContext:

```csharp
public class ProductPrepper : IEntityPrepper<Product>
{
    public Task Prepare(Product modified, Product? original)
    {
        // Normalize data before saving
        modified.Title = modified.Title.Trim();
        
        if (string.IsNullOrEmpty(modified.Slug))
        {
            modified.Slug = GenerateSlug(modified.Title);
        }
        
        return Task.CompletedTask;
    }
    
    private string GenerateSlug(string title)
    {
        return title.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and");
    }
}
```

**Execution**: Preppers are invoked by the WriteRepository before calling DbContext.SaveChanges, allowing entity preparation and normalization.

### 5. Entity Primers

Primers are EF Core SaveChangesInterceptors executed by DbContext during SaveChanges/SubmitChanges. Primers can be registered **globally** (apply to all entities) or **per entity**.

```csharp
public class ProductPrimer : EntityPrimerBase<Product>
{
    protected override Task ProcessAsync(Product entity, EntityState state, CancellationToken cancellationToken)
    {
        if (state == EntityState.Added)
        {
            // Set defaults for new entities
            entity.Created = DateTime.UtcNow;
        }
        
        if (state == EntityState.Modified)
        {
            // Update modification timestamp
            entity.LastModified = DateTime.UtcNow;
        }
        
        return Task.CompletedTask;
    }
}
```

**Built-in Primers (registered globally via `UseDefaults()`):**
- `HasCreatedDbPrimer`: Sets Created timestamp for IHasCreated entities
- `HasLastModifiedDbPrimer`: Sets LastModified timestamp for IHasLastModified entities
- `ArchivablePrimer`: Handles archiving logic for IArchivable entities

**Registration:**

```csharp
// Global registration (applies to all entities)
services.UseEntities<AppDbContext>(o =>
{
    o.AddPrimer<ProductPrimer>();  // Global primer
    
    // Or use defaults which includes built-in primers:
    o.UseDefaults();  // Adds HasCreatedDbPrimer, HasLastModifiedDbPrimer, ArchivablePrimer
});

// Per-entity registration
services.UseEntities<AppDbContext>()
    .For<Product>(e =>
    {
        e.AddPrimer<ProductPrimer>();  // Only for Product entities
    });
```

**Execution**: Primers are registered as SaveChangesInterceptors in DbContext and are automatically invoked during SubmitChanges/SaveChanges operations.

## Service Registration

Services are typically registered in DI. See [DI Instructions](AI-INSTRUCTIONS-DI.md) for details:

```csharp
builder.Services
    .UseEntities<MyDbContext>(o => { /* config */ })
    .For<Product>(cfg =>
    {
        cfg.UseFilter<ProductQueryFilter>();
        cfg.UseSorting<ProductSortedQueryBuilder>();
        cfg.UseIncludes<ProductIncludableQueryBuilder>();
        cfg.UseProcessor<ProductProcessor>();
        cfg.UsePrepper<ProductPrepper>();
        cfg.UseAfterMapper<ProductAfterMapper>();
    });
```

## Best Practices

### DO:
- ✓ Use the minimal service interface needed
- ✓ Leverage built-in repositories when possible
- ✓ Use EntityManager for custom services
- ✓ Use `List(new { Id = id })` for fetching entities in custom logic
- ✓ Use SearchObject properties for all filtering needs
- ✓ Keep query builders focused and testable
- ✓ Use processors for post-fetch entity decoration
- ✓ Use after mappers for post-mapping DTO enrichment
- ✓ Use preppers for pre-save entity preparation (executed by WriteRepository)
- ✓ Use primers for database-level operations (executed by DbContext interceptors)

### DON'T:
- ✗ Create custom services unnecessarily
- ✗ Create custom fetch methods (use List with SearchObject instead)
- ✗ Mix concerns (filtering, business logic, etc.)
- ✗ Perform complex calculations in processors
- ✗ Modify entities in processors (read-only decoration)
- ✗ Access database in preppers, processors, or after mappers
- ✗ Confuse preppers (WriteRepository) with primers (DbContext interceptors)

## Next Steps

- Create API endpoints: [Controllers Instructions](AI-INSTRUCTIONS-CONTROLLERS.md)
- Configure dependency injection: [DI Instructions](AI-INSTRUCTIONS-DI.md)
- See complete examples: [Practical Examples](AI-INSTRUCTIONS-EXAMPLES.md)
