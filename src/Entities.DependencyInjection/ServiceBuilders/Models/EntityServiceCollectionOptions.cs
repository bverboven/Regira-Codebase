using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Mapping.Abstractions;

namespace Regira.Entities.DependencyInjection.ServiceBuilders.Models;

public class EntityServiceCollectionOptions(IServiceCollection services)
{
    public IServiceCollection Services => services;
    public Func<IServiceCollection, IEntityMapConfigurator>? EntityMapConfiguratorFactory { get; set; }

    // See extension methods for implementations
}