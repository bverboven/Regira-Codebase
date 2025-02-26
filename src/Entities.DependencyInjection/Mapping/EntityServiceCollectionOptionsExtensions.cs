using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.Models;

namespace Regira.Entities.DependencyInjection.Mapping;

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

        return options;
    }
}