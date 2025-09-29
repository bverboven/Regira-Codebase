using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;

namespace Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;

public static class ServiceCollectionExtensions
{
    public static EntityServiceCollection<TContext> UseEntities<TContext>(this IServiceCollection services, Action<EntityServiceCollectionOptions>? configure = null)
        where TContext : DbContext
    {
        var options = new EntityServiceCollectionOptions(services);
        configure?.Invoke(options);
        return new EntityServiceCollection<TContext>(options);
    }
}