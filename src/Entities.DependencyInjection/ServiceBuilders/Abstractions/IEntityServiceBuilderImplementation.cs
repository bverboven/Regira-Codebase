//using Microsoft.EntityFrameworkCore;
//using Regira.Entities.Abstractions;
//using Regira.Entities.EFcore.Normalizing.Abstractions;
//using Regira.Entities.EFcore.Preppers.Abstractions;
//using Regira.Entities.EFcore.Primers.Abstractions;
//using Regira.Entities.EFcore.Processing.Abstractions;
//using Regira.Entities.EFcore.QueryBuilders.Abstractions;
//using Regira.Entities.Models;
//using Regira.Entities.Models.Abstractions;
//using System.Linq.Expressions;

//namespace Regira.Entities.DependencyInjection.ServiceBuilders.Abstractions;

//public interface IEntityServiceBuilderImplementation<out TServiceBuilder, out TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>
//    where TContext : DbContext
//    where TEntity : class, IEntity<TKey>
//    where TSearchObject : class, ISearchObject<TKey>, new()
//    where TSortBy : struct, Enum
//    where TIncludes : struct, Enum
//    where TServiceBuilder : IEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject>
//{
//    TServiceBuilder AddDefaultService();
//    TServiceBuilder UseEntityService<TService>()
//        where TService : class, IEntityService<TEntity, TKey>, IEntityService<TEntity, TKey, SearchObject<TKey>>;
//    TServiceBuilder UseEntityService<TService>(Func<IServiceProvider, TService> factory)
//        where TService : class, IEntityService<TEntity, TKey, SearchObject<TKey>>;

//    TServiceBuilder UseReadService<TService>()
//        where TService : class, IEntityReadService<TEntity, TKey, SearchObject<TKey>>;

//    TServiceBuilder UseWriteService<TService>()
//        where TService : class, IEntityWriteService<TEntity, TKey>;

//    TServiceBuilder HasRepository<TService>()
//        where TService : class, IEntityRepository<TEntity, TKey, SearchObject<TKey>>;
//    TServiceBuilder HasRepository<TImplementation>(Func<IServiceProvider, TImplementation> factory)
//        where TImplementation : class, IEntityRepository<TEntity, TKey>, IEntityRepository<TEntity, TKey, SearchObject<TKey>>;

//    TServiceBuilder HasManager<TService>()
//        where TService : class, IEntityManager<TEntity, TKey>, IEntityManager<TEntity, TKey, SearchObject<TKey>>;
//    TServiceBuilder HasManager<TImplementation>(Func<IServiceProvider, TImplementation> factory)
//        where TImplementation : class, IEntityManager<TEntity, TKey>, IEntityManager<TEntity, TKey, SearchObject<TKey>>;

//    TServiceBuilder AddNormalizer<TNormalizer>()
//        where TNormalizer : class, IEntityNormalizer<TEntity>;

//    TServiceBuilder SortBy(Func<IQueryable<TEntity>, IQueryable<TEntity>> sortBy);
//    TServiceBuilder Includes(Func<IQueryable<TEntity>, EntityIncludes?, IQueryable<TEntity>> addIncludes);

//    TServiceBuilder AddPrimer<TPrimer>()
//        where TPrimer : class, IEntityPrimer<TEntity>;
//    TServiceBuilder Process(Func<IList<TEntity>, EntityIncludes?, Task> process);
//    TServiceBuilder Process(Action<TEntity, EntityIncludes?> process);
//    TServiceBuilder Process<TImplementation>()
//        where TImplementation : class, IEntityProcessor<TEntity, EntityIncludes>;

//    TServiceBuilder AddPrepper<TImplementation>()
//        where TImplementation : class, IEntityPrepper<TEntity, TKey>;
//    TServiceBuilder AddPrepper<TImplementation>(Func<IServiceProvider, TImplementation> factory)
//        where TImplementation : class, IEntityPrepper<TEntity, TKey>;
//    TServiceBuilder Prepare(Action<TEntity> prepareFunc);
//    TServiceBuilder Prepare(Func<TEntity, TContext, Task> prepareFunc);
//    TServiceBuilder Related<TRelated, TRelatedKey>(
//        Expression<Func<TEntity, ICollection<TRelated>?>> navigationExpression, Action<TEntity>? prepareFunc = null)
//        where TRelated : class, IEntity<TRelatedKey>;
    
//    TServiceBuilder AddQueryFilter<TImplementation>()
//        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, TSearchObject>;
//    TServiceBuilder AddQueryFilter<TImplementation>(Func<IServiceProvider, TImplementation> factory)
//        where TImplementation : class, IFilteredQueryBuilder<TEntity, TKey, TSearchObject>;
//    TServiceBuilder Filter(Func<IQueryable<TEntity>, TSearchObject?, IQueryable<TEntity>> filterFunc);

//    TServiceBuilder AddDefaultQueryBuilder();
//    TServiceBuilder UseQueryBuilder<TImplementation>()
//        where TImplementation : class, IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>;
//    TServiceBuilder UseQueryBuilder<TImplementation>(Func<IServiceProvider, TImplementation> factory)
//        where TImplementation : class, IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>;
//}
//public interface IEntityServiceBuilderImplementation<out TServiceBuilder, out TContext, TEntity, TKey, TSearchObject> : IEntityServiceBuilderImplementation<TServiceBuilder, TContext, TEntity, TKey, TSearchObject, EntitySortBy, EntityIncludes>
//    where TContext : DbContext
//    where TEntity : class, IEntity<TKey>
//    where TSearchObject : class, ISearchObject<TKey>, new()
//    where TServiceBuilder : IEntityServiceBuilder<TContext, TEntity, TKey, TSearchObject>
//{
//}
//public interface IEntityServiceBuilderImplementation<out TServiceBuilder, out TContext, TEntity, TKey> : IEntityServiceBuilderImplementation<TServiceBuilder, TContext, TEntity, TKey, SearchObject<TKey>>
//    where TContext : DbContext
//    where TEntity : class, IEntity<TKey>
//    where TServiceBuilder : IEntityServiceBuilder<TContext, TEntity, TKey>
//{
//}