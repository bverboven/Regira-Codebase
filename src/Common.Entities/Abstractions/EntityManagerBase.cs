using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Abstractions;

public abstract class EntityManagerBase<TEntity, TSearchObject, TSortBy, TIncludes> : EntityManagerBase<TEntity, int, TSearchObject, TSortBy, TIncludes>, IEntityManager<TEntity, TSearchObject, TSortBy, TIncludes>
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    protected EntityManagerBase(IEntityRepository<TEntity, int, TSearchObject, TSortBy, TIncludes> repo)
        : base(repo)
    {
    }
}
public abstract class EntityManagerBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes> : EntityWrappingServiceBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes>,
    IEntityManager<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    protected readonly IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes> Repo;
    protected EntityManagerBase(IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes> repo)
        : base(repo)
    {
        Repo = repo;
    }
}