using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Abstractions;

public interface IEntityWriteService<in TEntity> : IEntityWriteService<TEntity, int>
    where TEntity : class, IEntity<int>
{
}
public interface IEntityWriteService<in TEntity, in TKey>
    where TEntity : class, IEntity<TKey>
{
    Task Add(TEntity item);
    Task Modify(TEntity item);
    Task Save(TEntity item);
    Task Remove(TEntity item);
    Task<int> SaveChanges(CancellationToken token = default);
}