using Regira.DAL.Paging;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Abstractions;

public abstract class EntityManagerBase<TEntity, TSearchObject, TSortBy, TIncludes> : EntityManagerBase<TEntity, int, TSearchObject, TSortBy, TIncludes>, IEntityManager<TEntity, TSearchObject, TSortBy, TIncludes>
    where TEntity : class, IEntity<int>
    where TSearchObject : ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    protected EntityManagerBase(IEntityRepository<TEntity, int, TSearchObject, TSortBy, TIncludes> repo)
        : base(repo)
    {
    }
}
public abstract class EntityManagerBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes> : IEntityManager<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    protected readonly IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes> Repo;
    protected EntityManagerBase(IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes> repo)
    {
        Repo = repo;
    }

    public virtual Task<TEntity?> Details(TKey id) => Repo.Details(id);
    public virtual Task<IList<TEntity>> List(object? so = default, PagingInfo? pagingInfo = null) => Repo.List(so, pagingInfo);
    public virtual Task<IList<TEntity>> List(IList<TSearchObject?> so, IList<TSortBy> sortBy, TIncludes? includes = null, PagingInfo? pagingInfo = null)
        => Repo.List(so, sortBy, includes, pagingInfo);
    public virtual Task<int> Count(object? so) => Repo.Count(so);
    public virtual Task<int> Count(IList<TSearchObject?> so)
        => Repo.Count(so);

    public virtual Task Add(TEntity item) => Repo.Add(item);
    public virtual Task Modify(TEntity item) => Repo.Modify(item);
    public virtual Task Save(TEntity item) => Repo.Save(item);

    public virtual Task Remove(TEntity item) => Repo.Remove(item);

    public virtual Task<int> SaveChanges(CancellationToken token = default) => Repo.SaveChanges(token);

}