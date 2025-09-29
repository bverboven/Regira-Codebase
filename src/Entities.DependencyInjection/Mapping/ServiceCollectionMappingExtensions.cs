﻿using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;
using Regira.Entities.Mapping.Abstractions;

namespace Regira.Entities.DependencyInjection.Mapping;

public static class ServiceCollectionMappingExtensions
{
    public class MappingConfigurator(EntityServiceCollectionOptions options)
    {
        public void SetMapConfigurator(Func<IServiceCollection, IEntityMapConfigurator> factory)
        {
            options.EntityMapConfiguratorFactory = factory;
        }
        public void SetMapper(Func<IServiceProvider, IEntityMapper> factory)
        {
            options.Services.AddTransient(factory);
        }
        public void SetMapper<TEntityMapper>()
            where TEntityMapper : class, IEntityMapper
        {
            options.Services.AddTransient<IEntityMapper, TEntityMapper>();
        }
    }

    public static EntityServiceCollectionOptions AddMapping(this EntityServiceCollectionOptions options, Action<MappingConfigurator> configure)
    {
        configure.Invoke(new MappingConfigurator(options));

        return options;
    }
    public static EntityServiceCollectionOptions AddMapping<TEntityMapper>(this EntityServiceCollectionOptions options,
        Func<IServiceCollection, IEntityMapConfigurator> configFactory)
        where TEntityMapper : class, IEntityMapper
        => AddMapping(options, c =>
        {
            c.SetMapConfigurator(configFactory);
            c.SetMapper<TEntityMapper>();
        });
}