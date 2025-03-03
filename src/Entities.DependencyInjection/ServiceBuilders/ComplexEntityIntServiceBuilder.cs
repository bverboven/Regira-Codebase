using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Abstractions;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Entities.EFcore.Normalizing.Abstractions;
using Regira.Entities.EFcore.QueryBuilders;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models.Abstractions;
using System.Linq.Expressions;

namespace Regira.Entities.DependencyInjection.ServiceBuilders;

public partial class ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes>
{
    // Entity service
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> AddDefaultService()
        => UseEntityService<EntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>>();

    /// <summary>
    /// Adds <typeparamref name="TService"/> implementation for
    /// <list type="bullet">
    ///     <item><see cref="IEntityService{TEntity, TKey, TSearchObject, TSortBy, TIncludes}"/></item>
    /// </list>
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> UseEntityService<TService>()
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
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> UseEntityService<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>
    {
        base.UseEntityService(factory);
        Services.AddTransient<IEntityService<TEntity>>(factory);
        Services.AddTransient<IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>>(factory);
        return this;
    }

    // Entity Repository
    protected internal new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> HasRepositoryInner<TService>()
        where TService : class, IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>, IEntityRepository<TEntity>
    {
        base.HasRepositoryInner<TService>();
        Services.AddTransient<IEntityRepository<TEntity>, TService>();
        Services.AddTransient<IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>, TService>();
        return this;
    }
    protected internal new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> HasRepositoryInner<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>, IEntityRepository<TEntity>
    {
        base.HasRepository(factory);
        Services.AddTransient<IEntityRepository<TEntity>>(factory);
        Services.AddTransient<IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>>(factory);
        return this;
    }
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> HasRepository<TService>()
        where TService : class, IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>, IEntityRepository<TEntity>
    {
        UseEntityService<TService>();
        HasRepositoryInner<TService>();
        return this;
    }
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> HasRepository<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>, IEntityRepository<TEntity>
    {
        UseEntityService(factory);
        HasRepositoryInner(factory);
        return this;
    }

    // Entity Manager
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> HasManager<TService>()
        where TService : class, IEntityManager<TEntity, TSearchObject, TSortBy, TIncludes>, IEntityManager<TEntity>
    {
        base.HasManager<TService>();
        Services.AddTransient<IEntityManager<TEntity>, TService>();
        Services.AddTransient<IEntityManager<TEntity, TSearchObject, TSortBy, TIncludes>, TService>();
        return this;
    }
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> HasManager<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IEntityManager<TEntity, TSearchObject, TSortBy, TIncludes>, IEntityManager<TEntity>
    {
        base.HasManager(factory);
        Services.AddTransient<IEntityManager<TEntity>>(factory);
        Services.AddTransient<IEntityManager<TEntity, TSearchObject, TSortBy, TIncludes>>(factory);
        return this;
    }

    // EntityNormalizers
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> AddNormalizer<TNormalizer>()
        where TNormalizer : class, IEntityNormalizer<TEntity>
    {
        base.AddNormalizer<TNormalizer>();
        return this;
    }

    // Read Service
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> UseReadService<TService>()
        where TService : class, IEntityReadService<TEntity, int, TSearchObject, TSortBy, TIncludes>
    {
        base.UseReadService<TService>();
        return this;
    }

    // Query Builders
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> AddDefaultQueryBuilder()
    {
        Services.AddDefaultQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>();
        Services.UseQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes, QueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>>();
        return this;
    }
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> UseQueryBuilder<TImplementation>()
        where TImplementation : class, IQueryBuilder<TEntity, int, TSearchObject, TSortBy, TIncludes>
    {
        base.UseQueryBuilder<TImplementation>();
        return this;
    }
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> UseQueryBuilder<TQueryBuilder>(Func<IServiceProvider, TQueryBuilder> factory)
        where TQueryBuilder : class, IQueryBuilder<TEntity, int, TSearchObject, TSortBy, TIncludes>
    {
        base.UseQueryBuilder(factory);
        return this;
    }

    // Query Filters
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> AddQueryFilter<TImplementation>()
        where TImplementation : class, IFilteredQueryBuilder<TEntity, int, TSearchObject>
    {
        base.AddQueryFilter<TImplementation>();
        return this;
    }
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> AddQueryFilter<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IFilteredQueryBuilder<TEntity, int, TSearchObject>
    {
        base.AddQueryFilter(factory);
        return this;
    }
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> Filter(Func<IQueryable<TEntity>, TSearchObject?, IQueryable<TEntity>> filterFunc)
    {
        AddQueryFilter(_ => new EntityQueryFilter<TEntity, int, TSearchObject>(filterFunc));
        return this;
    }

    // SortBy
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> SortBy<TImplementation>()
        where TImplementation : class, ISortedQueryBuilder<TEntity, int, TSortBy>
    {
        base.SortBy<TImplementation>();
        return this;
    }
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> SortBy<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, ISortedQueryBuilder<TEntity, int, TSortBy>
    {
        base.SortBy(factory);
        return this;
    }
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> SortBy(Func<IQueryable<TEntity>, TSortBy?, IQueryable<TEntity>> sortByFunc)
    {
        base.SortBy(sortByFunc);
        return this;
    }

    // Includes
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> Includes<TImplementation>()
        where TImplementation : class, IIncludableQueryBuilder<TEntity, int, TIncludes>
    {
        base.Includes<TImplementation>();
        return this;
    }
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> Includes<TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class, IIncludableQueryBuilder<TEntity, int, TIncludes>
    {
        base.Includes(factory);
        return this;
    }
    public new ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> Includes(Func<IQueryable<TEntity>, TIncludes?, IQueryable<TEntity>> addIncludes)
    {
        base.Includes(addIncludes);
        return this;
    }

    // Related
    public ComplexEntityIntServiceBuilder<TContext, TEntity, TSearchObject, TSortBy, TIncludes> Related<TRelated>(
        Expression<Func<TEntity, ICollection<TRelated>?>> navigationExpression, Action<TEntity>? prepareFunc = null)
        where TRelated : class, IEntity<int>
    {
        Related<TRelated, int>(navigationExpression, prepareFunc);

        return this;
    }
}