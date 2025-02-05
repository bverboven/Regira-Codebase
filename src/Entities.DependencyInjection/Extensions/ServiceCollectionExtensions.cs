using System.Reflection;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;

namespace Regira.Entities.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public class Options
    {
        public ICollection<Assembly> ProfileAssemblies { get; set; } = new List<Assembly>();
    }

    /// <summary>
    /// <inheritdoc cref="UseEntities"/>
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static EntityServiceCollection<TContext> UseEntities<TContext>(this IServiceCollection services, Action<Options>? configure = null)
        where TContext : DbContext
        => new(services.UseEntities(configure));
    /// <summary>
    /// Configures AutoMapper to <see cref="IProfileExpression.AllowNullCollections">allow null destination collections</see>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection UseEntities(this IServiceCollection services, Action<Options>? configure = null)
    {
        var options = new Options();
        configure?.Invoke(options);

        services
            .AddAutoMapper(cfg =>
            {
                cfg.AllowNullCollections = true;

                if (options.ProfileAssemblies.Any())
                {
                    cfg.AddMaps(options.ProfileAssemblies);
                }
            });

        services.AddTransient<IGlobalFilteredQueryBuilder, DefaultFilteredQueryBuilder>();
        services.AddTransient<IGlobalFilteredQueryBuilder, FilterArchivablesQueryBuilder>();
        services.AddTransient<IGlobalFilteredQueryBuilder, FilterHasCreatedQueryBuilder>();
        services.AddTransient<IGlobalFilteredQueryBuilder, FilterHasLastModifiedQueryBuilder>();

        return services;
    }
}