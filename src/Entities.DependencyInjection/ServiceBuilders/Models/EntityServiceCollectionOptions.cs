using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Mapping.Abstractions;

namespace Regira.Entities.DependencyInjection.ServiceBuilders.Models;

public class EntityServiceCollectionOptions(IServiceCollection services)
{
    public IServiceCollection Services => services;
    public IEntityMapConfigurator EntityMapConfigurator { get; set; } = null!;

    // See extension methods for implementations
}