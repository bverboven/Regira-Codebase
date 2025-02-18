using Regira.Entities.DependencyInjection.Models;
using Regira.Entities.DependencyInjection.Normalizers;
using Regira.Entities.DependencyInjection.Primers;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Normalizing.Models;

#if NETCOREAPP3_1_OR_GREATER
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
#endif

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
#if NETCOREAPP3_1_OR_GREATER
    public static EntityServiceCollectionOptions ConfigureDefaultJsonOptions(this EntityServiceCollectionOptions options, Action<JsonOptions>? configure = null)
    {
        options.Services
            .Configure<JsonOptions>(o =>
            {
                o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                configure?.Invoke(o);
            });

        return options;
    }
#endif
}