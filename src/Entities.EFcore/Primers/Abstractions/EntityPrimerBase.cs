using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Regira.Entities.EFcore.Primers.Abstractions;

public abstract class EntityPrimerBase<T> : IEntityPrimer<T>
    where T : class
{
    public virtual async Task PrepareManyAsync(IList<EntityEntry> entries)
    {
        foreach (var entry in entries)
        {
            var entity = (T)entry.Entity;
            if (CanPrepare(entity))
            {
                await PrepareAsync(entity, entry);
            }
        }
    }

    public abstract Task PrepareAsync(T entity, EntityEntry entry);
    public virtual bool CanPrepare(T? entity) => entity != null;

    Task IEntityPrimer.PrepareAsync(object entity, EntityEntry entry) => PrepareAsync((T)entity, entry);
    bool IEntityPrimer.CanPrepare(object entity) => CanPrepare(entity as T);
}