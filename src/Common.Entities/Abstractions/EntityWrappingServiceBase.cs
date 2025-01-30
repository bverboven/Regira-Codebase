using Regira.DAL.Paging;
using Regira.Entities.Models.Abstractions;
using Regira.Utilities;

namespace Regira.Entities.Abstractions;

public abstract class EntityWrappingServiceBase<TEntity>(IEntityService<TEntity> service)
    : EntityWrappingServiceBase<TEntity, int>(service), IEntityService<TEntity>
    where TEntity : class, IEntity<int>;

public abstract class EntityWrappingServiceBase<TEntity, TKey>(IEntityService<TEntity, TKey> service)
    : IEntityService<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    public virtual Task<TEntity?> Details(TKey id)
        => service.Details(id);
    public virtual Task<IList<TEntity>> List(object? so = null, PagingInfo? pagingInfo = null)
        => service.List(so, pagingInfo);
    public virtual Task<int> Count(object? so)
        => service.Count(so);

    public virtual Task Add(TEntity item)
        => service.Add(item);
    public virtual Task Modify(TEntity item)
        => service.Modify(item);
    public virtual Task Save(TEntity item)
        => service.Save(item);
    public virtual Task Remove(TEntity item)
        => service.Remove(item);

    public virtual Task<int> SaveChanges(CancellationToken token = default)
        => service.SaveChanges(token);
}

public abstract class EntityWrappingServiceBase<TEntity, TKey, TSearchObject>(
    IEntityService<TEntity, TKey, TSearchObject> service) : EntityWrappingServiceBase<TEntity, TKey>(service),
    IEntityService<TEntity, TKey, TSearchObject>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
{
    public virtual Task<IList<TEntity>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null)
        => service.List(so, pagingInfo);
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
    public virtual Task<int> Count(object? so)
        => service.Count(so);

    public virtual Task<IList<TEntity>> List(IList<TSearchObject?> so, IList<TSortBy> sortBy, TIncludes? includes = null, PagingInfo? pagingInfo = null)
        => service.List(so, sortBy, includes, pagingInfo);
    public virtual Task<int> Count(IList<TSearchObject?> so)
        => service.Count(so);

    public virtual Task Add(TEntity item)
        => service.Add(item);
    public virtual Task Modify(TEntity item)
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
