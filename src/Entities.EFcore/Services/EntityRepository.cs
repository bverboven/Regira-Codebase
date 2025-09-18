using Regira.DAL.Paging;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Services.Abstractions;

namespace Regira.Entities.EFcore.Services;

public class EntityRepository<TEntity>(IEntityReadService<TEntity, int, SearchObject<int>> readService, IEntityWriteService<TEntity, int> writeService)
    : EntityRepository<TEntity, int>(readService, writeService), IEntityRepository<TEntity>
    where TEntity : class, IEntity<int>;

public class EntityRepository<TEntity, TKey>(IEntityReadService<TEntity, TKey, SearchObject<TKey>> readService, IEntityWriteService<TEntity, TKey> writeService)
    : EntityRepository<TEntity, TKey, SearchObject<TKey>>(readService, writeService)
    where TEntity : class, IEntity<TKey>;

public class EntityRepository<TEntity, TKey, TSearchObject>(IEntityReadService<TEntity, TKey, TSearchObject> readService, IEntityWriteService<TEntity, TKey> writeService)
    : IEntityRepository<TEntity, TKey, TSearchObject>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
{
    public virtual Task<TEntity?> Details(TKey id)
    => readService.Details(id);

    public virtual Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null)
        => readService.List(so, pagingInfo);
    public virtual Task<long> Count(TSearchObject? so)
        => readService.Count(so);

    public virtual Task<IList<TEntity>> List(object? so, PagingInfo? pagingInfo)
         => readService.List(so, pagingInfo);
    public virtual Task<long> Count(object? so)
         => readService.Count(so);



    public virtual Task Add(TEntity item)
        => writeService.Add(item);
    public virtual Task<TEntity?> Modify(TEntity item)
        => writeService.Modify(item);
    public virtual Task Save(TEntity item)
        => writeService.Save(item);
    public virtual Task Remove(TEntity item)
        => writeService.Remove(item);

    public virtual Task<int> SaveChanges(CancellationToken token = default)
        => writeService.SaveChanges(token);
}