﻿using Microsoft.EntityFrameworkCore;
using Regira.DAL.Paging;
using Regira.Entities.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Extensions;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Utilities;

namespace Regira.Entities.EFcore.Services;

public class EntityRepository<TContext, TEntity>(TContext dbContext)
    : EntityRepository<TContext, TEntity, int>(dbContext), IEntityRepository<TEntity>
    where TContext : DbContext
    where TEntity : class, IEntity<int>;
public abstract class EntityRepository<TContext, TEntity, TKey>(TContext dbContext)
    : EntityRepository<TContext, TEntity, TKey, SearchObject<TKey>>(dbContext)
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>;
public class EntityRepository<TContext, TEntity, TKey, TSearchObject>(TContext dbContext)
    : IEntityService<TEntity, TKey, TSearchObject>, IEntityRepository<TEntity, TKey>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
{
    public virtual DbSet<TEntity> DbSet => DbContext.Set<TEntity>();
    public virtual IQueryable<TEntity> Query => DbSet;

    protected readonly TContext DbContext = dbContext;


    public virtual async Task<TEntity?> Details(TKey id)
        => id != null && id.Equals(default(TKey)) == false // make sure an id is passed or return null
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

    public virtual TSearchObject? Convert(object? so)
        => so != null
            ? so as TSearchObject ?? ObjectUtility.Create<TSearchObject>(so)
            : null;
    public bool IsNew(TEntity item)
        => item.IsNew();
}