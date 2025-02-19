using Microsoft.EntityFrameworkCore;
using Regira.DAL.Paging;
using Regira.Entities.Abstractions;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Utilities;

namespace Regira.Entities.EFcore.Services;

public class EntityReadService<TContext, TEntity>(TContext dbContext, IQueryBuilder<TEntity, int, SearchObject<int>, EntitySortBy, EntityIncludes> queryBuilder)
    : EntityReadService<TContext, TEntity, int>(dbContext, queryBuilder), IEntityReadService<TEntity>
    where TContext : DbContext
    where TEntity : class, IEntity<int>;

public class EntityReadService<TContext, TEntity, TKey>(TContext dbContext, IQueryBuilder<TEntity, TKey, SearchObject<TKey>, EntitySortBy, EntityIncludes> queryBuilder)
    : EntityReadService<TContext, TEntity, TKey, SearchObject<TKey>>(dbContext, queryBuilder)
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>;

public class EntityReadService<TContext, TEntity, TKey, TSearchObject>(TContext dbContext, IQueryBuilder<TEntity, TKey, TSearchObject, EntitySortBy, EntityIncludes> queryBuilder)
    : EntityReadService<TContext, TEntity, TKey, TSearchObject, EntitySortBy, EntityIncludes>(dbContext, queryBuilder)
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new();

public class EntityReadService<TContext, TEntity, TSearchObject, TSortBy, TIncludes>(TContext dbContext, IQueryBuilder<TEntity, int, TSearchObject, TSortBy, TIncludes> queryBuilder)
    : EntityReadService<TContext, TEntity, int, TSearchObject, TSortBy, TIncludes>(dbContext, queryBuilder), IEntityReadService<TEntity, TSearchObject, TSortBy, TIncludes>
    where TContext : DbContext
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;

public class EntityReadService<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>(TContext dbContext, IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes> queryBuilder)
    : IEntityReadService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    public virtual DbSet<TEntity> DbSet => dbContext.Set<TEntity>();
    // Details
    public virtual async Task<TEntity?> Details(TKey id)
    {
        if (id?.Equals(default(TKey)) == true)
        {
            return null;
        }

        return await queryBuilder.Query(DbSet, [Convert(new { Id = id })], [], Enum.GetValues(typeof(TIncludes)).Cast<TIncludes>().Last(), null)
#if NETSTANDARD2_0
            .AsNoTracking()
#else
            .AsNoTrackingWithIdentityResolution()
#endif
            .SingleOrDefaultAsync();
    }

    // List
    public virtual async Task<IList<TEntity>> List(IList<TSearchObject?> searchObjects, IList<TSortBy> sortBy, TIncludes? includes = null, PagingInfo? pagingInfo = null)
        => await queryBuilder.Query(DbSet, searchObjects, sortBy, includes, pagingInfo)
#if NETSTANDARD2_0
            .AsNoTracking()
#else
            .AsNoTrackingWithIdentityResolution()
#endif
            .ToListAsync();
    public virtual async Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null)
        => await List([so], [], null, pagingInfo);
    Task<IList<TEntity>> IEntityReadService<TEntity, TKey>.List(object? so, PagingInfo? pagingInfo)
        => List(Convert(so), pagingInfo);

    // Count
    public virtual Task<int> Count(IList<TSearchObject?> searchObjects)
        => queryBuilder.FilterList(dbContext.Set<TEntity>(), searchObjects).CountAsync();
    public virtual Task<int> Count(TSearchObject? so = null)
        => queryBuilder.Filter(DbSet, so).CountAsync();
    Task<int> IEntityReadService<TEntity, TKey>.Count(object? so)
        => Count(Convert(so));

    // Helpers
    public virtual TSearchObject? Convert(object? so)
        => so != null
            ? so as TSearchObject ?? ObjectUtility.Create<TSearchObject>(so)
            : null;
}
