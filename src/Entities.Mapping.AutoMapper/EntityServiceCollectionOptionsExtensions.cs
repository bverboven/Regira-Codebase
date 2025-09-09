using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;
using System.Reflection;

namespace Regira.Entities.Mapping.AutoMapper;

public static class EntityServiceCollectionOptionsExtensions
{
    public static EntityServiceCollectionOptions UseAutoMapper(this EntityServiceCollectionOptions options, Action<IServiceCollection>? configure = null)
        => UseAutoMapper(options, null, configure);
    public static EntityServiceCollectionOptions UseAutoMapper(this EntityServiceCollectionOptions options, IList<Assembly>? assemblies, Action<IServiceCollection>? configure = null)
    {
        options.Services.AddTransient<IEntityMapper, EntityMapper>();

        configure?.Invoke(options.Services);

        return options;
    }
}