using Microsoft.EntityFrameworkCore;
using Regira.DAL.Paging;
using Regira.Entities.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Utilities;

namespace Regira.Entities.EFcore.Services;

public class EntityRepository<TContext, TEntity> : EntityRepository<TContext, TEntity, int>, IEntityRepository<TEntity>
    where TContext : DbContext
    where TEntity : class, IEntity<int>
{
    public EntityRepository(TContext dbContext)
        : base(dbContext)
    {
    }
}
public abstract class EntityRepository<TContext, TEntity, TKey> : EntityRepository<TContext, TEntity, TKey, SearchObject<TKey>>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
{
    protected EntityRepository(TContext dbContext)
        : base(dbContext)
    {
    }
}
public class EntityRepository<TContext, TEntity, TKey, TSearchObject> : IEntityService<TEntity, TKey>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
{
    public virtual DbSet<TEntity> DbSet => DbContext.Set<TEntity>();
    public virtual IQueryable<TEntity> Query => DbSet;

    protected readonly TContext DbContext;
    public EntityRepository(TContext dbContext)
    {
        DbContext = dbContext;
    }


    public virtual async Task<TEntity?> Details(TKey id)
        => default(TKey)?.Equals(id) == false // make sure an id is passed or return null
            ? (await List(new TSearchObject { Id = id }, new PagingInfo { PageSize = 1 })).SingleOrDefault()
            : null;
    public virtual async Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null)
    {
        var query = Filter(Query, so);
        query = Sort(query);
        query = query.PageQuery(pagingInfo);
        return await query
#if NETSTANDARD2_0
            .AsNoTracking()
#else
            .AsNoTrackingWithIdentityResolution()
#endif
            .ToListAsync();
    }

    public virtual Task<int> Count(TSearchObject? so)
        => Filter(Query, so)
            .CountAsync();

    Task<IList<TEntity>> IEntityReadService<TEntity, TKey>.List(object? so, PagingInfo? pagingInfo)
        => List(Convert(so), pagingInfo);
    Task<int> IEntityReadService<TEntity, TKey>.Count(object? so)
        => Count(Convert(so));

    public virtual Task Add(TEntity item)
    {
        PrepareItem(item);

        DbSet.Add(item);
        return Task.CompletedTask;
    }
    public virtual async Task Modify(TEntity item)
    {
        PrepareItem(item);

        var original = await Details(item.Id);
        if (original == null)
        {
            return;
        }

        DbContext.Attach(original);
        DbContext.Entry(original).CurrentValues.SetValues(item);
        DbContext.Entry(original).State = EntityState.Modified;

        Modify(item, original);
    }
    public virtual Task Save(TEntity item)
        => IsNew(item) ? Add(item) : Modify(item);
    public virtual Task Remove(TEntity item)
    {
        DbSet.Remove(item);
        return Task.CompletedTask;
    }

    public virtual Task<int> SaveChanges(CancellationToken token = default)
        => DbContext.SaveChangesAsync(token);


    public virtual IQueryable<TEntity> Filter(IQueryable<TEntity> query, TSearchObject? so)
        => query.Filter(so);
    public virtual IQueryable<TEntity> Sort(IQueryable<TEntity> query)
        => query.SortQuery<TEntity, TKey>();

    public virtual void PrepareItem(TEntity item)
    {
    }
    public virtual void Modify(TEntity item, TEntity original)
    {
    }

    protected virtual TSearchObject? Convert(object? so)
        => so != default
            ? so as TSearchObject ?? ObjectUtility.Create<TSearchObject>(so)
            : default;
    protected bool IsNew(TEntity item)
        => default(TKey)?.Equals(item.Id) == true;
}