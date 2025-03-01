using Microsoft.EntityFrameworkCore;
using Regira.Entities.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.EFcore.Preppers.Abstractions;
using Regira.Entities.Extensions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.Services;

public class EntityWriteService<TContext, TEntity>(TContext dbContext, IEntityReadService<TEntity, int> readService, IEnumerable<IEntityPrepper> preppers)
    : EntityWriteService<TContext, TEntity, int>(dbContext, readService, preppers)
    where TContext : DbContext
    where TEntity : class, IEntity<int>;


public class EntityWriteService<TContext, TEntity, TKey>(TContext dbContext, IEntityReadService<TEntity, TKey> readService, IEnumerable<IEntityPrepper> preppers)
    : IEntityWriteService<TEntity, TKey>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
{
    protected TContext DbContext = dbContext;
    public virtual DbSet<TEntity> DbSet => DbContext.Set<TEntity>();

    public virtual async Task Add(TEntity item)
    {
        await PrepareItem(item, null);

        DbSet.Add(item);
    }
    public virtual async Task<TEntity?> Modify(TEntity item)
    {
        var original = await readService.Details(item.Id);

        await PrepareItem(item, original);

        if (original != null)
        {
            DbContext.Entry(original).State = EntityState.Detached;
            DbContext.Attach(item);
            DbContext.Entry(item).OriginalValues.SetValues(original);
            DbContext.Entry(item).State = EntityState.Modified;
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

    public virtual async Task PrepareItem(TEntity item, TEntity? original)
    {
        var matchingPreppers = preppers.FindMatchingServices(item);
        foreach (var prepper in matchingPreppers)
        {
            await prepper.Prepare(item, original);
        }
    }

    public virtual Task<int> SaveChanges(CancellationToken token = default)
        => DbContext.SaveChangesAsync(token);
}
