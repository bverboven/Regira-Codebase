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

        public Options UseDefaults()
        {
            AddDefaultQKeywordHelper();
            AddDefaultPrimers();
            AddDefaultEntityNormalizer();
            AddDefaultGlobalQueryFilters();

            return this;
        }


        public Options AddDefaultQKeywordHelper(Func<IServiceProvider, INormalizer>? normalizerFactory = null, QKeywordHelperOptions? options = null)
        {
            services.AddTransient<IQKeywordHelper>(p => new QKeywordHelper(normalizerFactory?.Invoke(p), options));
            return this;
        }

        public Options AddDefaultPrimers()
        {
            AddPrimer<HasCreatedDbPrimer>();
            AddPrimer<HasLastModifiedDbPrimer>();
            return this;
        }
        public Options AddPrimer<TPrimer>()
            where TPrimer : class, IEntityPrimer
        {
            services.AddTransient<IEntityPrimer, TPrimer>();
            return this;
        }
        public Options AddPrimer<TPrimer, TKey>()
            where TPrimer : class, IEntityPrimer<TKey>
        {
            services.AddTransient<IEntityPrimer<TKey>, TPrimer>();
            return this;
        }
        public Options AddDefaultEntityNormalizer(Action<IServiceProvider, NormalizingOptions>? configure = null)
        {
            if (configure == null)
            {
                services.AddTransient<INormalizer, DefaultNormalizer>();
                services.AddTransient<IObjectNormalizer, ObjectNormalizer>();
                services.AddTransient<IEntityNormalizer, DefaultEntityNormalizer>();
                return this;
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

            services.AddTransient<IEntityNormalizer, DefaultEntityNormalizer>();
            return this;
        }
        public Options AddNormalizer<TNormalizer>()
            where TNormalizer : class, IEntityNormalizer
        {
            services.AddTransient<IEntityNormalizer, TNormalizer>();
            return this;
        }
        public Options AddNormalizer<TNormalizer, TEntity>()
            where TNormalizer : class, IEntityNormalizer<TEntity>
        {
            services.AddTransient<IEntityNormalizer<TEntity>, TNormalizer>();
            return this;
        }


        /// <summary>
        /// Adds default filtered query builders
        /// <list type="bullet">
        /// <item>Id(s)</item>
        /// <item>Timestamps</item>
        /// <item>Archivable</item>
        /// </list>
        /// </summary>
        public Options AddDefaultGlobalQueryFilters()
        {
            services.AddTransient<IGlobalFilteredQueryBuilder, FilterIdsQueryBuilder>();
            services.AddTransient<IGlobalFilteredQueryBuilder, FilterArchivablesQueryBuilder>();
            services.AddTransient<IGlobalFilteredQueryBuilder, FilterHasCreatedQueryBuilder>();
            services.AddTransient<IGlobalFilteredQueryBuilder, FilterHasLastModifiedQueryBuilder>();

            return this;
        }
        public Options AddGlobalFilterQueryBuilder<TImplementation>()
            where TImplementation : class, IGlobalFilteredQueryBuilder
        {
            services.AddTransient<IGlobalFilteredQueryBuilder, TImplementation>();
            return this;
        }
        public Options AddGlobalFilterQueryBuilder<TImplementation, TKey>()
            where TImplementation : class, IGlobalFilteredQueryBuilder<TKey>
        {
            services
                .AddGlobalFilterQueryBuilder<TImplementation>()
                .AddTransient<IGlobalFilteredQueryBuilder<TKey>, TImplementation>();
            return this;
        }
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
    static IServiceCollection UseEntities(this IServiceCollection services, Action<Options>? configure = null)
    {
        var options = new Options(services);
        configure?.Invoke(options);

        return services;
    }

    // QueryFilters
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

    // Primers
    public static IServiceCollection AddPrimer<TPrimer>(this IServiceCollection services)
        where TPrimer : class, IEntityPrimer
    {
        services.AddTransient<IEntityPrimer, TPrimer>();
        return services;
    }
    // Normalizer
    public static IServiceCollection AddNormalizer<TNormalizer>(this IServiceCollection services)
        where TNormalizer : class, IEntityNormalizer
    {
        services.AddTransient<IEntityNormalizer, TNormalizer>();
        return services;
    }

    // AutoTruncate
    public static IServiceCollection AddAutoTruncatePrimer<TServiceCollection>(this TServiceCollection services)
        where TServiceCollection : IServiceCollection
        => services.AddTransient<IEntityPrimer, AutoTruncatePrimer>();
    // Default normalizer
    public static IServiceCollection AddDefaultEntityNormalizerPrimer<TServiceCollection>(this TServiceCollection services)
        where TServiceCollection : IServiceCollection
        => services.AddTransient<IEntityPrimer, AutoNormalizingPrimer>();
}