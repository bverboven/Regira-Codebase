using Regira.DAL.Paging;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Utilities;

namespace Regira.Entities.EFcore.QueryBuilders;

public class QueryBuilder<TEntity>(
    IEnumerable<IGlobalFilteredQueryBuilder> globalFilters,
    IEnumerable<IFilteredQueryBuilder<TEntity, SearchObject<int>>>? filters = null
) : QueryBuilder<TEntity, int>(globalFilters, filters), IQueryBuilder<TEntity>
    where TEntity : IEntity<int>;

public class QueryBuilder<TEntity, TKey>(
    IEnumerable<IGlobalFilteredQueryBuilder> globalFilters,
    IEnumerable<IFilteredQueryBuilder<TEntity, TKey, SearchObject<TKey>>>? filters = null
) : QueryBuilder<TEntity, TKey, SearchObject<TKey>>(globalFilters, filters), IQueryBuilder<TEntity, TKey>
    where TEntity : IEntity<TKey>;

public class QueryBuilder<TEntity, TKey, TSearchObject>(
    IEnumerable<IGlobalFilteredQueryBuilder> globalFilters,
    IEnumerable<IFilteredQueryBuilder<TEntity, TKey, TSearchObject>>? filters = null
)
    : QueryBuilder<TEntity, TKey, TSearchObject, EntitySortBy, EntityIncludes>(globalFilters, filters),
        IQueryBuilder<TEntity, TKey, TSearchObject>
    where TEntity : IEntity<TKey>
    where TSearchObject : ISearchObject<TKey>
{
    public IQueryable<TEntity> Query(IQueryable<TEntity> query, IList<TSearchObject?>? searchObjects, PagingInfo? pagingInfo)
        => Query(query, searchObjects, [], null, pagingInfo);
}

public class QueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>(
    IEnumerable<IGlobalFilteredQueryBuilder> globalFilters,
    IEnumerable<IFilteredQueryBuilder<TEntity, TSearchObject>>? filters = null
)
    : QueryBuilder<TEntity, int, TSearchObject, TSortBy, TIncludes>(globalFilters, filters),
        IQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>
    where TEntity : IEntity<int>
    where TSearchObject : ISearchObject<int>
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;


public class QueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>(
        IEnumerable<IGlobalFilteredQueryBuilder> globalFilters,
        IEnumerable<IFilteredQueryBuilder<TEntity, TKey, TSearchObject>>? filters = null
    )
    : IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    where TEntity : IEntity<TKey>
    where TSearchObject : ISearchObject<TKey>
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    public virtual IQueryable<TEntity> Query(IQueryable<TEntity> query, IList<TSearchObject?>? searchObjects, IList<TSortBy> sortBy, TIncludes? includes, PagingInfo? pagingInfo)
    {
        var filteredQuery = FilterList(query, searchObjects);
        var sortedQuery = SortByList(filteredQuery, searchObjects, sortBy, includes);
        var pagedQuery = sortedQuery.PageQuery(pagingInfo);
        var includingQuery = AddIncludes(pagedQuery, searchObjects, sortBy, includes);

        return includingQuery;
    }

    public virtual IQueryable<TEntity> Filter(IQueryable<TEntity> query, TSearchObject? so)
    {
        var entityBaseTypes = TypeUtility.GetBaseTypes(typeof(TEntity)).ToArray();

        var globallyFilteredQuery = globalFilters
            .Aggregate(
                query,
                (filteredQuery, filter) =>
                {
                    var filterTypes = TypeUtility.GetBaseTypes(filter.GetType()).Concat([filter.GetType()]).Distinct();
                    var canFilter = filterTypes.Any(ft => entityBaseTypes.Any(bt => TypeUtility.HasGenericArgument(ft, bt)));
                    if (canFilter)
                    {
                        return filter.Build(filteredQuery, so);
                    }

                    return filteredQuery;
                }
            );
        return filters
            ?.Aggregate(
                globallyFilteredQuery,
                (filteredQuery, filter) => filter.Build(filteredQuery, so)
            ) ?? globallyFilteredQuery;
    }
    public virtual IQueryable<TEntity> FilterList(IQueryable<TEntity> query, IList<TSearchObject?>? searchObjects)
        => searchObjects
            ?.Aggregate(
                (IQueryable<TEntity>?)null,
                (r, so) => r == null
                    ? Filter(query, so)
                    : r.Union(Filter(query, so))
            ) ?? query;

    public virtual IQueryable<TEntity> SortBy(IQueryable<TEntity> query, IList<TSearchObject?>? so, TSortBy? sortBy, TIncludes? includes)
        => query;
    public virtual IQueryable<TEntity> SortByList(IQueryable<TEntity> query, IList<TSearchObject?>? so, IList<TSortBy>? sortByList, TIncludes? includes)
    {
        if (sortByList?.Any() != true)
        {
            return SortBy(query, so, null, includes);
        }
        return sortByList.Aggregate(query, (current, by) => SortBy(current, so, by, includes));
    }

    public virtual IQueryable<TEntity> AddIncludes(IQueryable<TEntity> query, IList<TSearchObject?>? so, IList<TSortBy>? sortByList, TIncludes? includes)
    {
        return query;
    }
}