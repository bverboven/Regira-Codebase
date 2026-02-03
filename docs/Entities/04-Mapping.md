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
        => TypeUtility.ImplementsType<TSource>(source.GetType());
    void IEntityAfterMapper.AfterMap(object source, object target)
        => AfterMap((TSource)source, (TTarget)target);
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