namespace Regira.Entities.EFcore.Normalizing.Abstractions;

public interface IEntityNormalizer
{
    bool IsExclusive { get; }
    Task HandleNormalize(object item);
    Task HandleNormalizeMany(IEnumerable<object> items);
}
public interface IEntityNormalizer<in T> : IEntityNormalizer
{
    Task HandleNormalize(T item);
    Task HandleNormalizeMany(IEnumerable<T> items);
}