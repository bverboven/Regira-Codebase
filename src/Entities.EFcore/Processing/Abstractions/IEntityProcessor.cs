namespace Regira.Entities.EFcore.Processing.Abstractions;

public interface IEntityProcessor<T> : IEntityProcessor<T, T>
    where T : class;
public interface IEntityProcessor<in T, out TTarget>
    where T : class
{
    IAsyncEnumerable<TTarget> ProcessManyAsync(IEnumerable<T> items);
    TTarget Process(T item);
}