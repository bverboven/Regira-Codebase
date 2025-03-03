using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;
using Regira.Entities.EFcore.Preppers.Abstractions;

namespace Regira.Entities.DependencyInjection.Preppers;

public static class ServiceCollectionPrepperExtensions
{
    // Preppers
    public static IServiceCollection AddPrepper<TPrepper>(this IServiceCollection services)
        where TPrepper : class, IEntityPrepper
    {
        services.AddTransient<IEntityPrepper, TPrepper>();
        return services;
    }
    public static IServiceCollection AddPrepper<TEntity, TPrepper>(this IServiceCollection services)
        where TPrepper : class, IEntityPrepper<TEntity>
    {
        AddPrepper<TPrepper>(services);
        services.AddTransient<IEntityPrepper<TEntity>, TPrepper>();
        return services;
    }
    public static IServiceCollection AddPrepper<TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityPrepper
    {
        services.AddTransient<IEntityPrepper>(factory);
        return services;
    }
    public static IServiceCollection AddPrepper<TEntity>(this IServiceCollection services, Func<IServiceProvider, IEntityPrepper<TEntity>> factory)
    {
        services.AddTransient<IEntityPrepper>(factory);
        services.AddTransient(factory);
        return services;
    }

    public static EntityServiceCollectionOptions AddDefaultPreppers(this EntityServiceCollectionOptions options)
    {
        // ToDo: ?
        return options;
    }
    public static EntityServiceCollectionOptions AddPrepper<TImplementation>(this EntityServiceCollectionOptions options)
        where TImplementation : class, IEntityPrepper
    {
        options.Services.AddTransient<IEntityPrepper, TImplementation>();
        return options;
    }
    public static EntityServiceCollectionOptions AddPrepper<TImplementation, TKey>(this EntityServiceCollectionOptions options)
        where TImplementation : class, IEntityPrepper<TKey>
    {
        options.AddPrepper<TImplementation>();
        options.Services.AddTransient<IEntityPrepper<TKey>, TImplementation>();
        return options;
    }
}