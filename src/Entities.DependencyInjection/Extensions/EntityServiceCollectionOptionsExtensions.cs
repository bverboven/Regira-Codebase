using Regira.Entities.DependencyInjection.Models;
using Regira.Entities.DependencyInjection.Normalizers;
using Regira.Entities.DependencyInjection.Primers;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
using Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders;
using Regira.Entities.Keywords;
using Regira.Normalizing;
using Regira.Normalizing.Models;

#if NETCOREAPP3_1_OR_GREATER
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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
    /// <summary>
    /// Adds default services
    /// Primers
    /// <list type="bullet">
    ///     <item><see cref="HasCreatedDbPrimer"/></item>
    ///     <item><see cref="HasLastModifiedDbPrimer"/></item>
    ///     <item><see cref="ArchivablePrimer"/></item>
    /// </list>
    /// Normalizing
    /// <list type="bullet">
    ///     <item><see cref="DefaultNormalizer"/></item>
    ///     <item><see cref="QKeywordHelper"/></item>
    ///     <item><see cref="ObjectNormalizer"/></item>
    ///     <item><see cref="DefaultEntityNormalizer"/></item>
    /// </list>
    /// QueryFilters
    /// <list type="bullet">
    ///     <item><see cref="FilterIdsQueryBuilder"/></item>
    ///     <item><see cref="FilterArchivablesQueryBuilder"/></item>
    ///     <item><see cref="FilterHasCreatedQueryBuilder"/></item>
    ///     <item><see cref="FilterHasLastModifiedQueryBuilder"/></item>
    /// </list>
    /// </summary>
    /// <param name="options"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
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
    /// <summary>
    /// <list type="bullet">
    /// <item>Ignore nulls</item>
    /// <item>Ignore reference cycles</item>
    /// <item>Enums as string</item>
    /// </list>
    /// </summary>
    /// <param name="options"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static EntityServiceCollectionOptions ConfigureDefaultJsonOptions(this EntityServiceCollectionOptions options, Action<JsonOptions>? configure = null)
    {
        options.Services
            .Configure<JsonOptions>(o =>
            {
                o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                configure?.Invoke(o);
            });

        return options;
    }
#endif
}