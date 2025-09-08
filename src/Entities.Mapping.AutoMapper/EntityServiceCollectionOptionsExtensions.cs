using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;
using System.Reflection;

namespace Regira.Entities.Mapping.AutoMapper;

public static class EntityServiceCollectionOptionsExtensions
{
    public static EntityServiceCollectionOptions UseAutoMapper(this EntityServiceCollectionOptions options, IList<Assembly>? assemblies = null, Action<IMapperConfigurationExpression>? configure = null)
    {
        options.Services.AddAutoMapper(o =>
        {
            o.AllowNullCollections = true;
            configure?.Invoke(o);
        });

        if (assemblies?.Any() == true)
        {
            options.Services.AddAutoMapper(assemblies);
        }

        options.Services.AddTransient<IEntityMapper, EntityMapper>();

        return options;
    }
}