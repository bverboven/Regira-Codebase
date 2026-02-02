# Entity Services

The `IEntityService` is the core service interface for managing entities. It provides standard CRUD operations and can be customized or extended as needed.

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

- Primers are executed as EF Core SaveChangesInterceptors by DbContext. 
- Primers can be registered **globally** (apply to an interface/base type) or **per entity**.

```csharp
// interface
public interface IEntityPrimer<in T>
{
    Task PrepareAsync(T entity, EntityEntry entry);
    bool CanPrepare(T entity);
}
```

## Next Steps

- ToDo