using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Mapping.AutoMapper;

public static class EntityServiceBuilderExtensions
{
    /// <summary>
    /// Adds AutoMapper maps for
    /// <list type="bullet">
    ///     <item><typeparamref name="TEntity"/> -&gt; <see cref="TDto"/></item>
    ///     <item><see cref="TDto"/> -&gt; <typeparamref name="TEntity"/></item>
    ///     <item><see cref="TInputDto"/> -&gt; <typeparamref name="TEntity"/></item>
    /// </list>
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TInputDto"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <returns></returns>
    public static IEntityServiceBuilder<TContext, TEntity, TKey> AddMapping<TContext, TEntity, TKey, TDto, TInputDto>(this IEntityServiceBuilder<TContext, TEntity, TKey> builder)
        where TContext : DbContext
        where TEntity : class, IEntity<TKey>
        where TDto : class
        where TInputDto : class
    {
        builder.Services.AddAutoMapper(cfg =>
        {
            cfg.CreateMap<TEntity, TDto>();
            cfg.CreateMap<TInputDto, TEntity>();
        });
        return builder;
    }

    public static IServiceCollection AddMapping<TEntity, TDto, TInputDto>(this IServiceCollection services)
        where TEntity : class, IEntity<int>
        where TDto : class
        where TInputDto : class
        => AddMapping<TEntity, int, TDto, TInputDto>(services);
    public static IServiceCollection AddMapping<TEntity, TKey, TDto, TInputDto>(this IServiceCollection services)
        where TEntity : class, IEntity<TKey>
        where TDto : class
        where TInputDto : class
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.CreateMap<TEntity, TDto>();
            cfg.CreateMap<TInputDto, TEntity>();
        });
        return services;
    }

    public static IEntityServiceBuilder<TContext, TEntity, TKey> AddMappingProfile<TContext, TEntity, TKey, TProfile>(this IEntityServiceBuilder<TContext, TEntity, TKey> builder)
        where TContext : DbContext
        where TEntity : class, IEntity<TKey>
        where TProfile : Profile, new()
    {
        builder.Services.AddAutoMapper(cfg => cfg.AddProfile<TProfile>());
        return builder;
    }
    public static IServiceCollection AddMapping<TProfile>(this IServiceCollection services)
        where TProfile : Profile, new()
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<TProfile>());
        return services;
    }
}