using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.DependencyInjection;

public class ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes>(
    EntityServiceBuilder<TContext, TEntity, int> services)
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

    // Query Builders
    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> AddDefaultQueryBuilders()
    {
        Services.AddTransient<IQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>,
            QueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>>();

        return this;
    }

    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> AddQueryFilter<TImplementation>()
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TSearchObject>
    {
        base.AddQueryFilter<TImplementation>();
        Services.AddTransient<IFilteredQueryBuilder<TEntity, TSearchObject>, TImplementation>();

        return this;
    }
    public ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> AddQueryFilter(Func<IServiceProvider, IFilteredQueryBuilder<TEntity, TSearchObject>> factory)
    {
        base.AddQueryFilter(factory);
        Services.AddTransient(factory);

        return this;
    }

    public new ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> AddQueryBuilder<TImplementation>()
        where TImplementation : class, IQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>
    {
        base.AddQueryBuilder<TImplementation>();
        Services.AddTransient<IQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>, TImplementation>();

        return this;
    }
    public ComplexEntityServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> AddQueryBuilder(Func<IServiceProvider, IQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>> factory)
    {
        base.AddQueryBuilder(factory);
        Services.AddTransient(factory);

        return this;
    }
}
public class ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>(
    EntityServiceBuilder<TContext, TEntity, TKey> services) : EntityServiceBuilder<TContext, TEntity, TKey>(services)
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


    // Query Builders
    public new ComplexEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes> AddDefaultQueryBuilders()
    {
        Services.AddTransient<IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>,
            QueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>>();

        return this;
    }

    public new void AddQueryFilter<TImplementation>()
        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, TSearchObject>
        => Services.AddTransient<IFilteredQueryBuilder<TEntity, TKey, TSearchObject>, TImplementation>();
    public void AddQueryFilter(Func<IServiceProvider, IFilteredQueryBuilder<TEntity, TKey, TSearchObject>> factory)
        => Services.AddTransient(factory);

    public new void AddQueryBuilder<TImplementation>()
        where TImplementation : class, IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
        => Services.AddTransient<IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, TImplementation>();
    public void AddQueryBuilder(Func<IServiceProvider, IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>> factory)
        => Services.AddTransient(factory);
}
