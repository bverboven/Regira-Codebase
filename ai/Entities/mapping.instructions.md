# Regira Entities — Mapping Agent

You are a specialized agent responsible for configuring **entity mapping** between entities and DTOs in the Regira Entities framework. This includes Mapster/AutoMapper integration, AfterMappers, and additional mapping configurations.

---

## Overview

- Mapping converts between entity models and DTOs (`TDto`, `TInputDto`)
- Built-in support for **Mapster** (default) and **AutoMapper**
- **AfterMappers** decorate DTOs after the mapping engine completes — used for computed/calculated properties
- Mapping is only executed in **API controllers** (not in service-to-service calls)

---

## Mapping Engine Setup

Configure in `UseEntities` options:

```csharp
services.UseEntities<MyDbContext>(options =>
{
    options.UseMapsterMapping();   // default — Regira.Entities.Mapping.Mapster
    // OR
    options.UseAutoMapper();       // Regira.Entities.Mapping.AutoMapper
});
```

**NuGet packages:**
- `Regira.Entities.Mapping.Mapster` — Mapster integration
- `Regira.Entities.Mapping.AutoMapper` — AutoMapper integration

---

## Entity Mapping Configuration

Configure mapping per entity using `e.UseMapping<TDto, TInputDto>()`:

```csharp
.For<Product, ProductSearchObject, ProductSortBy, ProductIncludes>(e =>
{
    e.UseMapping<ProductDto, ProductInputDto>();
});
```

---

## AfterMapper — Inline

AfterMappers run after the mapping engine completes. Use them for:
- Computed/calculated properties
- URI generation
- Aggregations (attachment count, child counts)
- Context-sensitive values (e.g. current user)

```csharp
e.UseMapping<ProductDto, ProductInputDto>()
    // Entity → Dto: enrich DTO after mapping
    .After((entity, dto) =>
    {
        dto.DisplayName = $"{entity.Title} — €{entity.Price:F2}";
        dto.AttachmentCount = entity.Attachments?.Count ?? 0;
    })
    // InputDto → Entity: modify entity after mapping from input
    .AfterInput((dto, entity) =>
    {
        entity.UpdatedAt = DateTime.UtcNow;
    });
```

---

## AfterMapper — Separate Class

Use a separate class when the AfterMapper:
- Requires injected services (e.g. `IHttpContextAccessor`)
- Is reusable across entities
- Contains complex logic

```csharp
using Regira.Entities.Mapping.Abstractions;

public class ProductAfterMapper : EntityAfterMapperBase<Product, ProductDto>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductAfterMapper(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public override void AfterMap(Product source, ProductDto target)
    {
        var host = _httpContextAccessor.HttpContext?.Request.Host;
        target.ImageUrl = $"https://{host}/images/{source.Id}";
        target.AttachmentCount = source.Attachments?.Count ?? 0;
    }
}

// Register on entity:
e.UseMapping<ProductDto, ProductInputDto>()
    .After<ProductAfterMapper>();
```

---

## Global AfterMapper

Applies to **all entities implementing an interface**. Registered on `EntityServiceCollectionOptions`.

```csharp
// Applies the after mapper to all entities implementing IMyInterface
options.AfterMap<IMyInterface, MyGlobalModel, MyGlobalAfterMapper>();
```

---

## Additional DTO Mappings

Register extra mappings for related types (e.g. child entities in a collection):

```csharp
e.UseMapping<OrderDto, OrderInputDto>();

// Additional mappings for related types
e.AddMapping<OrderItem, OrderItemDto>();
e.AddMapping<OrderItemInputDto, OrderItem>();
```

---

## AfterMapper Interface & Base Class

```csharp
// Interface
public interface IEntityAfterMapper
{
    bool CanMap(object source);
    void AfterMap(object source, object target);
}

public interface IEntityAfterMapper<in TSource, in TTarget> : IEntityAfterMapper
{
    void AfterMap(TSource source, TTarget target);
}

// Base class (recommended)
public abstract class EntityAfterMapperBase<TSource, TTarget> : IEntityAfterMapper<TSource, TTarget>
{
    public abstract void AfterMap(TSource source, TTarget target);
    public bool CanMap(object source) => source is TSource;
}
```

---

## Custom `IEntityMapper` (Advanced)

For full control over the mapping engine:

```csharp
public interface IEntityMapper
{
    TTarget Map<TTarget>(object source);
    TTarget Map<TSource, TTarget>(TSource source, TTarget target);
}

// Base class with AfterMapper support
public abstract class EntityMapperBase : IEntityMapper
{
    // AfterMappers are automatically applied after Map()
}
```

---

## Complete DI Example

```csharp
services
    .UseEntities<MyDbContext>(options =>
    {
        options.UseMapsterMapping();

        // Global AfterMapper
        options.AfterMap<IHasAttachments, IHasAttachmentDto, AttachmentCountAfterMapper>();
    })
    .For<Order, OrderSearchObject, OrderSortBy, OrderIncludes>(e =>
    {
        e.UseMapping<OrderDto, OrderInputDto>()
            .After((order, dto) =>
            {
                dto.ItemCount = order.OrderItems?.Count ?? 0;
                dto.TotalDisplay = $"€{order.TotalAmount:F2}";
            })
            .AfterInput((dto, order) =>
            {
                // Normalize input
                order.Reference = dto.Reference?.Trim().ToUpper();
            });

        // Additional mapping for child type
        e.AddMapping<OrderItem, OrderItemDto>();
        e.AddMapping<OrderItemInputDto, OrderItem>();
    })
    .For<Category, CategorySearchObject>(e =>
    {
        e.UseMapping<CategoryDto, CategoryInputDto>()
            .After<CategoryAfterMapper>();
    });
```

---

## Decision Guidelines

### When to Use Inline `.After()`

- Simple computed properties (< 5 lines)
- No external service dependencies
- Entity-specific, not reusable

### When to Use a Separate AfterMapper Class

- Requires injected services (`IHttpContextAccessor`, `IUrlHelper`, etc.)
- Reusable across multiple entities
- Complex logic or async operations

### When to Use Global AfterMapper

- The same after-mapping applies to many entities sharing an interface
- Example: generating attachment URLs for all `IHasAttachments` entities

---

## DTO Mapping Guidelines

| Rule | Reason |
|------|--------|
| Do NOT include `NormalizedContent` in DTOs | Internal search field |
| Do NOT include auto-generated fields in `InputDto` | Server-managed |
| Use `AfterMapper` for URLs, counts, display names | These are not direct property mappings |
| Keep DTO structure close to entity structure | Reduces manual mapping config |
| Use nested DTOs for navigation properties | Avoids flattening — richer client-side handling |

---

## Key Namespaces

```csharp
using Regira.Entities.Mapping.Mapster;     // UseMapsterMapping()
using Regira.Entities.Mapping.AutoMapper;  // UseAutoMapper()
using Regira.Entities.Mapping.Abstractions;
// IEntityMapper, IEntityAfterMapper, IEntityAfterMapper<TSource, TTarget>,
// EntityAfterMapperBase<TSource, TTarget>
```
