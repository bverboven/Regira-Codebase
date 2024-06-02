using Regira.Normalizing;

namespace DAL.EFcore.Testing.Infrastructure;

public abstract class EntityNormalizerBase<T> : ObjectNormalizer
{
    public abstract Task HandleNormalize(IEnumerable<T> items);
    public override Task HandleNormalizeMany(IEnumerable<object?> items, bool recursive = true)
        => HandleNormalize(items.Cast<T>());
}
