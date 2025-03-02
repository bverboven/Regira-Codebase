using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.ServiceBuilders.Models;

namespace Regira.Entities.DependencyInjection.ServiceBuilders.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// <inheritdoc cref="UseEntities"/>
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static EntityServiceCollection<TContext> UseEntities<TContext>(this IServiceCollection services, Action<EntityServiceCollectionOptions>? configure = null)
        where TContext : DbContext
        => new(services.UseEntities(configure));
    /// <summary>
    /// Configures AutoMapper to <see cref="IProfileExpression.AllowNullCollections">allow null destination collections</see>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    static IServiceCollection UseEntities(this IServiceCollection services, Action<EntityServiceCollectionOptions>? configure = null)
    {
        var options = new EntityServiceCollectionOptions(services);
        configure?.Invoke(options);

        return services;
    }
}