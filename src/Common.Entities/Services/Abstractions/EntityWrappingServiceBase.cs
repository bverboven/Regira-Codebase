using Regira.DAL.Paging;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Utilities;

namespace Regira.Entities.Services.Abstractions;

public abstract class EntityWrappingServiceBase<TEntity>(IEntityService<TEntity, int, SearchObject<int>> service)
    : EntityWrappingServiceBase<TEntity, int, SearchObject<int>>(service), IEntityService<TEntity>
    where TEntity : class, IEntity<int>;

public abstract class EntityWrappingServiceBase<TEntity, TKey>(
    IEntityService<TEntity, TKey, SearchObject<TKey>> service)
    : EntityWrappingServiceBase<TEntity, TKey, SearchObject<TKey>>(service)//, IEntityService<TEntity, TKey> (already included)
    where TEntity : class, IEntity<TKey>;

public abstract class EntityWrappingServiceBase<TEntity, TKey, TSearchObject>(
    IEntityService<TEntity, TKey, TSearchObject> service) : IEntityService<TEntity, TKey, TSearchObject>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
{
    protected readonly IEntityService<TEntity, TKey, TSearchObject> Service = service;

    public virtual Task<TEntity?> Details(TKey id, CancellationToken token = default)
        => Service.Details(id, token);

    public virtual Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null, CancellationToken token = default)
        => Service.List(so, pagingInfo, token);
    public virtual Task<long> Count(TSearchObject? so = null, CancellationToken token = default)
        => Service.Count(so, token);

    public virtual Task<IList<TEntity>> List(object? so, PagingInfo? pagingInfo, CancellationToken token = default)
        => Service.List(so, pagingInfo, token);
    public virtual Task<long> Count(object? so, CancellationToken token = default)
        => Service.Count(so, token);


    public virtual Task Add(TEntity item, CancellationToken token = default)
        => Service.Add(item, token);
    public virtual Task<TEntity?> Modify(TEntity item, CancellationToken token = default)
        => Service.Modify(item, token);
    public virtual Task Save(TEntity item, CancellationToken token = default)
        => Service.Save(item, token);
    public virtual Task Remove(TEntity item, CancellationToken token = default)
        => Service.Remove(item, token);

    public virtual Task<int> SaveChanges(CancellationToken token = default)
        => Service.SaveChanges(token);
}

public abstract class EntityWrappingServiceBase<TEntity, TSearchObject, TSortBy, TIncludes>(
    IEntityService<TEntity, int, TSearchObject, TSortBy, TIncludes> service)
    : EntityWrappingServiceBase<TEntity, int, TSearchObject, TSortBy, TIncludes>(service), IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;
public abstract class EntityWrappingServiceBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes>(
    IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes> service)
    : IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    public virtual Task<TEntity?> Details(TKey id, CancellationToken token = default)
        => service.Details(id, token);
    public virtual Task<IList<TEntity>> List(object? so = null, PagingInfo? pagingInfo = null, CancellationToken token = default)
        => service.List(so, pagingInfo, token);
    public Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null, CancellationToken token = default)
        => service.List(so, pagingInfo, token);
    public virtual Task<IList<TEntity>> List(IList<TSearchObject?> so, IList<TSortBy> sortBy, TIncludes? includes = null, PagingInfo? pagingInfo = null, CancellationToken token = default)
        => service.List(so, sortBy, includes, pagingInfo, token);

    public virtual Task<long> Count(object? so, CancellationToken token = default)
        => service.Count(so, token);
    public Task<long> Count(TSearchObject? so = null, CancellationToken token = default)
        => service.Count(so, token);
    public virtual Task<long> Count(IList<TSearchObject?> so, CancellationToken token = default)
        => service.Count(so, token);

    public virtual Task Add(TEntity item, CancellationToken token = default)
        => service.Add(item, token);
    public virtual Task<TEntity?> Modify(TEntity item, CancellationToken token = default)
        => service.Modify(item, token);
    public virtual Task Save(TEntity item, CancellationToken token = default)
        => service.Save(item, token);
    public virtual Task Remove(TEntity item, CancellationToken token = default)
        => service.Remove(item, token);

    public virtual Task<int> SaveChanges(CancellationToken token = default)
        => service.SaveChanges(token);

    public virtual TSearchObject? Convert(object? so)
        => so != null
            ? so as TSearchObject ?? ObjectUtility.Create<TSearchObject>(so)
            : null;
}
