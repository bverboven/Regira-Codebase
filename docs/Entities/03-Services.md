# Entity Services

The `IEntityService` is the core service interface for managing entities. It provides standard CRUD operations and can be customized or extended as needed.

Possible combinations:
```csharp
IEntityService<TEntity> // int ID
IEntityService<TEntity, TKey>
IEntityService<TEntity, TKey, TSearchObject>
IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>
IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
```

## Service Layer Architecture

- Services can be injected into other services using `EntityWrappingServiceBase` to created a wrapped pipeline
- The default implementation is `EntityRepository`, which uses EF Core `DbContext` for data access
- The `EntityRepository` is enriched by multiple helper services (QueryBuilders, Processors, Preppers, Primers)

## Standard EntityRepository Methods

### Read Operations

```csharp
// Get single entity details by ID
Task<TEntity?> Details(TKey id);

// List with custom SearchObject (enhanced filtering)
Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null);
// List with sorting and includes (complex filtering)
Task<IList<TEntity>> List(IList<TSearchObject?> so, IList<TSortBy> sortBy, TIncludes? includes = null, PagingInfo? pagingInfo = null);

// Count with custom (nullable) SearchObject
Task<long> Count(TSearchObject? so);
// Count with multiple SearchObjects
Task<long> Count(IList<TSearchObject?> so);
```

### Write Operations

- Write methods (`Add`, `Modify`, `Save`, `Remove`) **do NOT automatically persist changes**
- You **must call** `SaveChanges()` to commit all changes to the database

```csharp
Task Save(TEntity item); // calls Add() or Modify() internally
Task Add(TEntity item);
Task<TEntity?> Modify(TEntity item);
Task Remove(TEntity item);
// Persist all changes to database
Task<int> SaveChanges(CancellationToken token = default);
```

## Repository helper services

### Query Builders

Query builders are used to filter, sort entities and include navigation properties.

#### Filter Query Builders

- uses the configured `TSearchObject`
- if no SearchObject is configured, a basic `SearchObject<TKey>` is provided

```csharp
// interface
public interface IFilteredQueryBuilder<TEntity, TKey, in TSearchObject>
    where TSearchObject : ISearchObject<TKey>
{
    IQueryable<TEntity> Build(IQueryable<TEntity> query, TSearchObject? so);
}
// base class
public abstract class FilteredQueryBuilderBase<TEntity, TKey, TSearchObject> : IFilteredQueryBuilder<TEntity, TKey, TSearchObject>
    where TSearchObject : ISearchObject<TKey>
{
    public abstract IQueryable<TEntity> Build(IQueryable<TEntity> query, TSearchObject? so);
}
```
#### Global Filter Query Builders

- Global filters apply to all entities implementing an interface and are **registered globally**
- uses the configured `TSearchObject` for the Entity who's Filter is being executed
- if no SearchObject is configured, a basic `SearchObject<TKey>` is provided

```csharp
// interface
public interface IGlobalFilteredQueryBuilder
{
    IQueryable<TEntity> Build<TEntity, TKey>(IQueryable<TEntity> query, ISearchObject<TKey>? so);
}
public interface IGlobalFilteredQueryBuilder<TEntity, TKey> : IGlobalFilteredQueryBuilder
{
    IQueryable<TEntity> Build(IQueryable<TEntity> query, ISearchObject<TKey>? so);
}
// base class
public abstract class GlobalFilteredQueryBuilderBase<TEntity> : GlobalFilteredQueryBuilderBase<TEntity, int>;
public abstract class GlobalFilteredQueryBuilderBase<TEntity, TKey> : FilteredQueryBuilderBase<TEntity, TKey, ISearchObject<TKey>>,
    IGlobalFilteredQueryBuilder<TEntity, TKey>
{
    IQueryable<TEntity> IGlobalFilteredQueryBuilder<TEntity, TKey>.Build(IQueryable<TEntity> query, ISearchObject<TKey>? so)
        => Build(query, so);
    IQueryable<T> IGlobalFilteredQueryBuilder.Build<T, TK>(IQueryable<T> query, ISearchObject<TK>? so)
        => Build(query.Cast<TEntity>(), so as ISearchObject<TKey>).Cast<T>();
}
```

#### Sort Query Builder

- uses the configured `TSortyBy`
- if no SortBy enum is configured, a basic `EntitySortBy` is provided

```csharp
// interface
public interface ISortedQueryBuilder<TEntity, TKey, TSortBy>
    where TEntity : IEntity<TKey>
    where TSortBy : struct, Enum
{
    IQueryable<TEntity> SortBy(IQueryable<TEntity> query, TSortBy? sortBy = null);
}
```

#### Include Query Builder

- uses the configured `TIncludes`
- if no Includes enum is configured, a basic `EntityIncludes` is provided

```csharp
// interface
public interface IIncludableQueryBuilder<TEntity, TKey, TIncludes>
    where TEntity : IEntity<TKey>
    where TIncludes : struct, Enum
{
    IQueryable<TEntity> AddIncludes(IQueryable<TEntity> query, TIncludes? includes = null);
}
```

### Entity Processors

Processors modify/decorate entities after fetching from database:

```csharp
// interface
public interface IEntityProcessor<TEntity, TIncludes>
    where TIncludes : struct, Enum
{
    Task Process(IList<TEntity> items, TIncludes? includes);
}
```

### Entity Preppers

Preppers prepare entities before saving. 
The original item is passed to enable advanced operations.

```csharp
// interface
public interface IEntityPrepper<in TEntity> : IEntityPrepper
{
    Task Prepare(TEntity modified, TEntity? original);
}
```

### Entity Primers

- Primers are executed as EF Core SaveChangesInterceptors by DbContext (via extension method `DbContextOptionsBuilder.AddPrimerInterceptors`)
- Primers can be registered **globally** (apply to an interface/base type) or **per entity**.

```csharp
// interface
public interface IEntityPrimer<in T>
{
    Task PrepareAsync(T entity, EntityEntry entry);
    bool CanPrepare(T entity);
}
```

## Dependency Injection

### Configuration Example

This example demonstrates how to configure entities with all helper services:

```csharp
// Configure DbContext with Interceptors
services.AddDbContext<MyDbContext>((sp, db) =>
{
    db.UseSqlServer(connectionString)
        .AddPrimerInterceptors(sp);
});

// Configure Entity Services with all helper services
services
    .UseEntities<MyDbContext>(options =>
    {
        // Global helper services (apply to all entities implementing an interface)
        options.AddGlobalFilterQueryBuilder<FilterIdsQueryBuilder<int>>();
        options.AddGlobalFilterQueryBuilder<FilterArchivablesQueryBuilder>();
        options.AddPrepper<IHasAggregateKey>(x => x.AggregateKey ??= Guid.NewGuid());
        options.AddPrimer<AutoTruncatePrimer>();
    })
    
    // Category
    .For<Category, Guid>(e =>
    {
        // Query Filter
        e.AddQueryFilter<CategoryQueryFilter>();
        
        // Sorting
        e.SortBy((query, sortBy) =>
        {
            return sortBy switch
            {
                EntitySortBy.Id => query.OrderBy(x => x.Id),
                EntitySortBy.IdDesc => query.OrderByDescending(x => x.Id),
                _ => query.OrderBy(x => x.Name)
            };
        });

        // Processor
        e.Process<CategoryProcessor>();
    })
    
    // Product
    .For<Product, ProductSearchObject, ProductSortBy, ProductIncludes>(e =>
    {
        // Query Filter (inline)
        e.Filter((query, so) =>
        {
            // filtering on Id is implemented by global filter
            if (so?.MinPrice != null)
                query = query.Where(x => x.Price >= so.MinPrice);
            if (so?.MaxPrice != null)
                query = query.Where(x => x.Price <= so.MaxPrice);
            return query;
        });
        
        // Sorting
        e.SortBy((query, sortBy) =>
        {
            return sortBy switch
            {
                ProductSortBy.Name => query.OrderBy(x => x.Name),
                ProductSortBy.NameDesc => query.OrderByDescending(x => x.Name),
                ProductSortBy.Price => query.OrderBy(x => x.Price),
                ProductSortBy.PriceDesc => query.OrderByDescending(x => x.Price),
                _ => query.OrderBy(x => x.Id)
            };
        });
        
        // Include
        e.Includes((query, includes) =>
        {
            if (includes?.HasFlag(ProductIncludes.Category) == true)
                query = query.Include(x => x.Category);
            if (includes?.HasFlag(ProductIncludes.Reviews) == true)
                query = query.Include(x => x.Reviews);
            return query;
        });
        
        // Processor
        e.Process((items, includes) =>
        {
            foreach (var item in items)
            {
                // Calculate display properties
                item.DisplayPrice = $"${item.Price:F2}";
            }
            return Task.CompletedTask;
        });
        
        // Prepper
        e.Prepare(item =>
        {
            // Ensure SKU is set
            item.Sku ??= GenerateSku(item);
        });
        
        // Primer
        e.AddPrimer<ProductPrimer>();
        
        // Related entities
        e.Related(x => x.Reviews);
    })
    
    // Order
    .For<Order, int, OrderSearchObject, OrderSortBy, OrderIncludes>(e =>
    {
        e.AddQueryFilter<OrderQueryFilter>();
        
        e.SortBy((query, sortBy) =>
        {
            // Support ThenBy sorting
            if (query is IOrderedQueryable<Order> sortedQuery){
                return sortBy switch
                {
                    OrderSortBy.OrderNumber => sortedQuery.ThenBy(x => x.OrderNumber),
                    OrderSortBy.OrderDate => sortedQuery.ThenBy(x => x.OrderDate),
                    OrderSortBy.TotalAmount => sortedQuery.ThenBy(x => x.TotalAmount),
                    _ => sortedQuery.ThenByDescending(x => x.OrderDate)
                };
            }
            return sortBy switch
            {
                OrderSortBy.OrderNumber => query.OrderBy(x => x.OrderNumber),
                OrderSortBy.OrderDate => query.OrderBy(x => x.OrderDate),
                OrderSortBy.TotalAmount => query.OrderBy(x => x.TotalAmount),
                _ => query.OrderByDescending(x => x.OrderDate)
            };
        });
        
        e.Includes<OrderIncludableQueryBuilder>();
        
        e.Process<OrderProcessor>();
        
        // Complex prepper with DbContext
        e.Prepare(async (item, dbContext) =>
        {
            // Recalculate order totals
            item.TotalAmount = item.OrderItems?.Sum(x => x.Quantity * x.UnitPrice) ?? 0;
            await Task.CompletedTask;
        });
        
        e.Related(x => x.OrderItems, (item, _) => item.OrderItems?.Prepare());
    });
```

**Registration Order Matters**

1. **Global services** execute first (registered on `EntityServiceCollectionOptions`)
2. **Entity-specific services** execute next (registered on entity builder)

**Tip**:

```csharp
// Use extension methods to configure Entities
public static class ProductServiceCollectionExtensions
{
    public static IEntityServiceCollection<TContext> AddProducts<TContext>(this IEntityServiceCollection<TContext> services)
        where TContext : DbContext
    {
        services
            .For<Product>(e =>
            {
                // put logic here ...
            });
        return services;
    }
}

// Resulting:
services
    .UseEntities<MyDbContext>(/* ... */)
    .AddProducts()
    .AddCategories()
    .AddOrders();
```

## Next Steps

- ToDo