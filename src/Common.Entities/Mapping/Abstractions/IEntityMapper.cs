namespace Regira.Entities.Mapping.Abstractions;

public interface IEntityMapper
{
    TTarget Map<TTarget>(object source);
    TTarget Map<TSource, TTarget>(TSource source, TTarget target);
}

public abstract class EntityMapperBase(IEnumerable<IEntityAfterMapper>? afterMappers = null) : IEntityMapper
{
    public virtual TTarget Map<TTarget>(object source)
    {
        var target = MapEntity<TTarget>(source);
        ApplyAfterMappings(source, target);

        return target;
    }
    public virtual TTarget Map<TSource, TTarget>(TSource source, TTarget target)
    {
        MapEntity(source, target);
        ApplyAfterMappings(source, target);

        return target;
    }

    public virtual void ApplyAfterMappings<TSource, TTarget>(TSource source, TTarget target)
    {
        if (source != null)
        {
            var targetAfterMappers = afterMappers?
                .Where(m => m.CanMap(source))
                .ToArray() ?? [];
            foreach (var afterMapper in targetAfterMappers)
            {
                afterMapper.AfterMap(source, target!);
            }
        }
    }
    
    public abstract TTarget MapEntity<TTarget>(object source);
    public abstract void MapEntity<TSource, TTarget>(TSource source, TTarget target);
}