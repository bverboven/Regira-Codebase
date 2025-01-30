using Microsoft.EntityFrameworkCore;
using Regira.DAL.Paging;
using Regira.Entities.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Extensions;
using Regira.Entities.Models.Abstractions;
using Regira.Utilities;

namespace Regira.Entities.EFcore.Abstractions;

public abstract class EntityRepositoryBase<TContext, TEntity, TSearchObject, TSortBy, TIncludes>(TContext dbContext)
    : EntityRepositoryBase<TContext, TEntity, int, TSearchObject, TSortBy, TIncludes>(dbContext),
        IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>
    where TContext : DbContext
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;
public abstract class EntityRepositoryBase<TContext, TEntity, TKey, TSearchObject, TSortBy, TIncludes>(
    TContext dbContext) : IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    public virtual DbSet<TEntity> DbSet => DbContext.Set<TEntity>();

    protected readonly TContext DbContext = dbContext;


    Task<IList<TEntity>> IEntityReadService<TEntity, TKey>.List(object? so, PagingInfo? pagingInfo)
        => List(Convert(so), pagingInfo);
    Task<int> IEntityReadService<TEntity, TKey>.Count(object? so)
        => Count(Convert(so));

    public virtual async Task<TEntity?> Details(TKey id)
        => await AddIncludes(
                DbSet.Where(x => x.Id!.Equals(id)),
                // Get TIncludes.All
                Enum.GetValues(typeof(TIncludes)).Cast<TIncludes>().Last()
            )
#if NETSTANDARD2_0
            .AsNoTracking()
#else
                .AsNoTrackingWithIdentityResolution()
#endif
            .SingleOrDefaultAsync();

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
        => Filter(DbContext.Set<TEntity>(), searchObjects).CountAsync();
    public virtual Task<int> Count(TSearchObject? so = null)
        => Filter(DbSet, so).CountAsync();


    public virtual IQueryable<TEntity> Filter(IQueryable<TEntity> query, TSearchObject? so)
        => query.Filter(Convert(so));
    public virtual IQueryable<TEntity> Filter(IQueryable<TEntity> query, IList<TSearchObject?> searchObjects)
        => searchObjects.Aggregate((IQueryable<TEntity>?)null, (r, so) => r == null ? Filter(query, so) : r.Union(Filter(query, so))) ?? query;

    public virtual IQueryable<TEntity> SortBy(IQueryable<TEntity> query, IList<TSortBy> sortings)
        => sortings.Any() ? sortings.Aggregate(query, (current, sortBy) => SortBy(current, sortBy)) : SortBy(query);
    public virtual IQueryable<TEntity> SortBy(IQueryable<TEntity> query, TSortBy? sortBy = null)
        => query;

    public virtual IQueryable<TEntity> AddIncludes(IQueryable<TEntity> query, TIncludes? includes)
        => query;
    public virtual IQueryable<TEntity> Query(IQueryable<TEntity> query, IList<TSearchObject?> searchObjects, IList<TSortBy> sortBy, TIncludes? includes, PagingInfo? pagingInfo)
    {
        var filteredQuery = Filter(query, searchObjects);
        var sortedQuery = SortBy(filteredQuery, sortBy);
        var pagedQuery = sortedQuery.PageQuery(pagingInfo);
        var includingQuery = AddIncludes(pagedQuery, includes);

        return includingQuery;
    }


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

        DbContext.Update(original);
        DbContext.Entry(original).CurrentValues.SetValues(item);

        //DbContext.Attach(original);
        //DbContext.Entry(original).CurrentValues.SetValues(item);
        //DbContext.Entry(original).State = EntityState.Modified;
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
        => DbContext.SaveChangesAsync(token);

    protected TSearchObject? Convert(object? so)
        => so == null ? null
            : so is TSearchObject tso ? tso
            : ObjectUtility.Create<TSearchObject>(so);
    protected bool IsNew(TEntity item)
        => item.IsNew();
}