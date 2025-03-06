﻿using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;
using Regira.Entities.EFcore.Primers;
using Regira.Entities.EFcore.Primers.Abstractions;

namespace Regira.Entities.DependencyInjection.Primers;

public static class ServiceCollectionPrimerExtensions
{
    // Primers
    public static IServiceCollection AddPrimer<TPrimer>(this IServiceCollection services)
        where TPrimer : class, IEntityPrimer
    {
        services.AddTransient<IEntityPrimer, TPrimer>();
        return services;
    }
    public static IServiceCollection AddPrimer<TEntity, TPrimer>(this IServiceCollection services)
        where TPrimer : class, IEntityPrimer<TEntity>
    {
        AddPrimer<TPrimer>(services);
        services.AddTransient<IEntityPrimer<TEntity>, TPrimer>();
        return services;
    }
    public static IServiceCollection AddPrimer(this IServiceCollection services, Func<IServiceProvider, IEntityPrimer> factory)
    {
        services.AddTransient(factory);
        return services;
    }
    public static IServiceCollection AddPrimer<TEntity>(this IServiceCollection services, Func<IServiceProvider, IEntityPrimer<TEntity>> factory)
    {
        services.AddTransient<IEntityPrimer>(factory);
        services.AddTransient(factory);
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


    // EntityServiceCollectionOptions
    public static EntityServiceCollectionOptions AddDefaultPrimers(this EntityServiceCollectionOptions options)
    {
        options.AddPrimer<HasCreatedDbPrimer>();
        options.AddPrimer<HasLastModifiedDbPrimer>();
        options.AddPrimer<ArchivablePrimer>();
        return options;
    }
    public static EntityServiceCollectionOptions AddPrimer<TPrimer>(this EntityServiceCollectionOptions options)
        where TPrimer : class, IEntityPrimer
    {
        options.Services.AddTransient<IEntityPrimer, TPrimer>();
        return options;
    }
    public static EntityServiceCollectionOptions AddPrimer<TPrimer, TKey>(this EntityServiceCollectionOptions options)
        where TPrimer : class, IEntityPrimer<TKey>
    {
        options.AddPrimer<TPrimer>();
        options.Services.AddTransient<IEntityPrimer<TKey>, TPrimer>();
        return options;
    }
}