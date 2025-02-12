using System.Reflection;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.EFcore.Abstractions;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Entities.EFcore.Primers;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders;
using Regira.Entities.Keywords;
using Regira.Entities.Keywords.Abstractions;
using Regira.Normalizing;
using Regira.Normalizing.Abstractions;
using Regira.Normalizing.Models;
using Regira.Utilities;

namespace Regira.Entities.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public class Options(IServiceCollection services)
    {
        public ICollection<Assembly> ProfileAssemblies { get; set; } = new List<Assembly>();
        /// <summary>
        /// Adds default filtered query builders
        /// <list type="bullet">
        /// <item>Id(s)</item>
        /// <item>Timestamps</item>
        /// <item>Archivable</item>
        /// </list>
        /// </summary>
        public IServiceCollection AddDefaultGlobalQueryFilters()
        {
            services.AddTransient<IGlobalFilteredQueryBuilder, FilterIdsQueryBuilder>();
            services.AddTransient<IGlobalFilteredQueryBuilder, FilterArchivablesQueryBuilder>();
            services.AddTransient<IGlobalFilteredQueryBuilder, FilterHasCreatedQueryBuilder>();
            services.AddTransient<IGlobalFilteredQueryBuilder, FilterHasLastModifiedQueryBuilder>();

            return services;
        }

        public IServiceCollection AddDefaultQKeywordHelper(Func<IServiceProvider, INormalizer>? normalizerFactory = null, QKeywordHelperOptions? options = null)
            => services.AddTransient<IQKeywordHelper>(p => new QKeywordHelper(normalizerFactory?.Invoke(p), options));
        public IServiceCollection AddDefaultEntityNormalizer(Action<IServiceProvider, NormalizingOptions>? configure = null)
        {
            if (configure == null)
            {
                services.AddTransient<INormalizer, DefaultNormalizer>();
                return services.AddTransient<IObjectNormalizer, ObjectNormalizer>();
            }

            services.AddTransient<INormalizer>(p =>
            {
                var options = new NormalizingOptions();
                configure.Invoke(p, options);
                return options.DefaultNormalizer ?? new DefaultNormalizer(options);
            });
            services.AddTransient<IObjectNormalizer>(p =>
            {
                var options = new NormalizingOptions();
                configure.Invoke(p, options);
                return options.DefaultObjectNormalizer ?? new ObjectNormalizer(options);
            });

            return services.AddTransient<IEntityNormalizer, DefaultEntityNormalizer>();
        }

        public IServiceCollection AddGlobalFilterQueryBuilder<TImplementation>()
            where TImplementation : class, IGlobalFilteredQueryBuilder
            => services
                .AddTransient<IGlobalFilteredQueryBuilder, TImplementation>();
        public IServiceCollection AddGlobalFilterQueryBuilder<TImplementation, TKey>()
            where TImplementation : class, IGlobalFilteredQueryBuilder<TKey>
            => services
                .AddGlobalFilterQueryBuilder<TImplementation>()
                .AddTransient<IGlobalFilteredQueryBuilder<TKey>, TImplementation>();
    }

    /// <summary>
    /// <inheritdoc cref="UseEntities"/>
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static EntityServiceCollection<TContext> UseEntities<TContext>(this IServiceCollection services, Action<Options>? configure = null)
        where TContext : DbContext
        => new(services.UseEntities(configure));
    /// <summary>
    /// Configures AutoMapper to <see cref="IProfileExpression.AllowNullCollections">allow null destination collections</see>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection UseEntities(this IServiceCollection services, Action<Options>? configure = null)
    {
        var options = new Options(services);
        configure?.Invoke(options);

        services
            .AddAutoMapper(cfg =>
            {
                cfg.AllowNullCollections = true;

                if (options.ProfileAssemblies.Any())
                {
                    cfg.AddMaps(options.ProfileAssemblies);
                }
            });

        return services;
    }

    public static IServiceCollection AddGlobalFilterQueryBuilder<TImplementation>(this IServiceCollection services)
        where TImplementation : class, IGlobalFilteredQueryBuilder
        => services
            .AddTransient<IGlobalFilteredQueryBuilder, TImplementation>();
    public static IServiceCollection AddGlobalFilterQueryBuilder<TImplementation, TKey>(this IServiceCollection services)
        where TImplementation : class, IGlobalFilteredQueryBuilder<TKey>
        => services
            .AddGlobalFilterQueryBuilder<TImplementation>()
            .AddTransient<IGlobalFilteredQueryBuilder<TKey>, TImplementation>();
    public static TServiceCollection RemoveGlobalQueryFilters<TServiceCollection>(this TServiceCollection services)
        where TServiceCollection : IServiceCollection
    {
        var globalFilters = services
            .Where(d =>
                d.ImplementationType != null
                && TypeUtility.ImplementsInterface<IGlobalFilteredQueryBuilder>(d.ImplementationType)
            );

        foreach (var descriptor in globalFilters)
        {
            services.Remove(descriptor);
        }

        return services;
    }


    public static IServiceCollection AddAutoTruncatePrimer<TServiceCollection>(this TServiceCollection services)
        where TServiceCollection : IServiceCollection
        => services.AddTransient<IEntityPrimer, AutoTruncatePrimer>();
    public static IServiceCollection AddDefaultEntityNormalizerPrimer<TServiceCollection>(this TServiceCollection services)
        where TServiceCollection : IServiceCollection
        => services.AddTransient<IEntityPrimer, AutoNormalizingPrimer>();
}