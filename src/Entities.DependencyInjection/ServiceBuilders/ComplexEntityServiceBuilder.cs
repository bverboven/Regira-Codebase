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
    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> HasRepository<TService>()
        where TService : class, IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>
    {
        base.HasRepository<TService>();
        Services.AddTransient<IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>, TService>();
        return this;
    }
    public ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> HasRepository(Func<IServiceProvider, IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>> factory)
    {
        base.HasRepository(factory);
        Services.AddTransient(factory);
        return this;
    }
    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> HasManager<TService>()
        where TService : class, IEntityManager<TEntity, TSearchObject, TSortBy, TIncludes>
    {
        base.HasManager<TService>();
        Services.AddTransient<IEntityManager<TEntity, TSearchObject, TSortBy, TIncludes>, TService>();
        return this;
    }
    public ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> HasManager(Func<IServiceProvider, IEntityManager<TEntity, TSearchObject, TSortBy, TIncludes>> factory)
    {
        base.HasManager(factory);
        Services.AddTransient(factory);
        return this;
    }

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

    // Read Service
    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> UseReadService<TService>()
        where TService : class, IEntityReadService<TEntity, TSearchObject, TSortBy, TIncludes>
    {
        base.UseReadService<TService>();
        Services.AddTransient<IEntityReadService<TEntity, TSearchObject, TSortBy, TIncludes>, TService>();
        return this;
    }

    // Query Builders
    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> AddDefaultQueryBuilder()
    {
        Services.AddDefaultQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>();
        return this;
    }
    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> UseQueryBuilder<TImplementation>()
        where TImplementation : class, IQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>
    {
        Services.UseQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes, TImplementation>();
        return this;
    }
    public ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> UseQueryBuilder(Func<IServiceProvider, IQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>> factory)
    {
        Services.UseQueryBuilder(factory);
        return this;
    }

    // Query Filters
    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> AddQueryFilter<TImplementation>()
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TSearchObject>
    {
        Services.AddQueryFilter<TEntity, TSearchObject, TImplementation>();
        return this;
    }
    public ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> AddQueryFilter(Func<IServiceProvider, IFilteredQueryBuilder<TEntity, TSearchObject>> factory)
    {
        Services.AddQueryFilter(factory);
        return this;
    }

    // Default SortBy
    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> UseDefaultSortBy(Func<IQueryable<TEntity>, IQueryable<TEntity>> sortBy)
    {
        base.UseDefaultSortBy(sortBy);
        Services.AddTransient<ISortedQueryBuilder<TEntity>>(_ => new SortedQueryBuilder<TEntity>(sortBy));
        return this;
    }
    // Default Includes
    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> UseDefaultIncludes(Func<IQueryable<TEntity>, IQueryable<TEntity>> addIncludes)
    {
        base.UseDefaultIncludes(addIncludes);
        Services.AddTransient<IIncludableQueryBuilder<TEntity>>(_ => new IncludableQueryBuilder<TEntity>(addIncludes));
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
    /// <inheritdoc cref="EntityServiceBuilder{TContext,TEntity}.HasRepository{TService}"/><br />
    /// <list type="bullet">
    ///     <item><see cref="IEntityRepository{TEntity, TSearchObject, TSortBy, TIncludes}"/></item>
    ///     <item><see cref="IEntityRepository{TEntity, TKey, TSearchObject, TSortBy, TIncludes}"/></item>
    /// </list>
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> HasRepository<TService>()
        where TService : class, IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    {
        base.HasRepository<TService>();
        Services.AddTransient<IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, TService>();
        return this;
    }
    /// <summary>
    /// <inheritdoc cref="EntityServiceBuilder{TContext,TEntity}.HasRepository(Func{IServiceProvider,IEntityRepository{TEntity}})"/><br />
    /// <list type="bullet">
    ///     <item><see cref="IEntityRepository{TEntity, TSearchObject, TSortBy, TIncludes}"/></item>
    ///     <item><see cref="IEntityRepository{TEntity, TKey, TSearchObject, TSortBy, TIncludes}"/></item>
    /// </list>
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> HasRepository(Func<IServiceProvider, IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>> factory)
    {
        base.HasRepository(factory);
        Services.AddTransient(factory);
        return this;
    }
    /// <summary>
    /// <inheritdoc cref="EntityServiceBuilder{TContext,TEntity}.HasManager{TService}"/>
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
        base.HasManager<TService>();
        Services.AddTransient<IEntityManager<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, TService>();
        return this;
    }
    /// <summary>
    /// <inheritdoc cref="EntityServiceBuilder{TContext,TEntity}.HasManager(Func{IServiceProvider,IEntityManager{TEntity}})"/>
    /// <list type="bullet">
    ///     <item><see cref="IEntityManager{TEntity, TSearchObject, TSortBy, TIncludes}"/></item>
    ///     <item><see cref="IEntityManager{TEntity, TKey, TSearchObject, TSortBy, TIncludes}"/></item>
    /// </list>
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> HasManager(Func<IServiceProvider, IEntityManager<TEntity, TKey, TSearchObject, TSortBy, TIncludes>> factory)
    {
        base.HasManager(factory);
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

    // Read Service
    public new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> UseReadService<TService>()
        where TService : class, IEntityReadService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    {
        base.UseReadService<TService>();
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
    public ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> UseQueryBuilder(Func<IServiceProvider, IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>> factory)
    {
        Services.UseQueryBuilder(factory);
        return this;
    }

    // Query Filters
    public new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> AddQueryFilter<TImplementation>()
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, TSearchObject>
    {
        Services.AddQueryFilter<TEntity, TKey, TSearchObject, TImplementation>();
        return this;
    }
    public new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> AddQueryFilter(Func<IServiceProvider, IFilteredQueryBuilder<TEntity, TKey, TSearchObject>> factory)
    {
        Services.AddQueryFilter(factory);
        return this;
    }
}
