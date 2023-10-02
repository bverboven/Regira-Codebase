using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Regira.Entities.EFcore.Abstractions;

public interface IEntityPrimer<in T> : IEntityPrimer
    where T : class
{
    Task PrepareAsync(T entity, EntityEntry entry);
    bool CanPrepare(T entity);
}

public interface IEntityPrimer
{
    Task PrepareManyAsync(IList<EntityEntry> entries);
    Task PrepareAsync(object entity, EntityEntry entry);
    bool CanPrepare(object entity);
}