using Microsoft.Extensions.DependencyInjection;

namespace Regira.Entities.DependencyInjection.ServiceBuilders.Models;

public class EntityServiceCollectionOptions(IServiceCollection services)
{
    public IServiceCollection Services => services;

    // See extension methods for implementations
}