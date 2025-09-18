using Regira.DAL.Paging;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Services.Abstractions;

namespace Regira.Entities.EFcore.Services;

public class EntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>
    (IEntityReadService<TEntity, int, TSearchObject, TSortBy, TIncludes> readService, IEntityWriteService<TEntity, int> writeService)
    : EntityRepository<TEntity, int, TSearchObject, TSortBy, TIncludes>(readService, writeService), IEntityRepository<TEntity, TSearchObject, TSortBy, TIncludes>
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;

public class EntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>(
    IEntityReadService<TEntity, TKey, TSearchObject, TSortBy, TIncludes> readService,
    IEntityWriteService<TEntity, TKey> writeService)
    : IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    public virtual Task<TEntity?> Details(TKey id)
        => readService.Details(id);
    public virtual Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null)
        => readService.List(so, pagingInfo);
    public Task<long> Count(TSearchObject? so)
        => readService.Count(so);

    public virtual Task<IList<TEntity>> List(IList<TSearchObject?> so, IList<TSortBy> sortBy, TIncludes? includes = null, PagingInfo? pagingInfo = null)
        => readService.List(so, sortBy, includes, pagingInfo);

    public virtual Task<long> Count(IList<TSearchObject?> so)
        => readService.Count(so);

    Task<IList<TEntity>> IEntityReadService<TEntity, TKey>.List(object? so, PagingInfo? pagingInfo)
        => readService.List(so, pagingInfo);
    Task<long> IEntityReadService<TEntity, TKey>.Count(object? so)
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