using Regira.DAL.Paging;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.QueryBuilders.Abstractions;

public interface IQueryBuilder<TEntity> 
    : IQueryBuilder<TEntity, int>
    where TEntity : IEntity<int>;

public interface IQueryBuilder<TEntity, TKey> 
    : IQueryBuilder<TEntity, TKey, SearchObject<TKey>>
    where TEntity : IEntity<TKey>;

public interface IQueryBuilder<TEntity, TKey, TSearchObject>
    : IQueryBuilder<TEntity, TKey, TSearchObject, EntitySortBy, EntityIncludes>
    where TEntity : IEntity<TKey>
    where TSearchObject : ISearchObject<TKey>;

public interface IQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes> 
    : IQueryBuilder<TEntity, int, TSearchObject, TSortBy, TIncludes>
    where TEntity : IEntity<int>
    where TSearchObject : ISearchObject<int>
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;

public interface IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    where TEntity : IEntity<TKey>
    where TSearchObject : ISearchObject<TKey>
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    IQueryable<TEntity> Filter(IQueryable<TEntity> query, TSearchObject? so);
    IQueryable<TEntity> FilterList(IQueryable<TEntity> query, IList<TSearchObject?>? so);
    IQueryable<TEntity> SortByList(IQueryable<TEntity> query, IList<TSearchObject?>? so, IList<TSortBy>? sortByList, TIncludes? includes);
    IQueryable<TEntity> AddIncludes(IQueryable<TEntity> query, IList<TSearchObject?>? so, IList<TSortBy>? sortByList, TIncludes? includes);

    IQueryable<TEntity> Query(IQueryable<TEntity> query, IList<TSearchObject?>? searchObjects, IList<TSortBy> sortBy, TIncludes? includes, PagingInfo? pagingInfo);
}