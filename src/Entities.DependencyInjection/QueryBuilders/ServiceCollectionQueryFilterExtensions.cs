using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.QueryBuilders.GlobalFilterBuilders;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Utilities;

namespace Regira.Entities.DependencyInjection.QueryBuilders;

public static class ServiceCollectionQueryFilterExtensions
{
    // QueryFilters
    public static IServiceCollection AddQueryFilter<TEntity, TImplementation>(this IServiceCollection services)
        where TEntity : IEntity<int>
        where TImplementation : class, IFilteredQueryBuilder<TEntity, int, SearchObject<int>>
        => services.AddQueryFilter<TEntity, SearchObject<int>, TImplementation>();
    public static IServiceCollection AddQueryFilter<TEntity, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> factory)
        where TEntity : IEntity<int>
        where TImplementation : class, IFilteredQueryBuilder<TEntity, int, SearchObject<int>>
        => services.AddQueryFilter<TEntity, SearchObject<int>, TImplementation>(factory);

    public static IServiceCollection AddQueryFilter<TEntity, TSearchObject, TImplementation>(this IServiceCollection services)
        where TEntity : IEntity<int>
        where TSearchObject : ISearchObject<int>
        where TImplementation : class, IFilteredQueryBuilder<TEntity, int, TSearchObject>
        => services.AddQueryFilter<TEntity, int, TSearchObject, TImplementation>();
    public static IServiceCollection AddQueryFilter<TEntity, TSearchObject, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> factory)
        where TEntity : IEntity<int>
        where TSearchObject : ISearchObject<int>
        where TImplementation : class, IFilteredQueryBuilder<TEntity, int, TSearchObject>
        => services.AddQueryFilter<TEntity, int, TSearchObject, TImplementation>(factory);

    public static IServiceCollection AddQueryFilter<TEntity, TKey, TSearchObject, TImplementation>(this IServiceCollection services)
        where TEntity : IEntity<TKey>
        where TSearchObject : ISearchObject<TKey>
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, TSearchObject>
        => services.AddTransient<IFilteredQueryBuilder<TEntity, TKey, TSearchObject>, TImplementation>();
    public static IServiceCollection AddQueryFilter<TEntity, TKey, TSearchObject, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> factory)
        where TEntity : IEntity<TKey>
        where TSearchObject : ISearchObject<TKey>
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, TSearchObject>
        => services.AddTransient<IFilteredQueryBuilder<TEntity, TKey, TSearchObject>>(factory);


    // Global QueryFilters
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


    /// <inheritdoc cref="AddDefaultGlobalQueryFilters{TKey}"/>>
    public static EntityServiceCollectionOptions AddDefaultGlobalQueryFilters(this EntityServiceCollectionOptions options)
        => options.AddDefaultGlobalQueryFilters<int>();
    /// <summary>
    /// Adds default filtered query builders
    /// <list type="bullet">
    /// <item>Id(s)</item>
    /// <item>Timestamps</item>
    /// <item>Archivable</item>
    /// </list>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="options"></param>
    /// <returns></returns>
    public static EntityServiceCollectionOptions AddDefaultGlobalQueryFilters<TKey>(this EntityServiceCollectionOptions options)
    {
        options.AddGlobalFilterQueryBuilder<FilterIdsQueryBuilder<TKey>>();
        options.AddGlobalFilterQueryBuilder<FilterArchivablesQueryBuilder>();
        options.AddGlobalFilterQueryBuilder<FilterHasCreatedQueryBuilder>();
        options.AddGlobalFilterQueryBuilder<FilterHasLastModifiedQueryBuilder>();

        return options;
    }
    public static EntityServiceCollectionOptions AddGlobalFilterQueryBuilder<TImplementation>(this EntityServiceCollectionOptions options)
        where TImplementation : class, IGlobalFilteredQueryBuilder
    {
        options.Services.AddTransient<IGlobalFilteredQueryBuilder, TImplementation>();
        return options;
    }
    public static EntityServiceCollectionOptions AddGlobalFilterQueryBuilder<TImplementation, TKey>(this EntityServiceCollectionOptions options)
        where TImplementation : class, IGlobalFilteredQueryBuilder<TKey>
    {
        options.Services
            .AddGlobalFilterQueryBuilder<TImplementation>()
            .AddTransient<IGlobalFilteredQueryBuilder<TKey>, TImplementation>();
        return options;
    }
}