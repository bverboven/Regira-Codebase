using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Services.Abstractions;

//public interface IEntityWriteService<TEntity> : IEntityWriteService<TEntity, int>
//    where TEntity : class, IEntity<int>;
public interface IEntityWriteService<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    Task Add(TEntity item);
    Task<TEntity?> Modify(TEntity item);
    Task Save(TEntity item);
    Task Remove(TEntity item);
    Task<int> SaveChanges(CancellationToken token = default);
}