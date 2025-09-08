using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;
using Regira.Entities.EFcore.Preppers;
using Regira.Entities.EFcore.Preppers.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.DependencyInjection.Preppers;

public static class ServiceCollectionPrepperExtensions
{
    // Preppers
    public static IServiceCollection AddPrepper<TPrepper>(this IServiceCollection services)
        where TPrepper : class, IEntityPrepper
        => services.AddTransient<IEntityPrepper, TPrepper>();

    public static IServiceCollection AddPrepper<TEntity, TPrepper>(this IServiceCollection services)
        where TPrepper : class, IEntityPrepper<TEntity>
    {
        AddPrepper<TPrepper>(services);
        services.AddTransient<IEntityPrepper<TEntity>, TPrepper>();
        return services;
    }
    public static IServiceCollection AddPrepper<TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityPrepper
        => services.AddTransient<IEntityPrepper>(factory);

    public static IServiceCollection AddPrepper<TEntity>(this IServiceCollection services, Func<IServiceProvider, IEntityPrepper<TEntity>> factory)
    {
        services.AddTransient<IEntityPrepper>(factory);
        services.AddTransient(factory);
        return services;
    }

    public static IServiceCollection AddPrepper<TEntity>(this IServiceCollection services, Action<TEntity> prepareFunc)
        where TEntity : class
        => services.AddTransient<IEntityPrepper>(_ => new EntityPrepper<TEntity>(item =>
        {
            prepareFunc(item);
            return Task.CompletedTask;
        }));

    public static IServiceCollection AddPrepper<TContext, TEntity, TKey>(this IServiceCollection services, Func<TEntity, TContext, Task> prepareFunc)
        where TContext : DbContext
        where TEntity : class, IEntity<TKey>
        => services.AddTransient<IEntityPrepper>(p => new EntityPrepper<TContext, TEntity, TKey>(p.GetRequiredService<TContext>(), prepareFunc));


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
    public static EntityServiceCollectionOptions AddPrepper<TContext, TEntity, TKey>(this EntityServiceCollectionOptions options, Func<TEntity, TContext, Task> prepareFunc)
        where TContext : DbContext
        where TEntity : class, IEntity<TKey>
    {
        options.Services.AddPrepper<TContext, TEntity, TKey>(prepareFunc);
        return options;
    }
    public static EntityServiceCollectionOptions AddPrepper<TEntity>(this EntityServiceCollectionOptions options, Action<TEntity> prepareFunc)
        where TEntity : class
    {
        options.Services.AddPrepper(prepareFunc);
        return options;
    }
}