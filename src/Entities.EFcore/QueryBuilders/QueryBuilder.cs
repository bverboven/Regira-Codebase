using Regira.DAL.Paging;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.QueryBuilders;

public class QueryBuilder<TEntity, TSearchObject>(
    IEnumerable<IFilteredQueryBuilder<TEntity, TSearchObject>>? filters = null
) : QueryBuilder<TEntity, int, TSearchObject>(filters), IQueryBuilder<TEntity, TSearchObject>
    where TEntity : IEntity<int>
    where TSearchObject : ISearchObject;

public class QueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>(
    IEnumerable<IFilteredQueryBuilder<TEntity, TSearchObject>>? filters = null
)
    : QueryBuilder<TEntity, int, TSearchObject, TSortBy, TIncludes>(filters), IQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>
    where TEntity : IEntity<int>
    where TSearchObject : ISearchObject<int>
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;

public class QueryBuilder<TEntity, TKey, TSearchObject>(
    IEnumerable<IFilteredQueryBuilder<TEntity, TKey, TSearchObject>>? filters = null
)
    : QueryBuilder<TEntity, TKey, TSearchObject, EntitySortBy, EntityIncludes>(filters), IQueryBuilder<TEntity, TKey, TSearchObject>
    where TEntity : IEntity<TKey>
    where TSearchObject : ISearchObject<TKey>
{
    public IQueryable<TEntity> Query(IQueryable<TEntity> query, IList<TSearchObject?>? searchObjects, PagingInfo? pagingInfo)
        => Query(query, searchObjects, [EntitySortBy.Default], EntityIncludes.None, pagingInfo);
}

public class QueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>(
        IEnumerable<IFilteredQueryBuilder<TEntity, TKey, TSearchObject>>? filters = null
    )
    : IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    where TEntity : IEntity<TKey>
    where TSearchObject : ISearchObject<TKey>
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    public virtual IQueryable<TEntity> Filter(IQueryable<TEntity> query, TSearchObject? so)
        => filters
            ?.Aggregate(
                query,
                (r, filter) => filter.Build(r, so)
            ) ?? query;
    public virtual IQueryable<TEntity> Filter(IQueryable<TEntity> query, IList<TSearchObject?>? searchObjects)
        => searchObjects
            ?.Aggregate(
                (IQueryable<TEntity>?)null,
                (r, so) => r == null
                    ? Filter(query, so)
                    : r.Union(Filter(query, so))
            ) ?? query;

    public virtual IQueryable<TEntity> SortBy(IQueryable<TEntity> query, IList<TSearchObject?>? so, TSortBy? sortBy, TIncludes? includes)
        => query;
    public virtual IQueryable<TEntity> SortBy(IQueryable<TEntity> query, IList<TSearchObject?>? so, IList<TSortBy>? sortByList, TIncludes? includes)
        => sortByList?.Aggregate(query, (r, sortBy) => SortBy(r, so, sortBy, includes)) ?? query;

    public virtual IQueryable<TEntity> AddIncludes(IQueryable<TEntity> query, IList<TSearchObject?>? so, IList<TSortBy>? sortByList, TIncludes? includes)
    {
        return query;
    }

    public virtual IQueryable<TEntity> Query(IQueryable<TEntity> query, IList<TSearchObject?>? searchObjects, IList<TSortBy> sortBy, TIncludes? includes, PagingInfo? pagingInfo)
    {
        var filteredQuery = Filter(query, searchObjects);
        var sortedQuery = SortBy(filteredQuery, searchObjects, sortBy, includes);
        var pagedQuery = sortedQuery.PageQuery(pagingInfo);
        var includingQuery = AddIncludes(pagedQuery, searchObjects, sortBy, includes);

        return includingQuery;
    }
}