using Regira.Entities.EFcore.Processing.Abstractions;

namespace Regira.Entities.EFcore.Processing;

public class EntityProcessor<TEntity>(Func<IList<TEntity>, Task>? process = null)
    : IEntityProcessor<TEntity>
{
    Task IEntityProcessor.Process<T>(IList<T> items)
        => Process(items.OfType<TEntity>().ToList());

    public virtual Task Process(IList<TEntity> items)
        => process?.Invoke(items) ?? Task.CompletedTask;
}
