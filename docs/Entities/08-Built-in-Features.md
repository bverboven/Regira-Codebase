# Ready to use Features

## Entity Services

### Wrapping service

**EntityWrappingServiceBase**: *Create a pipeline of services that wrap around an inner service.
Different responsibilities can be implemented in separate services.*

Samples:
- Auditing (Can also be done using Primers for write operations)
- Security
- Validation

### Input Validation

**EntityInputException**: Caught by Controllers and returned as BadRequest (400).

```csharp
public class EntityInputException<T>(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public T? Item { get; set; }
    public IDictionary<string, string> InputErrors { get; set; } = new Dictionary<string, string>();
}
```


## DbContext

**SetDecimalPrecisionConvention**: *Automatically configures decimal properties.*

```csharp
// In DbContext class
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.SetDecimalPrecisionConvention(18, 2);
}
```

**AddAutoTruncateInterceptors**: *Truncates string properties based on MaxLength attribute before saving to database.
Adds a global `AutoTruncatePrimer` as interceptor*.

```csharp
services.AddDbContext<MyDbContext>((serviceProvider, db) =>
{
    db.UseSqlServer(connectionString)
        .AddAutoTruncateInterceptors();
});
```

## Helper Services

### Preppers

**RelatedCollectionPrepper**: *Prepares related collections for saving by adding, updating, or removing items as necessary.*

```csharp
// use shortcut when configuring Entity (creates RelatedCollectionPrepper in background)
.For<Order>(e => {
    e.Related(x => x.OrderItems, (item, _) => item.OrderItems?.Prepare());
});
```

### Primers

**ArchivablePrimer**: *Sets Created/Modified timestamps and handles archiving logic.*

**HasCreatedDbPrimer**: *Sets Created timestamp on new entities implementing `IHasCreated`.*

**HasLastModifiedDbPrimer**: *Sets LastModified timestamp when updating entities implementing `IHasLastModified`.*


### Query Builders

`FilterArchivablesQueryBuilder`
`FilterHasCreatedQueryBuilder`
`FilterHasLastModifiedQueryBuilder`
`FilterHasNormalizedContentQueryBuilder`
`FilterIdsQueryBuilder`


### Query Extensions

```csharp
public static class QueryExtensions
{
    public static IQueryable<TEntity> FilterId<TEntity, TKey>(this IQueryable<TEntity> query, TKey? id)
    public static IQueryable<TEntity> FilterIds<TEntity, TKey>(this IQueryable<TEntity> query, ICollection<TKey>? ids)
    public static IQueryable<TEntity> FilterExclude<TEntity, TKey>(this IQueryable<TEntity> query, ICollection<TKey>? ids)

    public static IQueryable<TEntity> FilterCode<TEntity>(this IQueryable<TEntity> query, string? code)

    public static IQueryable<TEntity> FilterTitle<TEntity>(this IQueryable<TEntity> query, ParsedKeywordCollection? keywords)
    public static IQueryable<TEntity> FilterNormalizedTitle<TEntity>(this IQueryable<TEntity> query, ParsedKeywordCollection? keywords)

    public static IQueryable<TEntity> FilterCreated<TEntity>(this IQueryable<TEntity> query, DateTime? minDate, DateTime? maxDate)
    public static IQueryable<TEntity> FilterLastModified<TEntity>(this IQueryable<TEntity> query, DateTime? minDate, DateTime? maxDate)
    public static IQueryable<TEntity> FilterTimestamps<TEntity>(this IQueryable<TEntity> query, DateTime? minCreated, DateTime? maxCreated, DateTime? minModified, DateTime? maxModified)

    public static IQueryable<TEntity> FilterQ<TEntity>(this IQueryable<TEntity> query, ParsedKeywordCollection? keywords)
    public static IQueryable<TEntity> FilterArchivable<TEntity>(this IQueryable<TEntity> query, bool? isArchived)

    public static IQueryable<TEntity> FilterHasAttachment<TEntity>(this IQueryable<TEntity> query, bool? hasAttachment)

    public static IQueryable<TEntity> SortQuery<TEntity, TKey>(this IQueryable<TEntity> query)
}
```


### Pagination

```csharp
public static class QueryExtensions
{
    public static IQueryable<T> PageQuery<T>(this IQueryable<T> query, PagingInfo? info)
    public static IQueryable<T> PageQuery<T>(this IQueryable<T> query, int pageSize, int page = 1)
}
```

## Entity Extensions

```csharp
public static class EntityExtensions
{
    public static bool IsNew<TKey>(this IEntity<TKey> item)
    public static void SetSortOrder(this IEnumerable<ISortable> items)
}
```

## Entity Interfaces

| Interface | Properties | Services | Info |
|-----------|-----------|-------------|-------------|
| `IEntity<TKey>` | Id (TKey) | `FilterIdsQueryBuilder` | Define type of Primary Key |
| `IEntityWithSerial` | Id (int) | *see `IEntity<int>`* | Auto-incrementing int ID |
| `IHasCode` | Code (string) | *`Normalizers`* | Entities with short code |
| `IHasTitle` | Title (string) | *`Normalizers`* | Name, title, short description to display |
| `IHasDescription` | Description (string) | *`Normalizers`* | Entities with description field |
| `IHasCreated` | Created (DateTime) | `HasCreatedDbPrimer` | Track creation time |
| `IHasLastModified` | LastModified (DateTime?) | `HasLastModifiedDbPrimer` | Track modification time |
| `IHasTimestamps` | Created, LastModified | see `IHasCreated` & `IHasLastModified` | Both timestamps |
| `IArchivable` | IsArchived (bool) | `ArchivablePrimer` | Soft delete capability |
| `ISortable` | SortOrder (int) | *`Preppers`* -> `EntityExtensions.SetSortOrder` | Sortable as (child) collection |
