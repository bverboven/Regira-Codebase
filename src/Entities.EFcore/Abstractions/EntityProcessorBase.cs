namespace Regira.Entities.EFcore.Abstractions;

public abstract class EntityProcessorBase<T> : IEntityProcessor<T>
    where T : class
{
    public virtual async IAsyncEnumerable<T> ProcessManyAsync(IEnumerable<T> items)
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