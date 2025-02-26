using Regira.Normalizing;
using Regira.Normalizing.Abstractions;
using Regira.Normalizing.Models;

namespace Regira.Entities.EFcore.Normalizing.Abstractions;

public class EntityNormalizingOptions : NormalizingOptions
{
    public bool IsExclusive { get; set; }
}
public abstract class EntityNormalizerBase<T>(INormalizer? normalizer = null) : IEntityNormalizer<T>
    where T : class
{
    public virtual bool IsExclusive => false;

    protected internal virtual INormalizer DefaultPropertyNormalizer => normalizer ?? NormalizingDefaults.DefaultPropertyNormalizer ?? new DefaultNormalizer();

    public virtual Task HandleNormalize(T item)
        => Task.CompletedTask;

    public virtual async Task HandleNormalizeMany(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            await HandleNormalize(item);
        }
    }

    Task IEntityNormalizer.HandleNormalize(object item)
        => HandleNormalize((T)item);
    Task IEntityNormalizer.HandleNormalizeMany(IEnumerable<object> items)
        => HandleNormalizeMany(items.Cast<T>());
}