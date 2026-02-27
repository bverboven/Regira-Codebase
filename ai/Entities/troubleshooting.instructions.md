# Regira Entities — Troubleshooting Agent

You are a specialized agent responsible for diagnosing and resolving common issues in the Regira Entities framework. Use the checklists and resolutions below to identify root causes and fix problems.

---

## Interceptors & Startup

### Primers not running (Created/LastModified/IsArchived not set)

**Cause:** `AddPrimerInterceptors(sp)` is missing or called without the `IServiceProvider`.

**Fix:** Always use the `(sp, options) =>` factory overload of `AddDbContext`:

```csharp
builder.Services.AddDbContext<YourDbContext>((sp, options) =>
    options.UseSqlite(connectionString)
           .AddPrimerInterceptors(sp)       // ← sp required
           .AddNormalizerInterceptors(sp)
           .AddAutoTruncateInterceptors());
```

> **Never** use the parameterless overload `AddDbContext<T>(options => ...)` when you need interceptors — it has no access to the DI container.

---

### Normalizer not running (NormalizedContent not populated)

**Cause:** `AddNormalizerInterceptors(sp)` is missing, or the wrong `AddDbContext` overload is used.

**Fix:** Same as above — use `(sp, options) =>` and call `.AddNormalizerInterceptors(sp)`.

Also ensure `UseDefaults()` (or `AddDefaultEntityNormalizer()`) is called in `UseEntities`:

```csharp
services.UseEntities<YourDbContext>(options =>
{
    options.UseDefaults(); // registers DefaultNormalizer, ObjectNormalizer, DefaultEntityNormalizer, QKeywordHelper
});
```

---

### `AutoTruncatePrimer` not truncating strings / string truncation exceptions

**Cause:** `AddAutoTruncateInterceptors()` is missing in `AddDbContext`.

**Fix:**
```csharp
options.AddAutoTruncateInterceptors(); // call after UseSqlite/UseSqlServer
```

---

## Query & Filtering

### Filter not applied

**Cause:** Query builder not registered, or `SearchObject` property name doesn't match the builder logic.

**Checklist:**
- Verify `e.AddQueryFilter<YourQueryBuilder>()` is called, or `e.Filter(...)` is configured
- Check property names in the query builder match the `SearchObject`
- Ensure the entity is registered with `.For<TEntity,...>(e => { ... })`

---

### Navigation properties not loaded (null on DTO)

**Cause:** `Includes` config missing, wrong flag value, or the client isn't sending the `includes` parameter.

**Checklist:**
- Verify `e.Includes(...)` is configured for the entity
- Check the `[Flags]` enum value is correct (powers of 2: `1 << 0`, `1 << 1`, …)
- Confirm the client sends `?includes=1` (or the combined flag) on the request
- Ensure the `Include(x => x.NavigationProperty)` call is inside the flag check

```csharp
e.Includes((query, includes) =>
{
    if (includes?.HasFlag(ProductIncludes.Category) == true)
        query = query.Include(x => x.Category);
    return query;
});
```

---

### `Q` search returns no results

**Cause:** `NormalizedContent` is not populated (normalizer not running), or `FilterHasNormalizedContentQueryBuilder` is not registered.

**Checklist:**
1. Verify `AddNormalizerInterceptors(sp)` is called (see above)
2. Verify `UseDefaults()` is called (registers `FilterHasNormalizedContentQueryBuilder`)
3. Verify the entity implements `IHasNormalizedContent`
4. Verify the `[Normalized]` attribute is applied to `NormalizedContent`
5. Re-save an entity and check if `NormalizedContent` is now populated

---

## Write & Persistence

### Save not persisting (changes not saved to database)

**Cause:** `SaveChanges()` was not called.

**Rule:** Write methods (`Save`, `Add`, `Modify`, `Remove`) do **not** auto-persist — the caller must call `SaveChanges()`:

```csharp
await service.Save(item);
await service.SaveChanges(); // ← required when calling service directly
```

> The base `EntityControllerBase` calls `SaveChanges()` automatically. Custom code (e.g. custom controller actions, manual service calls) must call it explicitly.

---

### Soft delete not working (DELETE removes record instead of archiving)

**Cause:** Entity does not implement `IArchivable`, or `ArchivablePrimer` is not registered.

**Checklist:**
- Entity must implement `IArchivable`: `public bool IsArchived { get; set; }`
- `ArchivablePrimer` must be registered — call `options.UseDefaults()` or `options.AddPrimer<ArchivablePrimer>()`
- `AddPrimerInterceptors(sp)` must be present in `AddDbContext` (see above)

---

### Mapping errors (object mapping fails or returns empty DTO)

**Cause:** Mapping engine not configured, property name mismatch, or `UseMapping` not called for the entity.

**Checklist:**
- Ensure `options.UseMapsterMapping()` (or `options.UseAutoMapper()`) is called in `UseEntities`
- Ensure `e.UseMapping<TDto, TInputDto>()` is called for the entity
- Check that DTO property names match entity property names (Mapster/AutoMapper is case-sensitive by convention)
- For child types (e.g. `OrderItem → OrderItemDto`), register additional mappings with `e.AddMapping<>()`

---

## Controllers & Services

### `IEntityService` not injected / controller cannot resolve the service

**Cause:** The service is injected via constructor — this is incorrect.

**Fix:** The base controller resolves `IEntityService` automatically via `HttpContext.RequestServices`. In custom actions, resolve it explicitly:

```csharp
// ✗ Wrong — do not inject in constructor
public MyController(IEntityService<Product, ...> service) { }

// ✓ Correct — resolve from request services in custom actions
var service = HttpContext.RequestServices
    .GetRequiredService<IEntityService<Product, int, ProductSearchObject, ProductSortBy, ProductIncludes>>();
```

---

### `EntityWrappingServiceBase` causes infinite loop / stack overflow

**Cause:** The wrapping service is resolving itself as the inner service.

**Fix:** Register the wrapper with `UseEntityService<T>()` (replaces the default `EntityRepository`), and register the interface separately with `AddTransient`:

```csharp
e.AddTransient<IProductService, ProductService>(); // registers the interface
e.UseEntityService<ProductService>();              // replaces EntityRepository as inner service
```

Do **not** register the wrapper as both the interface and the implementation of `IEntityService<...>` directly — that creates a circular dependency.

---

### Custom entity service (`UseEntityService`) is not being used

**Cause:** `e.UseEntityService<T>()` was not called, or the service is only registered via `AddTransient` (which makes it available by interface, but doesn't replace the `EntityRepository`).

**Fix:** Call both when using a wrapping service with a custom interface:

```csharp
e.AddTransient<IProductService, ProductService>(); // typed injection by interface
e.UseEntityService<ProductService>();              // replaces EntityRepository
```

---

## Dependency Injection

### `UseDefaults()` not called — missing primers, filters, or normalizers

**Cause:** `options.UseDefaults()` was omitted from `UseEntities`.

**Fix:** Add it to the configuration:

```csharp
services.UseEntities<YourDbContext>(options =>
{
    options.UseDefaults(); // registers primers, global query filters, normalizer services
    options.UseMapsterMapping();
});
```

`UseDefaults()` registers:
- **Primers:** `ArchivablePrimer`, `HasCreatedDbPrimer`, `HasLastModifiedDbPrimer`
- **Global query filters:** `FilterIdsQueryBuilder`, `FilterArchivablesQueryBuilder`, `FilterHasCreatedQueryBuilder`, `FilterHasLastModifiedQueryBuilder`
- **Normalizer services:** `DefaultNormalizer`, `ObjectNormalizer`, `DefaultEntityNormalizer`, `QKeywordHelper`

---

### Entity not found in DI / controller returns 500 for unknown entity type

**Cause:** The entity was not registered with `.For<TEntity,...>(e => { ... })`.

**Fix:** Register the entity in the DI entry point:

```csharp
services.UseEntities<YourDbContext>(/* ... */)
    .For<Product, ProductSearchObject, ProductSortBy, ProductIncludes>(e =>
    {
        // configure here
    });
```

Also ensure the `DbSet<TEntity>` exists in the `DbContext`.

---

## Migrations

### Migration fails or DbContext cannot be found by EF tools

**Cause:** `Microsoft.EntityFrameworkCore.Design` package is missing, or the startup project is wrong.

**Checklist:**
- Add `<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="*" />` to the project
- Run `dotnet ef migrations add ...` from the project directory (not the solution root)
- If using a separate data project, specify `--startup-project` and `--project`

---

### Migration applied but schema is wrong

**Cause:** Migration was generated before the latest entity changes were saved.

**Fix:** Always generate a new migration after changing entity classes or `DbContext` configuration:

```bash
dotnet ef migrations add Update_{Description}
dotnet ef database update
```

---

## Quick Reference Checklist

| Problem | Likely Cause | Fix |
|---------|-------------|-----|
| Primers not running | `AddPrimerInterceptors(sp)` missing or wrong overload | Use `(sp, options) =>` factory overload in `AddDbContext` |
| Normalizer not running | `AddNormalizerInterceptors(sp)` missing | Same as above; also call `UseDefaults()` |
| `AddPrimerInterceptors` compile error (0 args) | Missing `IServiceProvider` | Use `(sp, options) =>` factory overload; pass `sp` |
| Navigation properties null | Missing `e.Includes(...)` or client not sending flag | Check `Includes` config and request parameters |
| Filter not applied | Query builder not registered | Verify `e.AddQueryFilter<>()` or `e.Filter(...)` |
| `Q` search returns nothing | Normalizer not running or `FilterHasNormalizedContentQueryBuilder` missing | Fix interceptors; call `UseDefaults()` |
| Save not persisting | `SaveChanges()` not called | Call `service.SaveChanges()` explicitly in custom code |
| Soft delete not working | `IArchivable` not implemented or `ArchivablePrimer` missing | Implement `IArchivable`; call `UseDefaults()` |
| Mapping errors | Mapping engine not configured or `UseMapping` missing | Call `UseMapsterMapping()` and `e.UseMapping<TDto, TInputDto>()` |
| Controller can't resolve service | `IEntityService` injected in constructor | Use `HttpContext.RequestServices.GetRequiredService<>()` |
| `EntityWrappingServiceBase` infinite loop | Wrapper resolves itself as inner service | Use `UseEntityService<T>()` to replace `EntityRepository` |
| `UseEntityService` not taking effect | Only `AddTransient` called, not `UseEntityService` | Call both `AddTransient<IMyService, MyService>()` and `UseEntityService<MyService>()` |
| Entity not registered | `.For<TEntity,...>()` missing | Register in `UseEntities` and add `DbSet<T>` to `DbContext` |
````
