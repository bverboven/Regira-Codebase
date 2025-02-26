using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Regira.Entities.EFcore.Primers.Abstractions;

public abstract class EntityPrimerBase<T> : IEntityPrimer<T>
    where T : class
{
    public virtual Task PrepareManyAsync(IList<EntityEntry> entries)
    {
        foreach (var entry in entries)
        {
            var entity = (T)entry.Entity;
            if (CanPrepare(entity))
            {
                PrepareAsync(entity, entry);
            }
        }

        return Task.CompletedTask;
    }



    public abstract Task PrepareAsync(T entity, EntityEntry entry);
    public virtual bool CanPrepare(T? entity) => entity != null;

    Task IEntityPrimer.PrepareAsync(object entity, EntityEntry entry) => PrepareAsync((T)entity, entry);
    bool IEntityPrimer.CanPrepare(object entity) => CanPrepare(entity as T);
}