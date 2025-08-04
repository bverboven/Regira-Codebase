using Regira.Entities.DependencyInjection.Normalizers;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.Keywords;
using Regira.Normalizing;
using static Regira.Entities.DependencyInjection.ServiceBuilders.Extensions.EntityServiceCollectionNormalizingExtensions;

namespace Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;

public static class EntityServiceCollectionNormalizingExtensions
{
    /// <summary>
    /// Adds default Normalizing services
    /// <list type="bullet">
    ///     <item><see cref="DefaultNormalizer"/></item>
    ///     <item><see cref="QKeywordHelper"/></item>
    ///     <item><see cref="ObjectNormalizer"/></item>
    ///     <item><see cref="DefaultEntityNormalizer"/></item>
    /// </list>
    /// </summary>
    /// <param name="options"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static EntityServiceCollectionOptions UseNormalizerDefaults(this EntityServiceCollectionOptions options, Action<EntityDefaultNormalizingOptions>? configure = null)
    {
        var entityDefaultOptions = new EntityDefaultNormalizingOptions();
        configure?.Invoke(entityDefaultOptions);

        if (entityDefaultOptions.ConfigureNormalizingFunc != null)
        {
            options.AddDefaultEntityNormalizer(entityDefaultOptions.ConfigureNormalizingFunc);
        }
        else
        {
            options.AddDefaultEntityNormalizer();
        }
        return options;
    }
}