using Regira.Utilities;

namespace Regira.Entities.Mapping.Abstractions;

public interface IEntityAfterMapper
{
    bool CanMap(object source);
    void AfterMap(object source, object target);
}
public interface IEntityAfterMapper<in TSource, in TTarget> : IEntityAfterMapper
{
    void AfterMap(TSource source, TTarget target);
}
public abstract class EntityAfterMapperBase<TSource, TTarget> : IEntityAfterMapper<TSource, TTarget>
{
    public abstract void AfterMap(TSource source, TTarget target);
    public bool CanMap(object source)
        => TypeUtility.ImplementsType<TSource>(source.GetType());
    void IEntityAfterMapper.AfterMap(object source, object target)
        => AfterMap((TSource)source, (TTarget)target);
}
public class EntityAfterMapper<TSource, TTarget>(Action<TSource, TTarget> afterMapAction) : EntityAfterMapperBase<TSource, TTarget>
{
    public override void AfterMap(TSource source, TTarget target)
        => afterMapAction(source, target);
}