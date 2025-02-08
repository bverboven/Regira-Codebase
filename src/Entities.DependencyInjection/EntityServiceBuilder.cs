using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Regira.Entities.Abstractions;
using Regira.Entities.DependencyInjection.Abstractions;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.DependencyInjection;

public class EntityServiceBuilder<TContext, TEntity>(IServiceCollection services)
    : EntityServiceBuilder<TContext, TEntity, int>(services), IEntityServiceBuilder<TEntity>
    where TContext : DbContext
    where TEntity : class, IEntity<int>
{
    // Entity service
    public new EntityServiceBuilder<TContext, TEntity> AddService<TService>()
        where TService : class, IEntityService<TEntity>
    {
        base.AddService<TService>();
        Services.AddTransient<IEntityService<TEntity>, TService>();
        return this;
    }
    /// <summary>
    /// Adds an implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntity}"/></item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    public virtual EntityServiceBuilder<TContext, TEntity> AddService(Func<IServiceProvider, IEntityService<TEntity>> factory)
    {
        base.AddService(factory);
        Services.AddTransient(factory);
        return this;
    }

    // Default Entity service
    public new EntityServiceBuilder<TContext, TEntity> AddDefaultService()
        => AddService<EntityRepository<TContext, TEntity>>();

    public new EntityServiceBuilder<TContext, TEntity> HasRepository<TService>()
        where TService : class, IEntityRepository<TEntity>
    {
        base.HasRepository<TService>();
        Services.AddTransient<IEntityRepository<TEntity>, TService>();
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity> HasRepository(Func<IServiceProvider, IEntityRepository<TEntity>> factory)
    {
        base.HasRepository(factory);
        Services.AddTransient(factory);
        return this;
    }

    public new EntityServiceBuilder<TContext, TEntity> HasManager<TService>()
        where TService : class, IEntityManager<TEntity>
    {
        base.HasManager<TService>();
        Services.AddTransient<IEntityManager<TEntity>, TService>();
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity> HasManager(Func<IServiceProvider, IEntityManager<TEntity>> factory)
    {
        base.HasManager(factory);
        Services.AddTransient(factory);
        return this;
    }

    // Query Builders
    public new EntityServiceBuilder<TContext, TEntity> AddDefaultQueryBuilder()
        => AddQueryBuilder<QueryBuilder<TEntity, SearchObject>>();

    public new EntityServiceBuilder<TContext, TEntity> AddQueryBuilder<TImplementation>()
        where TImplementation : class, IQueryBuilder<TEntity, SearchObject>
    {
        Services.AddTransient<IQueryBuilder<TEntity, SearchObject>, TImplementation>();
        HasQueryBuilder = true;
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity> AddQueryBuilder(Func<IServiceProvider, IQueryBuilder<TEntity, SearchObject>> factory)
    {
        Services.AddTransient(factory);
        HasQueryBuilder = true;
        return this;
    }

    public new EntityServiceBuilder<TContext, TEntity> AddQueryFilter<TImplementation>()
        where TImplementation : class, IFilteredQueryBuilder<TEntity, SearchObject>
    {
        Services.AddTransient<IFilteredQueryBuilder<TEntity, SearchObject>, TImplementation>();
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity> AddQueryFilter(Func<IServiceProvider, IFilteredQueryBuilder<TEntity, SearchObject>> factory)
    {
        Services.AddTransient(factory);
        return this;
    }

    // Complex Entity service
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

public class EntityServiceBuilder<TContext, TEntity, TKey>(IServiceCollection services)
    : EntityServiceCollection<TContext>(services), IEntityServiceBuilder<TEntity, TKey>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
{
    // ToDo: make this a getter only that checks the Services collection?
    public bool HasQueryBuilder { get; set; }

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
    /// <summary>
    /// Adds an implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntity, TKey}"/></item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    public virtual EntityServiceBuilder<TContext, TEntity, TKey> AddService(Func<IServiceProvider, IEntityService<TEntity, TKey>> factory)
    {
        Services.AddTransient(factory);
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

    // Query Builders
    public EntityServiceBuilder<TContext, TEntity, TKey> AddDefaultQueryBuilder()
        => AddQueryBuilder<QueryBuilder<TEntity, TKey, SearchObject<TKey>>>();

    public EntityServiceBuilder<TContext, TEntity, TKey> AddQueryBuilder<TImplementation>()
        where TImplementation : class, IQueryBuilder<TEntity, TKey, SearchObject<TKey>>
    {
        Services.AddTransient<IQueryBuilder<TEntity, TKey, SearchObject<TKey>>, TImplementation>();
        HasQueryBuilder = true;
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, TKey> AddQueryBuilder(Func<IServiceProvider, IQueryBuilder<TEntity, TKey, SearchObject<TKey>>> factory)
    {
        Services.AddTransient(factory);
        HasQueryBuilder = true;
        return this;
    }

    public EntityServiceBuilder<TContext, TEntity, TKey> AddQueryFilter<TImplementation>()
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, SearchObject<TKey>>
    {
        Services.AddTransient<IFilteredQueryBuilder<TEntity, TKey, SearchObject<TKey>>, TImplementation>();
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, TKey> AddQueryFilter(Func<IServiceProvider, IFilteredQueryBuilder<TEntity, TKey, SearchObject<TKey>>> factory)
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

public class EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject>(IServiceCollection services)
    : EntityServiceBuilder<TContext, TEntity, TKey>(services)
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
{
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

    // Query Builders
    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> AddDefaultQueryBuilder()
        => AddQueryBuilder<QueryBuilder<TEntity, TKey, TSearchObject>>();

    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> AddQueryBuilder<TImplementation>()
        where TImplementation : class, IQueryBuilder<TEntity, TKey, TSearchObject>
    {
        Services.AddTransient<IQueryBuilder<TEntity, TKey, TSearchObject>, TImplementation>();
        HasQueryBuilder = true;
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> AddQueryBuilder(Func<IServiceProvider, IQueryBuilder<TEntity, TKey, TSearchObject>> factory)
    {
        Services.AddTransient(factory);
        HasQueryBuilder = true;
        return this;
    }

    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> AddQueryFilter<TImplementation>()
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, TSearchObject>
    {
        Services.AddTransient<IFilteredQueryBuilder<TEntity, TKey, TSearchObject>, TImplementation>();
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> AddQueryFilter(Func<IServiceProvider, IFilteredQueryBuilder<TEntity, TKey, TSearchObject>> factory)
    {
        Services.AddTransient(factory);
        return this;
    }

    // Complex Entity service
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