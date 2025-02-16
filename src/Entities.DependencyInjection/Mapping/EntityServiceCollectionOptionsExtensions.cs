using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.Models;

namespace Regira.Entities.DependencyInjection.Mapping;

public static class EntityServiceCollectionOptionsExtensions
{
    public static EntityServiceCollectionOptions UseAutoMapper(this EntityServiceCollectionOptions options, params Assembly[] assemblies)
        => options.UseAutoMapper(o => o.AllowNullCollections = true, assemblies);

    public static EntityServiceCollectionOptions UseAutoMapper(this EntityServiceCollectionOptions options, Action<IMapperConfigurationExpression> configure, params Assembly[] assemblies)
    {
        options.Services.AddAutoMapper(configure);

        if (assemblies.Any())
        {
            options.Services.AddAutoMapper(assemblies);
        }

        return options;
    }
}