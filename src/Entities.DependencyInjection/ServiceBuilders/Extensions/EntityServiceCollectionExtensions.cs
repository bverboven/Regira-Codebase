using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.Normalizers;
using Regira.Entities.DependencyInjection.Primers;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;
using Regira.Entities.Mapping.Abstractions;

namespace Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;

public static class EntityServiceCollectionExtensions
{
    /// <summary>
    /// <inheritdoc cref="ServiceCollectionPrimerExtensions.AddDefaultPrimers"/>
    /// <inheritdoc cref="ServiceCollectionNormalizerExtensions.AddDefaultEntityNormalizer(EntityServiceCollectionOptions,Action{Normalizing.Models.NormalizeOptions})"/>
    /// <inheritdoc cref="ServiceCollectionQueryFilterExtensions.AddDefaultGlobalQueryFilters"/>
    /// </summary>
    /// <param name="options"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static EntityServiceCollectionOptions UseDefaults(this EntityServiceCollectionOptions options, Action<EntityDefaultNormalizingOptions>? configure = null)
    {
        var entityDefaultOptions = new EntityDefaultNormalizingOptions();
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

    public static EntityServiceCollectionOptions UseMapper<TMapperImplementation>(this EntityServiceCollectionOptions options)
        where TMapperImplementation : class, IEntityMapper
    {
        options.Services.AddTransient<IEntityMapper, TMapperImplementation>();
        return options;
    }
    public static EntityServiceCollectionOptions UseMapper<TMapperImplementation>(this EntityServiceCollectionOptions options, Func<IServiceProvider, IEntityMapper> factory)
        where TMapperImplementation : class, IEntityMapper
    {
        options.Services.AddTransient(factory);
        return options;
    }
}