using Regira.Entities.DependencyInjection.Models;
using Regira.Entities.DependencyInjection.Normalizers;
using Regira.Entities.DependencyInjection.Primers;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Normalizing.Models;

namespace Regira.Entities.DependencyInjection.Extensions;

public static class EntityServiceCollectionOptionsExtensions
{
    public class EntityDefaultOptions
    {
        protected internal Action<NormalizeOptions>? ConfigureNormalizingFunc { get; set; }
        public void ConfigureNormalizing(Action<NormalizeOptions> configure)
            => ConfigureNormalizingFunc = configure;

    }
    public static EntityServiceCollectionOptions UseDefaults(this EntityServiceCollectionOptions options, Action<EntityDefaultOptions>? configure = null)
    {
        var entityDefaultOptions = new EntityDefaultOptions();
        configure?.Invoke(entityDefaultOptions);

        options.AddDefaultPrimers();
        if (entityDefaultOptions.ConfigureNormalizingFunc != null)
        {
            options.AddDefaultEntityNormalizer(entityDefaultOptions.ConfigureNormalizingFunc);
        }
        else
        {
            options.AddDefaultEntityNormalizer();
        }
        options.AddDefaultGlobalQueryFilters();
        return options;
    }
}