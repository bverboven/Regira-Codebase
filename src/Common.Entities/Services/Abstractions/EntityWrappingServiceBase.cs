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
    : EntityWrappingServiceBase<TEntity, TKey, SearchObject<TKey>>(service), IEntityService<TEntity, TKey>
    where TEntity : class, IEntity<TKey>;

public abstract class EntityWrappingServiceBase<TEntity, TKey, TSearchObject>(
    IEntityService<TEntity, TKey, TSearchObject> service) : IEntityService<TEntity, TKey, TSearchObject>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
{
    protected readonly IEntityService<TEntity, TKey, TSearchObject> Service = service;

    public virtual Task<TEntity?> Details(TKey id)
        => Service.Details(id);

    public virtual Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null)
        => Service.List(so, pagingInfo);
    public virtual Task<long> Count(TSearchObject? so)
        => Service.Count(so);

    public virtual Task<IList<TEntity>> List(object? so, PagingInfo? pagingInfo)
        => Service.List(so, pagingInfo);
    public virtual Task<long> Count(object? so)
        => Service.Count(so);


    public virtual Task Add(TEntity item)
        => Service.Add(item);
    public virtual Task<TEntity?> Modify(TEntity item)
        => Service.Modify(item);
    public virtual Task Save(TEntity item)
        => Service.Save(item);
    public virtual Task Remove(TEntity item)
        => Service.Remove(item);

    public virtual Task<int> SaveChanges(CancellationToken token = default)
        => Service.SaveChanges(token);
}

public abstract class EntityWrappingServiceBase<TEntity, TSearchObject, TSortBy, TIncludes>(
    IEntityService<TEntity, int, TSearchObject, TSortBy, TIncludes> service)
    : EntityWrappingServiceBase<TEntity, int, TSearchObject, TSortBy, TIncludes>(service)
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
    public virtual Task<TEntity?> Details(TKey id)
        => service.Details(id);
    public virtual Task<IList<TEntity>> List(object? so = null, PagingInfo? pagingInfo = null)
        => service.List(so, pagingInfo);
    public Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null)
        => service.List(so, pagingInfo);
    public virtual Task<IList<TEntity>> List(IList<TSearchObject?> so, IList<TSortBy> sortBy, TIncludes? includes = null, PagingInfo? pagingInfo = null)
        => service.List(so, sortBy, includes, pagingInfo);

    public virtual Task<long> Count(object? so)
        => service.Count(so);
    public Task<long> Count(TSearchObject? so)
        => service.Count(so);
    public virtual Task<long> Count(IList<TSearchObject?> so)
        => service.Count(so);

    public virtual Task Add(TEntity item)
        => service.Add(item);
    public virtual Task<TEntity?> Modify(TEntity item)
        => service.Modify(item);
    public virtual Task Save(TEntity item)
        => service.Save(item);
    public virtual Task Remove(TEntity item)
        => service.Remove(item);

    public virtual Task<int> SaveChanges(CancellationToken token = default)
        => service.SaveChanges(token);

    public virtual TSearchObject? Convert(object? so)
        => so != null
            ? so as TSearchObject ?? ObjectUtility.Create<TSearchObject>(so)
            : null;
}
