using Microsoft.EntityFrameworkCore;
using Regira.Entities.Abstractions;
using Regira.Entities.Extensions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.Services;

public class EntityWriteService<TContext, TEntity>(TContext dbContext, IEntityReadService<TEntity, int> readService)
    : EntityWriteService<TContext, TEntity, int>(dbContext, readService), IEntityWriteService<TEntity>
    where TContext : DbContext
    where TEntity : class, IEntity<int>;


public class EntityWriteService<TContext, TEntity, TKey>(TContext dbContext, IEntityReadService<TEntity, TKey> readService)
    : IEntityWriteService<TEntity, TKey>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
{
    protected TContext DbContext = dbContext;
    public virtual DbSet<TEntity> DbSet => DbContext.Set<TEntity>();

    public virtual Task Add(TEntity item)
    {
        PrepareItem(item);

        DbSet.Add(item);

        return Task.CompletedTask;
    }
    public virtual async Task<TEntity?> Modify(TEntity item)
    {
        PrepareItem(item);

        var original = await readService.Details(item.Id);
        if (original != null)
        {
            DbContext.Attach(original);
            DbContext.Entry(original).CurrentValues.SetValues(item);
            DbContext.Entry(original).State = EntityState.Modified;
        }

        return original;
    }
    public virtual Task Save(TEntity item)
        => item.IsNew() ? Add(item) : Modify(item);
    public virtual Task Remove(TEntity item)
    {
        DbSet.Remove(item);
        return Task.CompletedTask;
    }

    public virtual void PrepareItem(TEntity item)
    {
    }
    public virtual Task<int> SaveChanges(CancellationToken token = default)
        => DbContext.SaveChangesAsync(token);
}
