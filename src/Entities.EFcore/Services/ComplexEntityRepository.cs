using Microsoft.EntityFrameworkCore;
using Regira.DAL.Paging;
using Regira.Entities.Abstractions;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Extensions;
using Regira.Entities.Models.Abstractions;
using Regira.Utilities;

namespace Regira.Entities.EFcore.Services;

public class EntityRepository<TContext, TEntity, TSearchObject, TSortBy, TIncludes>
    (TContext dbContext, IQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes> queryBuilder)
    : EntityRepository<TContext, TEntity, int, TSearchObject, TSortBy, TIncludes>(dbContext, queryBuilder),
        IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>
    where TContext : DbContext
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;
public class EntityRepository<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    (TContext dbContext, IQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes> queryBuilder)
    : IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    public virtual DbSet<TEntity> DbSet => dbContext.Set<TEntity>();

    Task<IList<TEntity>> IEntityReadService<TEntity, TKey>.List(object? so, PagingInfo? pagingInfo)
        => List(Convert(so), pagingInfo);
    Task<int> IEntityReadService<TEntity, TKey>.Count(object? so)
        => Count(Convert(so));

    public virtual async Task<TEntity?> Details(TKey id)
    {
        if (id?.Equals(default(TKey)) == true)
        {
            return null;
        }

        var query = Query(
            DbSet,
            [Convert(new { Id = id })],
            [],
            Enum.GetValues(typeof(TIncludes)).Cast<TIncludes>().Last(),
            null
        );
        return await query
#if NETSTANDARD2_0
            .AsNoTracking()
#else
            .AsNoTrackingWithIdentityResolution()
#endif
            .SingleOrDefaultAsync();
    }

    public virtual async Task<IList<TEntity>> List(IList<TSearchObject?> searchObjects, IList<TSortBy> sortBy, TIncludes? includes = null, PagingInfo? pagingInfo = null)
        => await Query(DbSet, searchObjects, sortBy, includes, pagingInfo)
#if NETSTANDARD2_0
            .AsNoTracking()
#else
                .AsNoTrackingWithIdentityResolution()
#endif
            .ToListAsync();
    public virtual async Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null)
        => await Query(DbSet, [so!], Array.Empty<TSortBy>(), null, pagingInfo)
#if NETSTANDARD2_0
            .AsNoTracking()
#else
                .AsNoTrackingWithIdentityResolution()
#endif
            .ToListAsync();

    public virtual Task<int> Count(IList<TSearchObject?> searchObjects)
        => queryBuilder.Filter(dbContext.Set<TEntity>(), searchObjects).CountAsync();
    public virtual Task<int> Count(TSearchObject? so = null)
        => queryBuilder.Filter(DbSet, so).CountAsync();

    public virtual IQueryable<TEntity> Query(IQueryable<TEntity> query, IList<TSearchObject?> searchObjects, IList<TSortBy> sortBy, TIncludes? includes, PagingInfo? pagingInfo)
        => queryBuilder.Query(query, searchObjects, sortBy, includes, pagingInfo);


    public virtual Task Add(TEntity item)
    {
        PrepareItem(item);
        DbSet.Add(item);
        return Task.FromResult(true);
    }
    public virtual async Task Modify(TEntity item)
    {
        PrepareItem(item);

        var original = await Details(item.Id);
        if (original == null)
        {
            return;
        }

        Modify(item, original);

        dbContext.Update(original);
        dbContext.Entry(original).CurrentValues.SetValues(item);

        //dbContext.Attach(original);
        //dbContext.Entry(original).CurrentValues.SetValues(item);
        //dbContext.Entry(original).State = EntityState.Modified;
    }
    public virtual Task Save(TEntity item)
        => IsNew(item) ? Add(item) : Modify(item);
    public virtual Task Remove(TEntity item)
    {
        DbSet.Remove(item);
        return Task.CompletedTask;
    }

    public virtual void PrepareItem(TEntity item)
    {
    }
    public virtual void Modify(TEntity item, TEntity original)
    {
    }

    public virtual Task<int> SaveChanges(CancellationToken token = default)
        => dbContext.SaveChangesAsync(token);

    protected TSearchObject? Convert(object? so)
        => so == null ? null
            : so is TSearchObject tso ? tso
            : ObjectUtility.Create<TSearchObject>(so);
    protected bool IsNew(TEntity item)
        => item.IsNew();
}