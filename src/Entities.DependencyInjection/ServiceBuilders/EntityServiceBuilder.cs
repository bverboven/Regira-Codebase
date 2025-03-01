using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.DependencyInjection.Abstractions;
using Regira.Entities.DependencyInjection.Normalizers;
using Regira.Entities.DependencyInjection.Primers;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Entities.EFcore.Preppers;
using Regira.Entities.EFcore.Preppers.Abstractions;
using Regira.Entities.EFcore.Primers.Abstractions;
using Regira.Entities.EFcore.Processing;
using Regira.Entities.EFcore.Processing.Abstractions;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using System.Linq.Expressions;

namespace Regira.Entities.DependencyInjection.ServiceBuilders;

public class EntityServiceBuilder<TContext, TEntity>(IServiceCollection services)
    : EntityServiceBuilder<TContext, TEntity, int>(services), IEntityServiceBuilder<TEntity>
    where TContext : DbContext
    where TEntity : class, IEntity<int>
{
    public new bool HasEntityService() => HasService<IEntityService<TEntity>>();

    // Entity service
    public new EntityServiceBuilder<TContext, TEntity> AddDefaultService()
        => UseEntityService<EntityRepository<TEntity>>();
    public new EntityServiceBuilder<TContext, TEntity> UseEntityService<TService>()
        where TService : class, IEntityService<TEntity>, IEntityService<TEntity, int, SearchObject<int>>
    {
        Services.AddTransient<IEntityService<TEntity>, TService>();
        Services.AddTransient<IEntityService<TEntity, int>, TService>();
        Services.AddTransient<IEntityService<TEntity, int, SearchObject<int>>, TService>();
        return this;
    }
    /// <summary>
    /// Adds an implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntity}"/></item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    public new EntityServiceBuilder<TContext, TEntity> UseEntityService<TService>(Func<IServiceProvider, TService> factory)
        where TService : class, IEntityService<TEntity>, IEntityService<TEntity, int, SearchObject<int>>
    {
        Services.AddTransient<IEntityService<TEntity>>(factory);
        Services.AddTransient<IEntityService<TEntity, int>>(factory);
        Services.AddTransient<IEntityService<TEntity, int, SearchObject<int>>>(factory);
        Services.AddTransient(factory);
        return this;
    }

    // Entity Repository
    protected internal new EntityServiceBuilder<TContext, TEntity> HasRepositoryInner<TService>()
        where TService : class, IEntityRepository<TEntity>, IEntityRepository<TEntity, int, SearchObject<int>>
    {
        Services.AddTransient<IEntityRepository<TEntity>, TService>();
        Services.AddTransient<IEntityRepository<TEntity, int>, TService>();
        Services.AddTransient<IEntityRepository<TEntity, int, SearchObject<int>>, TService>();
        return this;
    }
    protected internal new EntityServiceBuilder<TContext, TEntity> HasRepositoryInner<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityRepository<TEntity>, IEntityRepository<TEntity, int, SearchObject<int>>
    {
        Services.AddTransient(factory);
        Services.AddTransient<IEntityRepository<TEntity, int>>(factory);
        Services.AddTransient<IEntityRepository<TEntity, int, SearchObject<int>>>(factory);
        return this;
    }
    public new EntityServiceBuilder<TContext, TEntity> HasRepository<TService>()
        where TService : class, IEntityRepository<TEntity>, IEntityRepository<TEntity, int, SearchObject<int>>
    {
        UseEntityService<TService>();
        HasRepositoryInner<TService>();
        return this;
    }
    public new EntityServiceBuilder<TContext, TEntity> HasRepository<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityRepository<TEntity>, IEntityRepository<TEntity, int, SearchObject<int>>
    {
        UseEntityService(factory);
        HasRepositoryInner(factory);
        return this;
    }

    // Entity Manager
    public new EntityServiceBuilder<TContext, TEntity> HasManager<TService>()
        where TService : class, IEntityManager<TEntity>, IEntityManager<TEntity, int, SearchObject<int>>
    {
        UseEntityService<TService>();
        Services.AddTransient<IEntityManager<TEntity>, TService>();
        Services.AddTransient<IEntityManager<TEntity, int>, TService>();
        Services.AddTransient<IEntityManager<TEntity, int, SearchObject<int>>, TService>();
        return this;
    }
    public new EntityServiceBuilder<TContext, TEntity> HasManager<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityManager<TEntity>, IEntityManager<TEntity, int, SearchObject<int>>
    {
        UseEntityService(factory);
        Services.AddTransient(factory);
        Services.AddTransient<IEntityManager<TEntity, int>>(factory);
        Services.AddTransient<IEntityManager<TEntity, int, SearchObject<int>>>(factory);
        return this;
    }

    // Query Builders
    public new EntityServiceBuilder<TContext, TEntity> AddDefaultQueryBuilder()
    {
        Services.AddDefaultQueryBuilder<TEntity>();
        Services.UseQueryBuilder<TEntity, QueryBuilder<TEntity>>();
        return this;
    }
    public new EntityServiceBuilder<TContext, TEntity> UseQueryBuilder<TImplementation>()
        where TImplementation : class, IQueryBuilder<TEntity, int, SearchObject<int>, EntitySortBy, EntityIncludes>
    {
        Services.UseQueryBuilder<TEntity, TImplementation>();
        return this;
    }
    public new EntityServiceBuilder<TContext, TEntity> UseQueryBuilder<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IQueryBuilder<TEntity, int, SearchObject<int>, EntitySortBy, EntityIncludes>
    {
        Services.UseQueryBuilder<TEntity, TImplementation>(factory);
        return this;
    }

    // Preppers
    public new EntityServiceBuilder<TContext, TEntity> Prepare(Func<TEntity, TContext, Task> prepareFunc)
    {
        Services.AddTransient<IEntityPrepper>(p => new EntityPrepper<TContext, TEntity, int>(p.GetRequiredService<TContext>(), prepareFunc));

        return this;
    }
    // Related
    public EntityServiceBuilder<TContext, TEntity> Related<TRelated>(
        Expression<Func<TEntity, ICollection<TRelated>?>> navigationExpression)
        where TRelated : class, IEntity<int>
    {
        AddPrepper(p => new RelatedCollectionPrepper<TContext, TEntity, TRelated, int, int>(p.GetRequiredService<TContext>(), navigationExpression));

        return this;
    }
}

public class EntityServiceBuilder<TContext, TEntity, TKey>(IServiceCollection services)
    : EntityServiceCollection<TContext>(services), IEntityServiceBuilder<TEntity, TKey>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
{
    public bool HasEntityService() => HasService<IEntityService<TEntity, TKey>>();
    public bool HasService<TService>() => Services.Any(s => s.ServiceType == typeof(TService));

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
    public EntityServiceBuilder<TContext, TEntity, TKey> AddMapping<TDto, TInputDto>()
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
    public EntityServiceBuilder<TContext, TEntity, TKey> AddMappingProfile<TProfile>()
        where TProfile : Profile, new()
    {
        Services.AddAutoMapper(cfg => cfg.AddProfile<TProfile>());
        return this;
    }

    // Entity service
    /// <summary>
    /// Adds <see cref="EntityRepository{TContext, TEntity}"/> as implementation for:
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntity}"/></item>
    ///     <item><see cref="IEntityService{TEntity, TKey}"/></item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    public EntityServiceBuilder<TContext, TEntity, TKey> AddDefaultService()
        => UseEntityService<EntityRepository<TEntity, TKey>>();
    /// <summary>
    /// Adds <typeparamref name="TService"/> implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntity, TKey}"/></item>
    /// </list>
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public EntityServiceBuilder<TContext, TEntity, TKey> UseEntityService<TService>()
        where TService : class, IEntityService<TEntity, TKey>, IEntityService<TEntity, TKey, SearchObject<TKey>>
    {
        Services.AddTransient<IEntityService<TEntity, TKey>, TService>();
        Services.AddTransient<IEntityService<TEntity, TKey, SearchObject<TKey>>, TService>();
        return this;
    }
    /// <summary>
    /// Adds an implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntity, TKey}"/></item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    public EntityServiceBuilder<TContext, TEntity, TKey> UseEntityService<TService>(Func<IServiceProvider, TService> factory)
        where TService : class, IEntityService<TEntity, TKey, SearchObject<TKey>>
    {
        Services.AddTransient<IEntityService<TEntity, TKey>>(factory);
        Services.AddTransient<IEntityService<TEntity, TKey, SearchObject<TKey>>>(factory);
        Services.AddTransient(factory);
        return this;
    }

    // Read Service
    public EntityServiceBuilder<TContext, TEntity, TKey> UseReadService<TService>()
        where TService : class, IEntityReadService<TEntity, TKey, SearchObject<TKey>>
    {
        Services.AddTransient<IEntityReadService<TEntity, TKey>, TService>();
        Services.AddTransient<IEntityReadService<TEntity, TKey, SearchObject<TKey>>, TService>();
        return this;
    }
    // Write Service
    public EntityServiceBuilder<TContext, TEntity, TKey> UseWriteService<TService>()
        where TService : class, IEntityWriteService<TEntity, TKey>
    {
        Services.AddTransient<IEntityWriteService<TEntity, TKey>, TService>();
        return this;
    }

    // Entity Repository
    protected internal EntityServiceBuilder<TContext, TEntity, TKey> HasRepositoryInner<TService>()
        where TService : class, IEntityRepository<TEntity, TKey>, IEntityRepository<TEntity, TKey, SearchObject<TKey>>
    {
        Services.AddTransient<IEntityRepository<TEntity, TKey>, TService>();
        Services.AddTransient<IEntityRepository<TEntity, TKey, SearchObject<TKey>>, TService>();
        return this;
    }
    protected internal EntityServiceBuilder<TContext, TEntity, TKey> HasRepositoryInner<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityRepository<TEntity, TKey>, IEntityRepository<TEntity, TKey, SearchObject<TKey>>
    {
        Services.AddTransient(factory);
        Services.AddTransient<IEntityRepository<TEntity, TKey>>(factory);
        Services.AddTransient<IEntityRepository<TEntity, TKey, SearchObject<TKey>>>(factory);
        return this;
    }
    /// <summary>
    /// Adds <typeparamref name="TService"/> implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityRepository{TEntity}"/></item>
    ///     <item><see cref="IEntityRepository{TEntity, TKey}"/></item>
    /// </list>
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public EntityServiceBuilder<TContext, TEntity, TKey> HasRepository<TService>()
        where TService : class, IEntityRepository<TEntity, TKey, SearchObject<TKey>>
    {
        UseEntityService<TService>();
        HasRepositoryInner<TService>();
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
    public EntityServiceBuilder<TContext, TEntity, TKey> HasRepository<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityRepository<TEntity, TKey>, IEntityRepository<TEntity, TKey, SearchObject<TKey>>
    {
        UseEntityService(factory);
        HasRepositoryInner(factory);
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
    public EntityServiceBuilder<TContext, TEntity, TKey> HasManager<TService>()
        where TService : class, IEntityManager<TEntity, TKey>, IEntityManager<TEntity, TKey, SearchObject<TKey>>
    {
        UseEntityService<TService>();
        Services.AddTransient<IEntityManager<TEntity, TKey>, TService>();
        Services.AddTransient<IEntityManager<TEntity, TKey, SearchObject<TKey>>, TService>();
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
    public EntityServiceBuilder<TContext, TEntity, TKey> HasManager<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityManager<TEntity, TKey>, IEntityManager<TEntity, TKey, SearchObject<TKey>>
    {
        UseEntityService(factory);
        Services.AddTransient(factory);
        Services.AddTransient<IEntityManager<TEntity, TKey>>(factory);
        Services.AddTransient<IEntityManager<TEntity, TKey, SearchObject<TKey>>>(factory);
        return this;
    }

    // Query Builders
    public EntityServiceBuilder<TContext, TEntity, TKey> AddDefaultQueryBuilder()
    {
        Services.AddDefaultQueryBuilder<TEntity, TKey>();
        Services.UseQueryBuilder<TEntity, TKey, QueryBuilder<TEntity, TKey>>();
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, TKey> UseQueryBuilder<TImplementation>()
        where TImplementation : class, IQueryBuilder<TEntity, TKey, SearchObject<TKey>, EntitySortBy, EntityIncludes>
    {
        Services.UseQueryBuilder<TEntity, TKey, SearchObject<TKey>, EntitySortBy, EntityIncludes, TImplementation>();
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, TKey> UseQueryBuilder<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IQueryBuilder<TEntity, TKey, SearchObject<TKey>, EntitySortBy, EntityIncludes>
    {
        Services.UseQueryBuilder<TEntity, TKey, SearchObject<TKey>, EntitySortBy, EntityIncludes, TImplementation>(factory);
        return this;
    }

    // Query Filters
    public EntityServiceBuilder<TContext, TEntity, TKey> AddQueryFilter<TImplementation>()
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, SearchObject<TKey>>
    {
        Services.AddQueryFilter<TEntity, TKey, SearchObject<TKey>, TImplementation>();
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, TKey> AddQueryFilter<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, SearchObject<TKey>>
    {
        Services.AddQueryFilter<TEntity, TKey, SearchObject<TKey>, TImplementation>(factory);
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, TKey> Filter(Func<IQueryable<TEntity>, SearchObject<TKey>?, IQueryable<TEntity>> filterFunc)
    {
        AddQueryFilter(_ => new EntityQueryFilter<TEntity, TKey, SearchObject<TKey>>(filterFunc));
        return this;
    }


    // Default SortBy
    public EntityServiceBuilder<TContext, TEntity, TKey> SortBy(Func<IQueryable<TEntity>, IQueryable<TEntity>> sortBy)
    {
        Services.AddTransient<ISortedQueryBuilder<TEntity, TKey>>(_ => new SortedQueryBuilder<TEntity, TKey>(sortBy));
        return this;
    }
    // Default Includes
    public EntityServiceBuilder<TContext, TEntity, TKey> Includes(Func<IQueryable<TEntity>, EntityIncludes?, IQueryable<TEntity>> addIncludes)
    {
        Services.AddTransient<IIncludableQueryBuilder<TEntity, TKey>>(_ => new IncludableQueryBuilder<TEntity, TKey>(addIncludes));
        return this;
    }

    // Primers
    public EntityServiceBuilder<TContext, TEntity, TKey> AddPrimer<TPrimer>()
        where TPrimer : class, IEntityPrimer<TEntity>
    {
        Services.AddPrimer<TEntity, TPrimer>();
        return this;
    }

    // EntityNormalizers
    public EntityServiceBuilder<TContext, TEntity, TKey> AddNormalizer<TNormalizer>()
        where TNormalizer : class, IEntityNormalizer<TEntity>
    {
        Services.AddNormalizer<TEntity, TNormalizer>();
        return this;
    }

    // Entity Processor
    public EntityServiceBuilder<TContext, TEntity, TKey> Process(Func<IList<TEntity>, EntityIncludes?, Task> process)
    {
        Services.AddTransient<IEntityProcessor<TEntity, EntityIncludes>>(_ => new EntityProcessor<TEntity, EntityIncludes>(process));
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, TKey> Process(Action<TEntity, EntityIncludes?> process)
    {
        Services.AddTransient<IEntityProcessor<TEntity, EntityIncludes>>(_ => new EntityProcessor<TEntity, EntityIncludes>((items, includes) =>
        {
            foreach (var item in items)
            {
                process(item, includes);
            }
            return Task.CompletedTask;
        }));

        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, TKey> Process<TImplementation>()
        where TImplementation : class, IEntityProcessor<TEntity, EntityIncludes>
    {
        Services.AddTransient<IEntityProcessor<TEntity, EntityIncludes>, TImplementation>();
        return this;
    }

    // Preppers
    public EntityServiceBuilder<TContext, TEntity, TKey> AddPrepper<TImplementation>()
        where TImplementation : class, IEntityPrepper<TEntity, TKey>
    {
        Services.AddTransient<IEntityPrepper, TImplementation>();
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, TKey> AddPrepper<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityPrepper<TEntity, TKey>
    {
        Services.AddTransient<IEntityPrepper>(factory);
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, TKey> Prepare(Action<TEntity> prepareFunc)
    {
        Services.AddTransient<IEntityPrepper>(p => new EntityPrepper<TContext, TEntity, TKey>(p.GetRequiredService<TContext>(), (item, _) =>
        {
            prepareFunc(item);
            return Task.CompletedTask;
        }));

        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, TKey> Prepare(Func<TEntity, TContext, Task> prepareFunc)
    {
        Services.AddTransient<IEntityPrepper>(p => new EntityPrepper<TContext, TEntity, TKey>(p.GetRequiredService<TContext>(), prepareFunc));

        return this;
    }
    // Related
    public EntityServiceBuilder<TContext, TEntity, TKey> Related<TRelated, TRelatedKey>(
        Expression<Func<TEntity, ICollection<TRelated>?>> navigationExpression, Action<TEntity>? prepareFunc = null)
        where TRelated : class, IEntity<TRelatedKey>
    {
        AddPrepper(p => new RelatedCollectionPrepper<TContext, TEntity, TRelated, TKey, TRelatedKey>(p.GetRequiredService<TContext>(), navigationExpression));
        if (prepareFunc != null)
        {
            Prepare(prepareFunc);
        }

        return this;
    }
}

public class EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject>(IServiceCollection services)
    : EntityServiceBuilder<TContext, TEntity, TKey>(services), IEntityServiceBuilder<TEntity, TKey, TSearchObject>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
{
    public new bool HasEntityService() => HasService<IEntityService<TEntity, TKey, TSearchObject>>();

    // Entity services
    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> AddDefaultService()
        => UseEntityService<EntityRepository<TEntity, TKey, TSearchObject>>();
    /// <summary> 
    /// Adds an implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntity, TKey, TSearchObject}"/></item>
    /// </list>
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> UseEntityService<TService>()
        where TService : class, IEntityService<TEntity, TKey, TSearchObject>
    {
        Services.AddTransient<IEntityService<TEntity, TKey>, TService>();
        Services.AddTransient<IEntityService<TEntity, TKey, TSearchObject>, TService>();
        return this;
    }
    /// <summary>
    /// Adds an implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntity, TKey, TSearchObject}"/></item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> UseEntityService<TService>(Func<IServiceProvider, TService> factory)
        where TService : class, IEntityService<TEntity, TKey, TSearchObject>
    {
        Services.AddTransient<IEntityService<TEntity, TKey>>(factory);
        Services.AddTransient<IEntityService<TEntity, TKey, TSearchObject>>(factory);
        Services.AddTransient(factory);
        return this;
    }

    // Read Service
    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> UseReadService<TService>()
        where TService : class, IEntityReadService<TEntity, TKey, TSearchObject>
    {
        Services.AddTransient<IEntityReadService<TEntity, TKey>, TService>();
        Services.AddTransient<IEntityReadService<TEntity, TKey, TSearchObject>, TService>();
        return this;
    }
    // Write Service
    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> UseWriteService<TService>()
        where TService : class, IEntityWriteService<TEntity, TKey>
    {
        Services.AddTransient<IEntityWriteService<TEntity, TKey>, TService>();
        return this;
    }

    // Entity Repository
    protected internal new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> HasRepositoryInner<TService>()
        where TService : class, IEntityRepository<TEntity, TKey, TSearchObject>
    {
        Services.AddTransient<IEntityRepository<TEntity, TKey>, TService>();
        Services.AddTransient<IEntityRepository<TEntity, TKey, TSearchObject>, TService>();
        return this;
    }
    protected internal new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> HasRepositoryInner<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityRepository<TEntity, TKey, TSearchObject>
    {
        Services.AddTransient(factory);
        Services.AddTransient<IEntityRepository<TEntity, TKey>>(factory);
        Services.AddTransient<IEntityRepository<TEntity, TKey, TSearchObject>>(factory);
        return this;
    }
    /// <summary>
    /// Adds <typeparamref name="TService"/> implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityRepository{TEntity}"/></item>
    ///     <item><see cref="IEntityRepository{TEntity, TKey}"/></item>
    /// </list>
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> HasRepository<TService>()
        where TService : class, IEntityRepository<TEntity, TKey, TSearchObject>
    {
        UseEntityService<TService>();
        HasRepositoryInner<TService>();
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
    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> HasRepository<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityRepository<TEntity, TKey>, IEntityRepository<TEntity, TKey, TSearchObject>
    {
        UseEntityService(factory);
        HasRepositoryInner(factory);
        return this;
    }
    // Entity Manager
    /// <summary>
    /// Adds <typeparamref name="TService"/> implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityManager{TEntity}"/></item>
    ///     <item><see cref="IEntityManager{TEntity, TKey, TSearchObject}"/></item>
    /// </list>
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> HasManager<TService>()
        where TService : class, IEntityManager<TEntity, TKey, TSearchObject>
    {
        UseEntityService<TService>();
        Services.AddTransient<IEntityManager<TEntity, TKey>, TService>();
        Services.AddTransient<IEntityManager<TEntity, TKey, TSearchObject>, TService>();
        return this;
    }
    /// <summary>
    /// Adds implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityManager{TEntity}"/></item>
    ///     <item><see cref="IEntityManager{TEntity, TKey, TSearchObject}"/></item>
    /// </list>
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> HasManager<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityManager<TEntity, TKey, TSearchObject>
    {
        UseEntityService(factory);
        Services.AddTransient(factory);
        Services.AddTransient<IEntityManager<TEntity, TKey>>(factory);
        Services.AddTransient<IEntityManager<TEntity, TKey, TSearchObject>>(factory);
        return this;
    }

    // Query Builders
    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> AddDefaultQueryBuilder()
    {
        Services.AddDefaultQueryBuilder<TEntity, TKey, TSearchObject>();
        Services.UseQueryBuilder<TEntity, TKey, TSearchObject, QueryBuilder<TEntity, TKey, TSearchObject>>();
        return this;
    }
    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> UseQueryBuilder<TImplementation>()
        where TImplementation : class, IQueryBuilder<TEntity, TKey, TSearchObject, EntitySortBy, EntityIncludes>
    {
        Services.UseQueryBuilder<TEntity, TKey, TSearchObject, TImplementation>();
        return this;
    }
    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> UseQueryBuilder<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IQueryBuilder<TEntity, TKey, TSearchObject, EntitySortBy, EntityIncludes>
    {
        Services.UseQueryBuilder<TEntity, TKey, TSearchObject, TImplementation>(factory);
        return this;
    }

    // Query Filters
    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> AddQueryFilter<TImplementation>()
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, TSearchObject>
    {
        Services.AddQueryFilter<TEntity, TKey, TSearchObject, TImplementation>();
        return this;
    }
    public new EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> AddQueryFilter<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, TSearchObject>
    {
        Services.AddQueryFilter<TEntity, TKey, TSearchObject, TImplementation>(factory);
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> Filter(Func<IQueryable<TEntity>, TSearchObject?, IQueryable<TEntity>> filterFunc)
    {
        AddQueryFilter(_ => new EntityQueryFilter<TEntity, TKey, TSearchObject>(filterFunc));
        return this;
    }
}