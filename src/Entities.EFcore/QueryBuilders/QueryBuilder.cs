﻿using Microsoft.Extensions.Logging;
using Regira.DAL.Paging;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Utilities;

namespace Regira.Entities.EFcore.QueryBuilders;

public class QueryBuilder<TEntity>(
    IEnumerable<IGlobalFilteredQueryBuilder> globalFilters,
    IEnumerable<IFilteredQueryBuilder<TEntity, int, SearchObject<int>>>? filters = null,
    ISortedQueryBuilder<TEntity, int>? sortedQueryBuilder = null,
    IIncludableQueryBuilder<TEntity, int>? includableQueryBuilder = null,
    ILoggerFactory? loggerFactory = null
) : QueryBuilder<TEntity, int>(globalFilters, filters, sortedQueryBuilder, includableQueryBuilder, loggerFactory)//, IQueryBuilder<TEntity>
    where TEntity : IEntity<int>;

public class QueryBuilder<TEntity, TKey>(
    IEnumerable<IGlobalFilteredQueryBuilder> globalFilters,
    IEnumerable<IFilteredQueryBuilder<TEntity, TKey, SearchObject<TKey>>>? filters = null,
    ISortedQueryBuilder<TEntity, TKey>? sortedQueryBuilder = null,
    IIncludableQueryBuilder<TEntity, TKey>? includableQueryBuilder = null,
    ILoggerFactory? loggerFactory = null
) : QueryBuilder<TEntity, TKey, SearchObject<TKey>>(globalFilters, filters, sortedQueryBuilder, includableQueryBuilder, loggerFactory)
    where TEntity : IEntity<TKey>;

public class QueryBuilder<TEntity, TKey, TSearchObject>(
    IEnumerable<IGlobalFilteredQueryBuilder> globalFilters,
    IEnumerable<IFilteredQueryBuilder<TEntity, TKey, TSearchObject>>? filters = null,
    ISortedQueryBuilder<TEntity, TKey>? sortedQueryBuilder = null,
    IIncludableQueryBuilder<TEntity, TKey>? includableQueryBuilder = null,
    ILoggerFactory? loggerFactory = null
)
    : QueryBuilder<TEntity, TKey, TSearchObject, EntitySortBy, EntityIncludes>(globalFilters, filters, sortedQueryBuilder, includableQueryBuilder, loggerFactory)
    where TEntity : IEntity<TKey>
    where TSearchObject : ISearchObject<TKey>
{
    public IQueryable<TEntity> Query(IQueryable<TEntity> query, IList<TSearchObject?>? searchObjects, PagingInfo? pagingInfo)
        => Query(query, searchObjects, [], null, pagingInfo);
}

public class QueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes>(
    IEnumerable<IGlobalFilteredQueryBuilder> globalFilters,
    IEnumerable<IFilteredQueryBuilder<TEntity, int, TSearchObject>>? filters = null,
    ISortedQueryBuilder<TEntity, int>? sortedQueryBuilder = null,
    IIncludableQueryBuilder<TEntity, int>? includableQueryBuilder = null,
    ILoggerFactory? loggerFactory = null)
    : QueryBuilder<TEntity, int, TSearchObject, TSortBy, TIncludes>(globalFilters, filters, sortedQueryBuilder, includableQueryBuilder, loggerFactory)
    where TEntity : IEntity<int>
    where TSearchObject : ISearchObject<int>
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;


public class QueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>(
        IEnumerable<IGlobalFilteredQueryBuilder> globalFilters,
        IEnumerable<IFilteredQueryBuilder<TEntity, TKey, TSearchObject>>? filters = null,
        ISortedQueryBuilder<TEntity, TKey>? sortedQueryBuilder = null,
        IIncludableQueryBuilder<TEntity, TKey>? includableQueryBuilder = null,
        ILoggerFactory? loggerFactory = null
    )
    : IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    where TEntity : IEntity<TKey>
    where TSearchObject : ISearchObject<TKey>
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    readonly ILogger? _logger = loggerFactory?.CreateLogger<QueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>>();

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
                        _logger?.LogDebug($"Applying filter {filter.GetType().FullName} for type {typeof(TEntity).FullName}");
                        return filter.Build(filteredQuery, so);
                    }

                    return filteredQuery;
                }
            );
        return filters
            ?.Aggregate(
                globallyFilteredQuery,
                (filteredQuery, filter) =>
                {
                    _logger?.LogDebug($"Applying global filter {filter.GetType().FullName} for type {typeof(TEntity).FullName}");
                    return filter.Build(filteredQuery, so);
                }) ?? globallyFilteredQuery;
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
        => sortedQueryBuilder?.SortBy(query) ?? query;
    public virtual IQueryable<TEntity> SortByList(IQueryable<TEntity> query, IList<TSearchObject?>? so, IList<TSortBy>? sortByList, TIncludes? includes)
    {
        if (sortByList?.Any() != true)
        {
            return SortBy(query, so, null, includes);
        }
        return sortByList.Aggregate(query, (current, by) => SortBy(current, so, by, includes));
    }

    public virtual IQueryable<TEntity> AddIncludes(IQueryable<TEntity> query, IList<TSearchObject?>? so, IList<TSortBy>? sortByList, TIncludes? includes)
        => includableQueryBuilder?.AddIncludes(query) ?? query;
}