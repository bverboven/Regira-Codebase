using Regira.Normalizing;
using Regira.Normalizing.Abstractions;
using Testing.Library.Contoso;

namespace Normalizing.Testing.Services;

public class DummyPersonNormalizer : IObjectNormalizer
{
    public INormalizer DefaultNormalizer { get; }

    public DummyPersonNormalizer(INormalizer? defaultNormalizer = null)
    {
        DefaultNormalizer = defaultNormalizer ?? NormalizingDefaults.DefaultPropertyNormalizer ?? new DefaultNormalizer();
    }

    public void HandleNormalize(Person item)
    {
        item.NormalizedGivenName = item.GivenName;
        item.NormalizedLastName = item.LastName;
    }
    public void HandleNormalize(object? instance, bool recursive = true)
    {
        if (instance is Person item)
        {
            HandleNormalize(item);
        }
    }
}