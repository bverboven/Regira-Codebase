using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.Mapping;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;

namespace Regira.Entities.Mapping.Mapster;

public static class ServiceCollectionExtensions
{
    public static EntityServiceCollectionOptions UseMapsterMapping(this EntityServiceCollectionOptions options, Action<TypeAdapterConfig>? configure = null)
    {
        var config = new TypeAdapterConfig();
        // important to prevent stackoverflow!!
        config.Default.PreserveReference(true);

        options.Services
            .AddSingleton(_ =>
            {
                configure?.Invoke(config);

                return config;
            })
            .AddMapster();

        options.AddMapping<EntityMapper>(services => new EntityMapConfigurator(services, config));

        return options;
    }
}