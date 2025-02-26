using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Regira.Entities.EFcore.Primers.Abstractions;

public interface IEntityPrimer<in T> : IEntityPrimer
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