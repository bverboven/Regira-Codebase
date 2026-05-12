# Entity Mapping

## EntityMapper

- A custom mapper should implement interface `IEntityMapper`
- A base class `EntityMapperBase` is provided with support for AfterMappers
- Built-in support for **AutoMapper** and **Mapster**.


## AfterMapper

- They can be configured **globally** (interface based) or for specific **entity** types
- After mappers decorate DTOs **after the mapping engine** completes
- A base class `EntityAfterMapperBase` is provided

```csharp
// interface
public interface IEntityAfterMapper
{
    bool CanMap(object source);
    void AfterMap(object source, object target);
}
public interface IEntityAfterMapper<in TSource, in TTarget> : IEntityAfterMapper
{
    void AfterMap(TSource source, TTarget target);
}
// base class
public abstract class EntityAfterMapperBase<TSource, TTarget> : IEntityAfterMapper<TSource, TTarget>
{
    public abstract void AfterMap(TSource source, TTarget target);
    public bool CanMap(object source)
}
```

## Dependency Injection

```csharp   
services
    .UseEntities<MyDbContext>(options => {
        // ...

        options.UseMapsterMapping();
        // or
        options.UseAutoMapper();

        // global AfterMapper
        options.AfterMap<IMyInterface, MyModel, MyAfterMapper>();
    })
    .For<Order>(e =>
    {
        // ...

        e.UseMapping<OrderDto, OrderInputDto>()
            .After((item, dto) => { /*...*/ })
            .AfterInput((dto, item) => { /*...*/ });
        
        // extra mapping config
        e.AddMapping<OrderItem, OrderItemDto>();
        e.AddMapping<OrderItemInputDto, OrderItem>();
    });

```

## Overview

1. [Index](../README.md) — Overview of Regira Entities
1. [Entity Models](models.md) — Creating and structuring entity models
1. [Services](services.md) — Implementing entity services and repositories
1. **[Mapping](mapping.md)** — Mapping Entities to and from DTOs
1. [Web Endpoints](web-endpoints.md) — Exposing entity operations as HTTP endpoints
1. [Normalizing](normalizing.md) — Data normalization techniques
1. [Attachments](attachments.md) — Managing file attachments
1. [Built-in Features](built-in-features.md) — Ready to use components
1. [Checklist](checklist.md) — Step-by-step guide for common tasks
1. [Practical Examples](examples.md) — Complete implementation examples
