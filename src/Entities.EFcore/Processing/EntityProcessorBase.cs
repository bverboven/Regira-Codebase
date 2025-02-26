using Regira.Entities.EFcore.Processing.Abstractions;

namespace Regira.Entities.EFcore.Processing;

public class EntityProcessor<TEntity, TIncludes>(Func<IList<TEntity>, TIncludes?, Task>? process = null)
    : IEntityProcessor<TEntity, TIncludes>
        where TIncludes : struct, Enum
{
    //Task IEntityProcessor.Process<T, TIncludes>(IList<T> items, TIncludes? includes)
    //    => Process(items.OfType<TEntity>().ToList(), includes);

    public virtual Task Process(IList<TEntity> items, TIncludes? includes)
        => process?.Invoke(items, includes) ?? Task.CompletedTask;
}
