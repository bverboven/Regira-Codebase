using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.Normalizers;
using Regira.Entities.DependencyInjection.Preppers;
using Regira.Entities.DependencyInjection.Primers;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Entities.EFcore.Preppers;
using Regira.Entities.EFcore.Primers.Abstractions;
using Regira.Entities.EFcore.Processing;
using Regira.Entities.EFcore.Processing.Abstractions;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Mapping.Abstractions;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Services.Abstractions;
using System.Linq.Expressions;

namespace Regira.Entities.DependencyInjection.ServiceBuilders;

public partial class EntityServiceBuilder<TContext, TEntity, TKey>
{
    public bool HasEntityService() => HasService<IEntityService<TEntity, TKey>>();
    public bool HasService<TService>() => Services.Any(s => s.ServiceType == typeof(TService));

    // Entity mapping
    public MappedEntityServiceBuilder<TContext, TEntity, TKey, TDto, TInputDto> UseMapping<TDto, TInputDto>(Action<IEntityMapConfigurator>? mapAction = null)
    {
        if (Options.EntityMapConfiguratorFactory == null)
        {
            throw new NullReferenceException("Missing mapping configuration.");
        }

        var mapConfig = Options.EntityMapConfiguratorFactory.Invoke(Options.Services);
        mapConfig.Configure<TEntity, TDto>();
        mapConfig.Configure<TInputDto, TEntity>();
        mapAction?.Invoke(mapConfig);

        return new MappedEntityServiceBuilder<TContext, TEntity, TKey, TDto, TInputDto>(Options);
    }
    public MappedEntityServiceBuilder<TContext, TEntity, TKey> UseMapping(Type dto, Type inputDto, Action<IEntityMapConfigurator>? mapAction = null)
    {
        if (Options.EntityMapConfiguratorFactory == null)
        {
            throw new NullReferenceException("Missing mapping configuration.");
        }

        var mapConfig = Options.EntityMapConfiguratorFactory.Invoke(Options.Services);
        mapConfig.Configure(typeof(TEntity), dto);
        mapConfig.Configure(inputDto, typeof(TEntity));
        mapAction?.Invoke(mapConfig);

        return new MappedEntityServiceBuilder<TContext, TEntity, TKey>(Options);
    }
    public EntityServiceBuilder<TContext, TEntity, TKey> AddMapping<TSource, TTarget>()
    {
        if (Options.EntityMapConfiguratorFactory == null)
        {
            throw new NullReferenceException("Missing mapping configuration.");
        }

        var mapConfig = Options.EntityMapConfiguratorFactory.Invoke(Options.Services);
        mapConfig.Configure<TSource, TTarget>();

        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, TKey> AddMapping(Type sourceType, Type targetType)
    {
        if (Options.EntityMapConfiguratorFactory == null)
        {
            throw new NullReferenceException("Missing mapping configuration.");
        }

        var mapConfig = Options.EntityMapConfiguratorFactory.Invoke(Options.Services);
        mapConfig.Configure(sourceType, targetType);

        return this;
    }

    // Entity service
    /// <summary>
    /// Adds <see cref="EntityRepository{TEntity}"/> as implementation for:
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
    public EntityServiceBuilder<TContext, TEntity, TKey> Prepare(Action<TEntity> prepareFunc)
    {
        Services.AddPrepper(prepareFunc);
        return this;
    }
    public EntityServiceBuilder<TContext, TEntity, TKey> Prepare(Func<TEntity, TContext, Task> prepareFunc)
    {
        Services.AddPrepper<TContext, TEntity, TKey>(prepareFunc);
        return this;
    }
    // Related
    public EntityServiceBuilder<TContext, TEntity, TKey> Related<TRelated, TRelatedKey>(
        Expression<Func<TEntity, ICollection<TRelated>?>> navigationExpression, Action<TEntity>? prepareFunc = null)
        where TRelated : class, IEntity<TRelatedKey>
    {
        Services.AddPrepper(p => new RelatedCollectionPrepper<TContext, TEntity, TRelated, TKey, TRelatedKey>(p.GetRequiredService<TContext>(), navigationExpression));
        if (prepareFunc != null)
        {
            Prepare(prepareFunc);
        }

        return this;
    }
}