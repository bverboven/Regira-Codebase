using Regira.Entities.EFcore.Processing.Abstractions;

namespace Regira.Entities.EFcore.Processing;

public abstract class EntityProcessorBase<T> : IEntityProcessor<T>
    where T : class
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public virtual async IAsyncEnumerable<T> ProcessManyAsync(IEnumerable<T> items)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        foreach (var item in items)
        {
            yield return Process(item);
        }
    }

    public virtual T Process(T item)
    {
        return item;
    }
}