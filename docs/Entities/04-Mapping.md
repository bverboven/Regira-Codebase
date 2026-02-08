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

1. [Index](01-Index.md) - Overview of Regira Entities
1. [Entity Models](02-Models.md) - Creating and structuring entity models
1. [Services](03-Services.md) - Implementing entity services and repositories
1. **[Mapping](04-Mapping.md)** - Mapping Entities to and from DTOs
1. [Controllers](05-Controllers.md) - Creating Web API controllers
1. [Normalizing](06-Normalizing.md) - Data normalization techniques
1. [Attachments](07-Attachments.md) - Managing file attachments] 
1. [Built-in Features](08-Built-in-Features.md) - Ready to use components
1. [Checklist](09-Checklist.md) - Step-by-step guide for common tasks
1. [Practical Examples](10-Examples.md) - Complete implementation examples
