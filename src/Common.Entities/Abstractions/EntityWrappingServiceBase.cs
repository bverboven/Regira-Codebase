using Regira.DAL.Paging;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Abstractions;

public abstract class EntityWrappingServiceBase<TEntity> : IEntityService<TEntity>
    where TEntity : class, IEntity<int>
{
    private readonly IEntityService<TEntity> _service;
    protected EntityWrappingServiceBase(IEntityService<TEntity> service)
    {
        _service = service;
    }

    public virtual Task<TEntity?> Details(int id)
        => _service.Details(id);
    public virtual Task<IList<TEntity>> List(object? so = default, PagingInfo? pagingInfo = null)
        => _service.List(so, pagingInfo);
    public virtual Task<int> Count(object? so)
        => _service.Count(so);

    public virtual Task Add(TEntity item)
        => _service.Add(item);
    public virtual Task Modify(TEntity item)
        => _service.Modify(item);
    public virtual Task Save(TEntity item)
        => _service.Save(item);
    public virtual Task Remove(TEntity item)
        => _service.Remove(item);

    public virtual Task<int> SaveChanges(CancellationToken token = default)
        => _service.SaveChanges(token);
}

public abstract class EntityWrappingServiceBase<TEntity, TSearchObject, TSortBy, TIncludes> : EntityWrappingServiceBase<TEntity, int, TSearchObject, TSortBy, TIncludes>
    where TEntity : class, IEntity<int>
    where TSearchObject : ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    protected EntityWrappingServiceBase(IEntityService<TEntity, int, TSearchObject, TSortBy, TIncludes> service)
        : base(service)
    {
    }
}
public abstract class EntityWrappingServiceBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes> : IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    private readonly IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes> _service;
    protected EntityWrappingServiceBase(IEntityService<TEntity, TKey, TSearchObject, TSortBy, TIncludes> service)
    {
        _service = service;
    }


    public virtual Task<TEntity?> Details(TKey id)
        => _service.Details(id);
    public virtual Task<IList<TEntity>> List(object? so = default, PagingInfo? pagingInfo = null)
        => _service.List(so, pagingInfo);
    public virtual Task<int> Count(object? so)
        => _service.Count(so);

    public virtual Task<IList<TEntity>> List(IList<TSearchObject?> so, IList<TSortBy> sortBy, TIncludes? includes = null, PagingInfo? pagingInfo = null)
        => _service.List(so, sortBy, includes, pagingInfo);
    public virtual Task<int> Count(IList<TSearchObject?> so)
        => _service.Count(so);

    public virtual Task Add(TEntity item)
        => _service.Add(item);
    public virtual Task Modify(TEntity item)
        => _service.Modify(item);
    public virtual Task Save(TEntity item)
        => _service.Save(item);
    public virtual Task Remove(TEntity item)
        => _service.Remove(item);

    public virtual Task<int> SaveChanges(CancellationToken token = default)
        => _service.SaveChanges(token);
}