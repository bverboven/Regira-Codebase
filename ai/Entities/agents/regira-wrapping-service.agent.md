---
name: Regira Wrapping Service
description: >
  Creates custom entity services using EntityWrappingServiceBase for caching,
  validation, authorization, auditing, or other cross-cutting concerns
  at the service level.
tools:
  - codebase
  - editFiles
handoffs:
  - label: "Wrapping service done → configure DI"
    agent: regira-services
    prompt: "The wrapping service is ready. Register it with e.AddTransient<IService, Impl>() and e.UseEntityService<Impl>()."
    send: false
---

# Regira Entities — Wrapping Service Agent

You create **custom entity services** using `EntityWrappingServiceBase`.

Always use exact namespaces from `.github/instructions/regira-namespaces.instructions.md`.

---

## When to Use a Wrapping Service

| Use Case | Correct Approach |
|----------|----------------|
| Per-field validation | `EntityInputException` in a **Prepper** |
| Calculating fields before save | **Prepper** |
| Enriching fetched data | **Processor** |
| Timestamps, soft-delete, codes | **Primer** |
| **Caching at service level** | ✓ Wrapping service |
| **Cross-entity validation** | ✓ Wrapping service |
| **Authorization / ownership checks** | ✓ Wrapping service |
| **Audit logging around all reads/writes** | ✓ Wrapping service |
| **Typed interface for injection by other services** | ✓ Wrapping service |

---

## File Locations

```
Entities/{Entities}/
├── I{Entity}Service.cs    (optional — enables typed DI injection)
└── {Entity}Service.cs
```

---

## 1 — Custom Service Interface (Optional)

```csharp
using Regira.Entities.Services.Abstractions;

public interface I{Entity}Service
    : IEntityService<{Entity}, int, {Entity}SearchObject, {Entity}SortBy, {Entity}Includes>
{
    // Optional extra methods
    Task<IList<{Entity}>> GetPopular(int count);
}
```

---

## 2 — Wrapping Service Class

```csharp
using Regira.Entities.Services.Abstractions;
using Regira.Entities.Models;
using Regira.DAL.Paging;

public class {Entity}Service
    : EntityWrappingServiceBase<{Entity}, int, {Entity}SearchObject, {Entity}SortBy, {Entity}Includes>,
      I{Entity}Service
{
    private readonly ILogger<{Entity}Service> _logger;
    private readonly IMemoryCache _cache;

    public {Entity}Service(
        IEntityService<{Entity}, int, {Entity}SearchObject, {Entity}SortBy, {Entity}Includes> inner,
        ILogger<{Entity}Service> logger,
        IMemoryCache cache)
        : base(inner)      // always pass inner to base
    {
        _logger = logger;
        _cache = cache;
    }

    // Override only the methods that need custom behaviour.
    // Unoverridden methods delegate to base.Method() automatically.

    public override async Task<{Entity}?> Details(int id)
    {
        var key = $"{nameof({Entity})}:{id}";
        if (_cache.TryGetValue(key, out {Entity}? cached)) return cached;

        var result = await base.Details(id);
        if (result != null) _cache.Set(key, result, TimeSpan.FromMinutes(5));
        return result;
    }

    public override async Task Save({Entity} item)
    {
        if (item.Price < 0)
            throw new EntityInputException<{Entity}>("Validation failed")
            {
                Item = item,
                InputErrors = new Dictionary<string, string>
                {
                    [nameof({Entity}.Price)] = "Price must be >= 0"
                }
            };

        await base.Save(item);
        _cache.Remove($"{nameof({Entity})}:{item.Id}");
    }

    public override async Task Remove({Entity} item)
    {
        _logger.LogInformation("Deleting {Entity} {Id}", nameof({Entity}), item.Id);
        await base.Remove(item);
        _cache.Remove($"{nameof({Entity})}:{item.Id}");
    }

    public async Task<IList<{Entity}>> GetPopular(int count)
        => await List(null, new PagingInfo { PageSize = count, Page = 1 });
}
```

---

## 3 — All Overridable Methods

```csharp
// Read
Task<TEntity?> Details(TKey id)
Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null)
Task<IList<TEntity>> List(IList<TSearchObject?> so, IList<TSortBy> sortBy,
                          TIncludes? includes = null, PagingInfo? pagingInfo = null)
Task<long> Count(TSearchObject? so)
Task<long> Count(IList<TSearchObject?> so)

// Write — do NOT auto-persist; SaveChanges() must be called
Task Save(TEntity item)          // calls Add() or Modify() internally
Task Add(TEntity item)
Task<TEntity?> Modify(TEntity item)
Task Remove(TEntity item)
Task<int> SaveChanges(CancellationToken token = default)
```

---

## 4 — DI Registration

```csharp
services.For<{Entity}, {Entity}SearchObject, {Entity}SortBy, {Entity}Includes>(e =>
{
    // 1. Register interface for typed injection elsewhere (optional)
    e.AddTransient<I{Entity}Service, {Entity}Service>();

    // 2. Replace EntityRepository with the wrapping service
    e.UseEntityService<{Entity}Service>();

    // 3. Other config continues normally
    e.AddQueryFilter<{Entity}QueryBuilder>();
    e.UseMapping<{Entity}Dto, {Entity}InputDto>()
        .After((entity, dto) => { /* ... */ });
});
```

> **Both calls are needed** when you want typed interface injection AND want the controller to use the wrapper.

---

## 5 — Common Patterns

### Caching

```csharp
public class Cached{Entity}Service
    : EntityWrappingServiceBase<{Entity}, int, {Entity}SearchObject, {Entity}SortBy, {Entity}Includes>
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    public Cached{Entity}Service(
        IEntityService<{Entity}, int, {Entity}SearchObject, {Entity}SortBy, {Entity}Includes> inner,
        IMemoryCache cache) : base(inner) => _cache = cache;

    public override async Task<{Entity}?> Details(int id)
    {
        var key = $"{nameof({Entity})}:details:{id}";
        if (_cache.TryGetValue(key, out {Entity}? hit)) return hit;
        var result = await base.Details(id);
        if (result != null) _cache.Set(key, result, Ttl);
        return result;
    }

    public override async Task Save({Entity} item)
    {
        await base.Save(item);
        _cache.Remove($"{nameof({Entity})}:details:{item.Id}");
    }
}
```

### Authorization / Ownership

```csharp
public class Secured{Entity}Service
    : EntityWrappingServiceBase<{Entity}, int, {Entity}SearchObject, {Entity}SortBy, {Entity}Includes>
{
    private readonly ICurrentUserService _currentUser;

    public Secured{Entity}Service(
        IEntityService<{Entity}, int, {Entity}SearchObject, {Entity}SortBy, {Entity}Includes> inner,
        ICurrentUserService currentUser) : base(inner) => _currentUser = currentUser;

    public override async Task Save({Entity} item)
    {
        if (!_currentUser.CanEdit(item))
            throw new UnauthorizedAccessException("You do not own this resource.");
        await base.Save(item);
    }

    public override async Task Remove({Entity} item)
    {
        if (!_currentUser.CanDelete(item))
            throw new UnauthorizedAccessException("You cannot delete this resource.");
        await base.Remove(item);
    }
}
```

### Audit Logging

```csharp
public class Audited{Entity}Service
    : EntityWrappingServiceBase<{Entity}, int, {Entity}SearchObject, {Entity}SortBy, {Entity}Includes>
{
    private readonly ILogger<Audited{Entity}Service> _logger;

    public Audited{Entity}Service(
        IEntityService<{Entity}, int, {Entity}SearchObject, {Entity}SortBy, {Entity}Includes> inner,
        ILogger<Audited{Entity}Service> logger) : base(inner) => _logger = logger;

    public override async Task<{Entity}?> Details(int id)
    {
        var result = await base.Details(id);
        _logger.LogInformation("Read {Entity} {Id}", nameof({Entity}), id);
        return result;
    }

    public override async Task Save({Entity} item)
    {
        var isNew = item.Id == 0;
        await base.Save(item);
        _logger.LogInformation("{Action} {Entity} {Id}",
            isNew ? "Created" : "Updated", nameof({Entity}), item.Id);
    }

    public override async Task Remove({Entity} item)
    {
        await base.Remove(item);
        _logger.LogWarning("Deleted {Entity} {Id}", nameof({Entity}), item.Id);
    }
}
```

---

## Common Pitfalls

| Pitfall | Fix |
|---------|-----|
| Infinite loop (wrapper wraps itself) | `AddTransient<IService, Impl>` registers the interface; `UseEntityService<Impl>` registers the wrapper. Do not call `UseEntityService` twice. |
| `Save()` not persisting | `Save()` stages changes only — `SaveChanges()` commits. The base controller calls it automatically; custom code must call it explicitly. |
| Constructor dependencies not resolving | Ensure all injected services are registered in DI before `UseEntityService<>()` is called. |
