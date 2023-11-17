using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Regira.Entities.Abstractions;
using Regira.Entities.DependencyInjection.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.DependencyInjection;

public class EntityServiceBuilder<TContext, TEntity> : EntityServiceBuilder<TContext, TEntity, int>, IEntityServiceBuilder<TEntity>
    where TContext : DbContext
    where TEntity : class, IEntity<int>
{
    public EntityServiceBuilder(IServiceCollection services)
        : base(services)
    {
    }

    public new EntityServiceBuilder<TContext, TEntity, int> AddService<TService>()
        where TService : class, IEntityService<TEntity>
    {
        base.AddService<TService>();
        Services.AddTransient<IEntityService<TEntity>, TService>();
        return this;
    }
    public new EntityServiceBuilder<TContext, TEntity, int> AddDefaultService()
        => AddService<EntityRepository<TContext, TEntity>>();

    public new EntityServiceBuilder<TContext, TEntity, int> HasRepository<TService>()
        where TService : class, IEntityRepository<TEntity>
    {
        base.HasRepository<TService>();
        Services.AddTransient<IEntityRepository<TEntity>, TService>();
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, int> HasRepository(Func<IServiceProvider, IEntityRepository<TEntity>> factory)
    {
        base.HasRepository(factory);
        Services.AddTransient(factory);
        return this;
    }

    public new EntityServiceBuilder<TContext, TEntity, int> HasManager<TService>()
        where TService : class, IEntityManager<TEntity>
    {
        base.HasManager<TService>();
        Services.AddTransient<IEntityManager<TEntity>, TService>();
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, int> HasManager(Func<IServiceProvider, IEntityManager<TEntity>> factory)
    {
        base.HasManager(factory);
        Services.AddTransient(factory);
        return this;
    }


    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> AddComplexService<TService, TSearchObject, TSortBy, TIncludes>()
        where TService : class, IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>, IEntityService<TEntity>
        where TSearchObject : class, ISearchObject, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
    {
        base.AddComplexService<TService, TSearchObject, TSortBy, TIncludes>();
        // Remove
        Services.RemoveAll<IEntityService<TEntity>>();
        // Add
        Services.AddTransient<IEntityService<TEntity>, TService>();
        Services.AddTransient<IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>, TService>();
        return new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes>(this);
    }
}

public class EntityServiceBuilder<TContext, TEntity, TKey> : EntityServiceCollection<TContext>, IEntityServiceBuilder<TEntity, TKey>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
{
    public EntityServiceBuilder(IServiceCollection services)
        : base(services)
    {
    }


    // Entity mapping
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
    /// <returns></returns>
    public virtual EntityServiceBuilder<TContext, TEntity, TKey> AddMapping<TDto, TInputDto>()
        where TDto : class
        where TInputDto : class
    {
        Services.AddAutoMapper(cfg =>
        {
            cfg.CreateMap<TEntity, TDto>();
            cfg.CreateMap<TInputDto, TEntity>();
        });
        return this;
    }
    public virtual EntityServiceBuilder<TContext, TEntity, TKey> AddMappingProfile<TProfile>() where TProfile : Profile, new()
    {
        Services.AddAutoMapper(cfg => cfg.AddProfile<TProfile>());
        return this;
    }

    // Entity service
    /// <summary>
    /// Adds <typeparamref name="TService"/> implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntity}"/></item>
    ///     <item><see cref="IEntityService{TEntity, TKey}"/></item>
    /// </list>
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public virtual EntityServiceBuilder<TContext, TEntity, TKey> AddService<TService>()
        where TService : class, IEntityService<TEntity, TKey>
    {
        Services.AddTransient<IEntityService<TEntity, TKey>, TService>();
        return this;
    }

    // Default Entity service
    /// <summary>
    /// Adds <see cref="EntityRepository{TContext, TEntity}"/> as implementation for:
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntity}"/></item>
    ///     <item><see cref="IEntityService{TEntity, TKey}"/></item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    public virtual EntityServiceBuilder<TContext, TEntity, TKey> AddDefaultService()
        => AddService<EntityRepository<TContext, TEntity, TKey>>();

    // Entity Repository
    /// <summary>
    /// Adds <typeparamref name="TService"/> implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityRepository{TEntity}"/></item>
    ///     <item><see cref="IEntityRepository{TEntity, TKey}"/></item>
    /// </list>
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public virtual EntityServiceBuilder<TContext, TEntity, TKey> HasRepository<TService>()
        where TService : class, IEntityRepository<TEntity, TKey>
    {
        Services.AddTransient<IEntityRepository<TEntity, TKey>, TService>();
        return this;
    }
    /// <summary>
    /// Adds implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityRepository{TEntity}"/></item>
    ///     <item><see cref="IEntityRepository{TEntity, TKey}"/></item>
    /// </list>
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    /// 
    public virtual EntityServiceBuilder<TContext, TEntity, TKey> HasRepository(Func<IServiceProvider, IEntityRepository<TEntity, TKey>> factory)
    {
        Services.AddTransient(factory);
        return this;
    }

    // Entity Manager
    /// <summary>
    /// Adds <typeparamref name="TService"/> implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityManager{TEntity}"/></item>
    ///     <item><see cref="IEntityManager{TEntity, TKey}"/></item>
    /// </list>
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public virtual EntityServiceBuilder<TContext, TEntity, TKey> HasManager<TService>()
        where TService : class, IEntityManager<TEntity, TKey>
    {
        Services.AddTransient<IEntityManager<TEntity, TKey>, TService>();
        return this;
    }
    /// <summary>
    /// Adds implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityManager{TEntity}"/></item>
    ///     <item><see cref="IEntityManager{TEntity, TKey}"/></item>
    /// </list>
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public virtual EntityServiceBuilder<TContext, TEntity, TKey> HasManager(Func<IServiceProvider, IEntityManager<TEntity, TKey>> factory)
    {
        Services.AddTransient(factory);
        return this;
    }

    // Complex Entity service
    /// <summary>
    /// Adds <typeparamref name="TService"/> implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntity}"/></item>
    ///     <item><see cref="IEntityService{TEntity, TKey}"/></item>
    ///     <item><see cref="IEntityService{TEntity, TSearchObject, TSortBy, TIncludes}"/></item>
    ///     <item><see cref="IEntityService{TEntity, TKey, TSearchObject, TSortBy, TIncludes}"/></item>
    /// </list>
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TSearchObject"></typeparam>
    /// <typeparam name="TSortBy"></typeparam>
    /// <typeparam name="TIncludes"></typeparam>
    /// <returns></returns>
    public virtual ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> AddComplexService<TService, TSearchObject, TSortBy, TIncludes>()
        where TService : class, IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, IEntityService<TEntity, TKey>
        where TSearchObject : class, ISearchObject<TKey>, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
    {
        // Remove
        Services.RemoveAll<IEntityService<TEntity, TKey>>();
        // Add
        Services.AddTransient<IEntityService<TEntity, TKey>, TService>();
        Services.AddTransient<IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, TService>();
        return new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>(this);
    }
}

public class EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> : EntityServiceBuilder<TContext, TEntity, TKey>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
{
    public EntityServiceBuilder(IServiceCollection services)
        : base(services)
    {
    }

    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> AddService<TService>()
        where TService : class, IEntityService<TEntity, TKey, TSearchObject>
    {
        base.AddService<TService>();
        Services.AddTransient<IEntityService<TEntity, TKey, TSearchObject>, TService>();
        return this;
    }
    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> AddDefaultService()
        => AddService<EntityRepository<TContext, TEntity, TKey, TSearchObject>>();

    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> HasRepository<TService>()
        where TService : class, IEntityRepository<TEntity, TKey, TSearchObject>
    {
        base.HasRepository<TService>();
        Services.AddTransient<IEntityRepository<TEntity, TKey, TSearchObject>, TService>();
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> HasRepository(Func<IServiceProvider, IEntityRepository<TEntity, TKey, TSearchObject>> factory)
    {
        base.HasRepository(factory);
        Services.AddTransient(factory);
        return this;
    }

    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> HasManager<TService>()
        where TService : class, IEntityManager<TEntity, TKey, TSearchObject>
    {
        base.HasManager<TService>();
        Services.AddTransient<IEntityManager<TEntity, TKey, TSearchObject>, TService>();
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> HasManager(Func<IServiceProvider, IEntityManager<TEntity, TKey, TSearchObject>> factory)
    {
        base.HasManager(factory);
        Services.AddTransient(factory);
        return this;
    }


    public ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> AddComplexService<TService, TSortBy, TIncludes>()
        where TService : class, IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, IEntityService<TEntity, TKey, TSearchObject>
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
    {
        base.AddComplexService<TService, TSearchObject, TSortBy, TIncludes>();
        // Remove
        Services.RemoveAll<IEntityService<TEntity, TKey, TSearchObject>>();
        // Add
        Services.AddTransient<IEntityService<TEntity, TKey>, TService>();
        Services.AddTransient<IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, TService>();
        return new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>(this);
    }
}