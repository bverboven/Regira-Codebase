using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.DependencyInjection.QueryBuilders;

public static class ServiceCollectionQueryBuilderExtensions
{
    // Query Builders
    public static IServiceCollection AddDefaultQueryBuilder<TEntity>(this IServiceCollection services)
        where TEntity : IEntity<int>
    {
        services.UseQueryBuilder<TEntity, int, QueryBuilder<TEntity>>();
        return services;
    }
    public static IServiceCollection UseQueryBuilder<TEntity, TImplementation>(this IServiceCollection services)
        where TEntity : IEntity<int>
        where TImplementation : class, IQueryBuilder<TEntity>
    {
        services.AddTransient<IQueryBuilder<TEntity>, TImplementation>();
        return services;
    }
    public static IServiceCollection UseQueryBuilder<TEntity>(this IServiceCollection services, Func<IServiceProvider, IQueryBuilder<TEntity>> factory)
        where TEntity : IEntity<int>
    {
        services.AddTransient(factory);
        return services;
    }

    public static IServiceCollection AddDefaultQueryBuilder<TEntity, TKey>(this IServiceCollection services)
        where TEntity : IEntity<TKey>
    {
        services.UseQueryBuilder<TEntity, TKey, QueryBuilder<TEntity, TKey>>();
        return services;
    }
    public static IServiceCollection UseQueryBuilder<TEntity, TKey, TImplementation>(this IServiceCollection services)
        where TEntity : IEntity<TKey>
        where TImplementation : class, IQueryBuilder<TEntity, TKey>
    {
        services.AddTransient<IQueryBuilder<TEntity, TKey>, TImplementation>();
        return services;
    }
    public static IServiceCollection UseQueryBuilder<TEntity, TKey>(this IServiceCollection services, Func<IServiceProvider, IQueryBuilder<TEntity, TKey>> factory)
        where TEntity : IEntity<TKey>
    {
        services.AddTransient(factory);
        return services;
    }

    public static IServiceCollection AddDefaultQueryBuilder<TEntity, TKey, TSearchObject>(this IServiceCollection services)
        where TEntity : IEntity<TKey>
        where TSearchObject : ISearchObject<TKey>
    {
        services.UseQueryBuilder<TEntity, TKey, TSearchObject, QueryBuilder<TEntity, TKey, TSearchObject>>();
        return services;
    }
    public static IServiceCollection UseQueryBuilder<TEntity, TKey, TSearchObject, TImplementation>(this IServiceCollection services)
        where TEntity : IEntity<TKey>
        where TSearchObject : ISearchObject<TKey>
        where TImplementation : class, IQueryBuilder<TEntity, TKey, TSearchObject>
    {
        services.AddTransient<IQueryBuilder<TEntity, TKey, TSearchObject>, TImplementation>();
        return services;
    }
    public static IServiceCollection UseQueryBuilder<TEntity, TKey, TSearchObject>(this IServiceCollection services, Func<IServiceProvider, IQueryBuilder<TEntity, TKey, TSearchObject>> factory)
        where TEntity : IEntity<TKey>
        where TSearchObject : ISearchObject<TKey>
    {
        services.AddTransient(factory);
        return services;
    }

    public static IServiceCollection AddDefaultQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>(this IServiceCollection services)
        where TEntity : IEntity<int>
        where TSearchObject : ISearchObject<int>, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
    {
        services.UseQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes, QueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>>();
        return services;
    }
    public static IServiceCollection UseQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes, TImplementation>(this IServiceCollection services)
        where TEntity : IEntity<int>
        where TSearchObject : ISearchObject<int>, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
        where TImplementation : class, IQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>
    {
        services.UseQueryBuilder<TEntity, int, TSearchObject, TSortBy, TIncludes, TImplementation>();
        services.AddTransient<IQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>, TImplementation>();
        return services;
    }
    public static IServiceCollection UseQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>(this IServiceCollection services, Func<IServiceProvider, IQueryBuilder<TEntity, int, TSearchObject, TSortBy, TIncludes>> factory)
        where TEntity : IEntity<int>
        where TSearchObject : ISearchObject<int>, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
    {
        services.UseQueryBuilder<TEntity, int, TSearchObject, TSortBy, TIncludes>(factory);
        services.AddTransient(factory);
        return services;
    }

    public static IServiceCollection AddDefaultQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>(this IServiceCollection services)
        where TEntity : IEntity<TKey>
        where TSearchObject : ISearchObject<TKey>, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
    {
        services.UseQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes, QueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>>();
        return services;
    }
    public static IServiceCollection UseQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TImplementation>(this IServiceCollection services)
        where TEntity : IEntity<TKey>
        where TSearchObject : ISearchObject<TKey>, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
        where TImplementation : class, IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    {
        services.AddTransient<IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, TImplementation>();
        return services;
    }
    public static IServiceCollection UseQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>(this IServiceCollection services, Func<IServiceProvider, IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>> factory)
        where TEntity : IEntity<TKey>
        where TSearchObject : ISearchObject<TKey>, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
    {
        services.AddTransient(factory);
        return services;
    }
}