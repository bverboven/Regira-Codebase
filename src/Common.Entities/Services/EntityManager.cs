using Regira.DAL.Paging;
using Regira.Entities.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Services;

public class EntityManager<TEntity>(IEntityRepository<TEntity> repo)
    : EntityManager<TEntity, int>(repo), IEntityManager<TEntity>
    where TEntity : class, IEntity<int>;
public class EntityManager<TEntity, TKey>(IEntityRepository<TEntity, TKey> repo) : IEntityManager<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    protected readonly IEntityRepository<TEntity, TKey> Repo = repo;


    public virtual Task<TEntity?> Details(TKey id) => Repo.Details(id);
    public virtual Task<IList<TEntity>> List(object? so = null, PagingInfo? pagingInfo = null) => Repo.List(so, pagingInfo);
    public virtual Task<int> Count(object? so) => Repo.Count(so);

    public virtual Task Add(TEntity item) => Repo.Add(item);
    public virtual Task Modify(TEntity item) => Repo.Modify(item);
    public virtual Task Save(TEntity item) => Repo.Save(item);

    public virtual Task Remove(TEntity item) => Repo.Remove(item);

    public virtual Task<int> SaveChanges(CancellationToken token = default) => Repo.SaveChanges(token);
}