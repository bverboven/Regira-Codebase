using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.DependencyInjection.ServiceBuilders;

public class ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes>(
    EntityServiceBuilder<TContext, TEntity, int, TSearchObject> services)
    : ComplexEntityServiceBuilder<TContext, TEntity, int, TSearchObject, TSortBy, TIncludes>(services)
    where TContext : DbContext
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    // Entity service
    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> AddDefaultService()
        => UseEntityService<EntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>>();

    /// <summary>
    /// Adds <typeparamref name="TService"/> implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntity, TKey, TSearchObject, TSortBy, TIncludes}"/></item>
    /// </list>
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> UseEntityService<TService>()
        where TService : class, IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>
    {
        base.UseEntityService<TService>();
        Services.AddTransient<IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>, TService>();
        Services.AddTransient<IEntityService<TEntity>, TService>();
        return this;
    }
    /// <summary>
    /// Adds an implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntity, TSearchObject, TSortBy, TIncludes}"/></item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    public virtual ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> UseEntityService(Func<IServiceProvider, IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>> factory)
    {
        base.UseEntityService(factory);
        Services.AddTransient(factory);
        return this;
    }

    // Entity Repository
    protected internal new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> HasRepositoryInner<TService>()
        where TService : class, IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>, IEntityRepository<TEntity>
    {
        Services.AddTransient<IEntityRepository<TEntity>, TService>();
        Services.AddTransient<IEntityRepository<TEntity, int>, TService>();
        Services.AddTransient<IEntityRepository<TEntity, int, TSearchObject>, TService>();
        Services.AddTransient<IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>, TService>();
        Services.AddTransient<IEntityRepository<TEntity, int, TSearchObject, TSortBy, TIncludes>, TService>();
        return this;
    }
    protected internal new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> HasRepositoryInner<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>, IEntityRepository<TEntity>
    {
        Services.AddTransient(factory);
        Services.AddTransient<IEntityRepository<TEntity, int>>(factory);
        Services.AddTransient<IEntityRepository<TEntity, int, TSearchObject>>(factory);
        Services.AddTransient<IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>>(factory);
        Services.AddTransient<IEntityRepository<TEntity, int, TSearchObject, TSortBy, TIncludes>>(factory);
        return this;
    }
    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> HasRepository<TService>()
        where TService : class, IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>, IEntityRepository<TEntity>
    {
        UseEntityService<TService>();
        HasRepositoryInner<TService>();
        return this;
    }
    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> HasRepository<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>, IEntityRepository<TEntity>
    {
        UseEntityService(factory);
        HasRepositoryInner(factory);
        return this;
    }

    // Entity Manager
    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> HasManager<TService>()
        where TService : class, IEntityManager<TEntity, TSearchObject, TSortBy, TIncludes>, IEntityManager<TEntity>
    {
        UseEntityService<TService>();
        Services.AddTransient<IEntityManager<TEntity>, TService>();
        Services.AddTransient<IEntityManager<TEntity, int>, TService>();
        Services.AddTransient<IEntityManager<TEntity, int, TSearchObject>, TService>();
        Services.AddTransient<IEntityManager<TEntity, TSearchObject, TSortBy, TIncludes>, TService>();
        Services.AddTransient<IEntityManager<TEntity, int, TSearchObject, TSortBy, TIncludes>, TService>();
        return this;
    }
    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> HasManager<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityManager<TEntity, TSearchObject, TSortBy, TIncludes>, IEntityManager<TEntity>
    {
        UseEntityService(factory);
        Services.AddTransient(factory);
        Services.AddTransient<IEntityManager<TEntity, int>>(factory);
        Services.AddTransient<IEntityManager<TEntity, int, TSearchObject>>(factory);
        Services.AddTransient<IEntityManager<TEntity, TSearchObject, TSortBy, TIncludes>>(factory);
        Services.AddTransient<IEntityManager<TEntity, int, TSearchObject, TSortBy, TIncludes>>(factory);
        return this;
    }

    // Query Builders
    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> AddDefaultQueryBuilder()
    {
        Services.AddDefaultQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>();
        Services.UseQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes, QueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>>();
        return this;
    }

    // Query Filters
    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> AddQueryFilter<TImplementation>()
        where TImplementation : class, IFilteredQueryBuilder<TEntity, int, TSearchObject>
    {
        Services.AddQueryFilter<TEntity, TSearchObject, TImplementation>();
        return this;
    }
}
public class ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>(
    EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject> services)
    : EntityServiceBuilder<TContext, TEntity, TKey, TSearchObject>(services)
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    /// <summary>
    /// <list type="bullet">
    ///     <item><see cref="IEntityManager{TEntity, TSearchObject, TSortBy, TIncludes}"/></item>
    ///     <item><see cref="IEntityManager{TEntity, TKey, TSearchObject, TSortBy, TIncludes}"/></item>
    /// </list>
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> HasManager<TService>()
        where TService : class, IEntityManager<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    {
        UseEntityService<TService>();
        Services.AddTransient<IEntityManager<TEntity, TKey>, TService>();
        Services.AddTransient<IEntityManager<TEntity, TKey, TSearchObject>, TService>();
        Services.AddTransient<IEntityManager<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, TService>();
        return this;
    }
    /// <summary>
    /// <list type="bullet">
    ///     <item><see cref="IEntityManager{TEntity, TSearchObject, TSortBy, TIncludes}"/></item>
    ///     <item><see cref="IEntityManager{TEntity, TKey, TSearchObject, TSortBy, TIncludes}"/></item>
    /// </list>
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> HasManager(Func<IServiceProvider, IEntityManager<TEntity, TKey, TSearchObject, TSortBy, TIncludes>> factory)
    {
        UseEntityService(factory);
        Services.AddTransient(factory);
        return this;
    }

    // Entity service
    public new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> AddDefaultService()
        => UseEntityService<EntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>>();

    /// <summary>
    /// Adds <typeparamref name="TService"/> implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntity, TKey, TSearchObject, TSortBy, TIncludes}"/></item>
    /// </list>
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> UseEntityService<TService>()
        where TService : class, IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    {
        base.UseEntityService<TService>();
        Services.AddTransient<IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, TService>();
        return this;
    }
    /// <summary>
    /// Adds an implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntity, TKey, TSearchObject, TSortBy, TIncludes}"/></item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    public virtual ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> UseEntityService(Func<IServiceProvider, IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>> factory)
    {
        base.UseEntityService(factory);
        Services.AddTransient(factory);
        return this;
    }

    // Entity Repository
    protected internal new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> HasRepositoryInner<TService>()
        where TService : class, IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, IEntityRepository<TEntity, TKey>
    {
        Services.AddTransient<IEntityRepository<TEntity, TKey>, TService>();
        Services.AddTransient<IEntityRepository<TEntity, TKey, TSearchObject>, TService>();
        Services.AddTransient<IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, TService>();
        return this;
    }
    protected internal new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> HasRepositoryInner<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, IEntityRepository<TEntity, TKey>
    {
        Services.AddTransient(factory);
        Services.AddTransient<IEntityRepository<TEntity, TKey>>(factory);
        Services.AddTransient<IEntityRepository<TEntity, TKey, TSearchObject>>(factory);
        Services.AddTransient<IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>>(factory);
        return this;
    }
    public new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> HasRepository<TService>()
        where TService : class, IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, IEntityRepository<TEntity, TKey>
    {
        UseEntityService<TService>();
        HasRepositoryInner<TService>();
        return this;
    }
    public new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> HasRepository<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, IEntityRepository<TEntity, TKey>
    {
        UseEntityService(factory);
        HasRepositoryInner(factory);
        return this;
    }

    // Read Service
    public new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> UseReadService<TService>()
        where TService : class, IEntityReadService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    {
        Services.AddTransient<IEntityReadService<TEntity, TKey>, TService>();
        Services.AddTransient<IEntityReadService<TEntity, TKey, TSearchObject>, TService>();
        Services.AddTransient<IEntityReadService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, TService>();
        return this;
    }

    // Query Builders
    public new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> AddDefaultQueryBuilder()
    {
        Services.AddDefaultQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>();
        return this;
    }
    public new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> UseQueryBuilder<TImplementation>()
        where TImplementation : class, IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    {
        Services.UseQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TImplementation>();
        return this;
    }
    public new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> UseQueryBuilder<TQueryBuilder>(Func<IServiceProvider, TQueryBuilder> factory)
        where TQueryBuilder : class, IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    {
        Services.UseQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes, TQueryBuilder>(factory);
        return this;
    }

    // Query Filters
    public new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> AddQueryFilter<TImplementation>()
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, TSearchObject>
    {
        Services.AddQueryFilter<TEntity, TKey, TSearchObject, TImplementation>();
        return this;
    }
    public new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> AddQueryFilter<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, TSearchObject>
    {
        Services.AddQueryFilter<TEntity, TKey, TSearchObject, TImplementation>(factory);
        return this;
    }
    public new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> Filter(Func<IQueryable<TEntity>, TSearchObject?, IQueryable<TEntity>> filterFunc)
    {
        AddQueryFilter(_ => new EntityQueryFilter<TEntity, TKey, TSearchObject>(filterFunc));
        return this;
    }

    // SortBy
    public ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> SortBy<TImplementation>()
        where TImplementation : class, ISortedQueryBuilder<TEntity, TKey, TSortBy>
    {
        Services.AddTransient<ISortedQueryBuilder<TEntity, TKey, TSortBy>, TImplementation>();
        return this;
    }
    public ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> SortBy<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, ISortedQueryBuilder<TEntity, TKey, TSortBy>
    {
        Services.AddTransient<ISortedQueryBuilder<TEntity, TKey, TSortBy>>(factory);
        return this;
    }
    public ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> SortBy(Func<IQueryable<TEntity>, TSortBy?, IQueryable<TEntity>> sortByFunc)
    {
        Services.AddTransient<ISortedQueryBuilder<TEntity, TKey, TSortBy>>(_ => new SortedQueryBuilder<TEntity, TKey, TSortBy>(sortByFunc));
        return this;
    }

    // Includes
    public ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> Includes<TImplementation>()
        where TImplementation : class, IIncludableQueryBuilder<TEntity, TKey, TIncludes>
    {
        Services.AddTransient<IIncludableQueryBuilder<TEntity, TKey, TIncludes>, TImplementation>();
        return this;
    }
    public ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> Includes<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IIncludableQueryBuilder<TEntity, TKey, TIncludes>
    {
        Services.AddTransient<IIncludableQueryBuilder<TEntity, TKey, TIncludes>>(factory);
        return this;
    }
    public ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> Includes(Func<IQueryable<TEntity>, TIncludes?, IQueryable<TEntity>> addIncludes)
    {
        Services.AddTransient<IIncludableQueryBuilder<TEntity, TKey, TIncludes>>(_ => new IncludableQueryBuilder<TEntity, TKey, TIncludes>(addIncludes));
        return this;
    }
}
