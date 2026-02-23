---
name: Regira Services
description: >
  Configures entity DI registration (.For<>), query builders (filter/sort/includes),
  processors, preppers, primers, mapping, and AfterMappers for Regira Entities.
tools:
  - codebase
  - editFiles
handoffs:
  - label: "Services ready → add controller"
    agent: regira-controllers
    prompt: "Services are configured. Create the API controller(s) for the entities."
    send: false
  - label: "Need wrapping service?"
    agent: regira-wrapping-service
    prompt: "Configure a custom wrapping service for this entity."
    send: false
---

# Regira Entities — Services Agent

You configure the **service layer**: DI registration, query builders, processors,
preppers, primers, mapping, and AfterMappers.

Always use exact namespaces from `.github/instructions/regira-namespaces.instructions.md`.

---

## File Locations

```
Extensions/
└── ServiceCollectionExtensions.cs        ← root DI wiring
Entities/
└── {EntityName}s/
    └── {Entity}ServiceCollectionExtensions.cs
```

---

## 1 — Root DI (`ServiceCollectionExtensions.cs`)

```csharp
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;
using Regira.Entities.Mapping.Mapster;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
    {
        services
            .UseEntities<AppDbContext>(options =>
            {
                options.UseDefaults();         // primers + global filters + normalizer services
                options.UseMapsterMapping();   // default mapping engine
                // options.UseAutoMapper();    // alternative

                // Optional global inline prepper (applies to all entities implementing the interface)
                // options.AddPrepper<ISomeInterface>(x => x.Field ??= Guid.NewGuid());

                // Optional global primer
                // options.AddPrimer<MyGlobalPrimer>();

                // Optional global filter
                // options.AddGlobalFilterQueryBuilder<MyGlobalFilterBuilder>();
            })
            .AddCategories()
            .AddProducts()
            .AddOrders();

        return services;
    }
}
```

### What `UseDefaults()` registers

**Primers:** `ArchivablePrimer`, `HasCreatedDbPrimer`, `HasLastModifiedDbPrimer`

**Global filters:** `FilterIdsQueryBuilder`, `FilterArchivablesQueryBuilder`,
`FilterHasCreatedQueryBuilder`, `FilterHasLastModifiedQueryBuilder`

**Normalizer services:** `DefaultNormalizer` (`INormalizer`),
`ObjectNormalizer` (`IObjectNormalizer`),
`DefaultEntityNormalizer<IEntity>` (`IEntityNormalizer`),
`QKeywordHelper` (`IQKeywordHelper`)

---

## 2 — Per-Entity Extension Method

```csharp
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;

public static class {Entity}ServiceCollectionExtensions
{
    public static IEntityServiceCollection<TContext> Add{Entities}<TContext>(
        this IEntityServiceCollection<TContext> services)
        where TContext : DbContext
    {
        services.For<{Entity}, {Entity}SearchObject, {Entity}SortBy, {Entity}Includes>(e =>
        {
            e.AddQueryFilter<{Entity}QueryBuilder>();     // or e.Filter((q, so) => { ... });

            e.SortBy((query, sortBy) => { /* ... */ return query; });

            e.Includes((query, includes) => { /* ... */ return query; });

            e.UseMapping<{Entity}Dto, {Entity}InputDto>()
                .After((entity, dto) => { /* enrich DTO */ })
                .AfterInput((dto, entity) => { /* modify entity after input mapping */ });

            e.Process<{Entity}Processor>();              // or e.Process((items, inc) => { ... });
            e.Prepare<{Entity}Prepper>();                // or e.Prepare(item => { ... });
            e.AddPrimer<{Entity}Primer>();

            e.Related(x => x.ChildItems);               // child collection management
        });

        return services;
    }
}
```

### `.For<>()` Overloads

```csharp
services.For<{Entity}>(e => { })                                                      // int PK, no custom types
services.For<{Entity}, {Entity}SearchObject>(e => { })                                // + filter
services.For<{Entity}, {Entity}SearchObject, {Entity}SortBy, {Entity}Includes>(e => { })  // + sort + includes
services.For<{Entity}, Guid>(e => { })                                                // non-int PK
services.For<{Entity}, Guid, {Entity}SearchObject, {Entity}SortBy, {Entity}Includes>(e => { })
```

---

## 3 — Filter Query Builder

### Inline
```csharp
e.Filter((query, so) =>
{
    if (so?.CategoryId?.Any() == true)
        query = query.Where(x => so.CategoryId.Contains(x.CategoryId ?? 0));
    if (so?.MinPrice != null)
        query = query.Where(x => x.Price >= so.MinPrice);
    if (so?.MaxPrice != null)
        query = query.Where(x => x.Price <= so.MaxPrice);
    return query;
});
```

### Separate Class
```csharp
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

public class {Entity}QueryBuilder : FilteredQueryBuilderBase<{Entity}, int, {Entity}SearchObject>
{
    public override IQueryable<{Entity}> Build(IQueryable<{Entity}> query, {Entity}SearchObject? so)
    {
        if (so == null) return query;

        if (so.CategoryId?.Any() == true)
            query = query.Where(x => so.CategoryId.Contains(x.CategoryId ?? 0));

        if (so.MinPrice.HasValue)
            query = query.Where(x => x.Price >= so.MinPrice.Value);

        if (so.MaxPrice.HasValue)
            query = query.Where(x => x.Price <= so.MaxPrice.Value);

        return query;
    }
}
```

### Global Filter (all entities implementing an interface)
```csharp
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models.Abstractions;

public class FilterByTenantQueryBuilder : GlobalFilteredQueryBuilderBase<ITenantEntity, int>
{
    private readonly ITenantContext _tenant;
    public FilterByTenantQueryBuilder(ITenantContext tenant) => _tenant = tenant;

    public override IQueryable<ITenantEntity> Build(
        IQueryable<ITenantEntity> query, ISearchObject<int>? so)
        => query.Where(x => x.TenantId == _tenant.CurrentTenantId);
}
// Register: options.AddGlobalFilterQueryBuilder<FilterByTenantQueryBuilder>();
```

### Built-in Query Extension Methods (`using Regira.Entities.EFcore.QueryBuilders`)
```csharp
query.FilterId(so.Id)                      query.FilterIds(so.Ids)
query.FilterExclude(so.Exclude)            query.FilterCode(so.Code)
query.FilterTitle(keywords)                query.FilterNormalizedTitle(keywords)
query.FilterCreated(so.MinCreated, so.MaxCreated)
query.FilterLastModified(so.MinLastModified, so.MaxLastModified)
query.FilterArchivable(so.IsArchived)      query.FilterHasAttachment(so.HasAttachment)
query.FilterQ(keywords)
```

---

## 4 — Sorting

```csharp
e.SortBy((query, sortBy) =>
{
    // Support ThenBy for multi-column sorting
    if (typeof(IOrderedQueryable).IsAssignableFrom(query.Expression.Type) && query is IOrderedQueryable<{Entity}> sorted)
        return sortBy switch
        {
            {Entity}SortBy.Title     => sorted.ThenBy(x => x.Title),
            {Entity}SortBy.TitleDesc  => sorted.ThenByDescending(x => x.Title),
            {Entity}SortBy.Price     => sorted.ThenBy(x => x.Price),
            {Entity}SortBy.PriceDesc  => sorted.ThenByDescending(x => x.Price),
            _                        => sorted.ThenByDescending(x => x.Created)
        };

    return sortBy switch
    {
        {Entity}SortBy.Title      => query.OrderBy(x => x.Title),
        {Entity}SortBy.TitleDesc   => query.OrderByDescending(x => x.Title),
        {Entity}SortBy.Price      => query.OrderBy(x => x.Price),
        {Entity}SortBy.PriceDesc   => query.OrderByDescending(x => x.Price),
        {Entity}SortBy.Created    => query.OrderBy(x => x.Created),
        {Entity}SortBy.CreatedDesc => query.OrderByDescending(x => x.Created),
        _                         => query.OrderByDescending(x => x.Created)
    };
});
```

---

## 5 — Includes

```csharp
e.Includes((query, includes) =>
{
    if (includes?.HasFlag({Entity}Includes.Category) == true)
        query = query.Include(x => x.Category);
    if (includes?.HasFlag({Entity}Includes.Reviews) == true)
        query = query.Include(x => x.Reviews).ThenInclude(r => r.Author);
    return query;
});
```

---

## 6 — Processor (fills [NotMapped] properties after fetch)

### Inline
```csharp
e.Process((items, includes) =>
{
    foreach (var item in items)
        item.DisplayPrice = $"€{item.Price:F2}";
    return Task.CompletedTask;
});
```

### Separate Class
```csharp
using Regira.Entities.EFcore.Processing.Abstractions;

public class {Entity}Processor : IEntityProcessor<{Entity}, {Entity}Includes>
{
    private readonly IHttpContextAccessor _http;
    public {Entity}Processor(IHttpContextAccessor http) => _http = http;

    public Task Process(IList<{Entity}> items, {Entity}Includes? includes)
    {
        var host = _http.HttpContext?.Request.Host.Value;
        foreach (var item in items)
            item.ImageUrl = $"https://{host}/images/{item.Id}";
        return Task.CompletedTask;
    }
}
// Register: e.Process<{Entity}Processor>();
```

---

## 7 — Prepper (runs before saving)

```csharp
// Simple
e.Prepare(item => { item.Slug ??= item.Title.ToLowerInvariant().Replace(' ', '-'); });

// With original (null on create)
e.Prepare((modified, original) =>
{
    if (original == null) modified.InitCode = Guid.NewGuid().ToString("N")[..8];
});

// With DbContext
e.Prepare(async (item, db) =>
{
    item.Total = item.Lines?.Sum(x => x.Qty * x.UnitPrice) ?? 0;
    await Task.CompletedTask;
});
```

### Separate Class
```csharp
using Regira.Entities.EFcore.Preppers.Abstractions;

public class {Entity}Prepper : EntityPrepperBase<{Entity}>
{
    public override Task Prepare({Entity} modified, {Entity}? original)
    {
        modified.Slug ??= modified.Title.ToLowerInvariant().Replace(' ', '-');
        return Task.CompletedTask;
    }
}
// Register: e.Prepare<{Entity}Prepper>();
```

### Child Collections
```csharp
e.Related(x => x.OrderItems);   // minimal

e.Related(x => x.OrderItems, (order, _) =>
{
    order.OrderItems?.Prepare();
    foreach (var item in order.OrderItems ?? [])
        item.OrderId = order.Id;
});
```

---

## 8 — Primer (EF Core SaveChanges interceptor)

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Primers.Abstractions;

public class {Entity}Primer : EntityPrimerBase<{Entity}>
{
    public override Task PrepareAsync({Entity} entity, EntityEntry entry)
    {
        if (entry.State == EntityState.Added)
            entity.Code ??= Guid.NewGuid().ToString("N")[..8].ToUpper();
        return Task.CompletedTask;
    }
}
// Per-entity: e.AddPrimer<{Entity}Primer>();
// Global:     options.AddPrimer<{Entity}Primer>();
```

> Primers require `AddPrimerInterceptors(sp)` in `AddDbContext`.
> Always use the `(sp, options) =>` factory overload — never `options =>`.

---

## 9 — Mapping & AfterMappers

```csharp
e.UseMapping<{Entity}Dto, {Entity}InputDto>()
    .After((entity, dto) =>
    {
        // Navigation objects (e.g. dto.Category) are mapped automatically by Mapster
        // Use AfterMapper only for computed values Mapster cannot calculate
        dto.AttachmentCount = entity.Attachments?.Count ?? 0;
        dto.ImageUrl = $"/images/{entity.Id}";
    })
    .AfterInput((dto, entity) =>
    {
        // runs after InputDto → Entity, before save
    });

// Additional child-type mappings
e.AddMapping<OrderItem, OrderItemDto>();
e.AddMapping<OrderItemInputDto, OrderItem>();
```

### Separate AfterMapper Class
```csharp
using Regira.Entities.Mapping.Abstractions;

public class {Entity}AfterMapper : EntityAfterMapperBase<{Entity}, {Entity}Dto>
{
    public override void AfterMap({Entity} source, {Entity}Dto target)
    {
        // Use for computed properties; navigation is handled automatically by Mapster
        target.AttachmentCount = source.Attachments?.Count ?? 0;
    }
}
// Global (for all entities implementing an interface):
// options.AfterMap<IMyInterface, MyDto, {Entity}AfterMapper>();
```

---

## 10 — Validation (EntityInputException → HTTP 400)

```csharp
using Regira.Entities.Models;

throw new EntityInputException<{Entity}>("Validation failed")
{
    Item = item,
    InputErrors = new Dictionary<string, string>
    {
        [nameof({Entity}.Price)] = "Price must be >= 0",
        [nameof({Entity}.Title)] = "Title is required"
    }
};
```
