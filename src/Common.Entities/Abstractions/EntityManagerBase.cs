using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Abstractions;

public abstract class EntityManagerBase<TEntity, TSearchObject, TSortBy, TIncludes>(
    IEntityRepository<TEntity, int, TSearchObject, TSortBy, TIncludes> repo)
    : EntityManagerBase<TEntity, int, TSearchObject, TSortBy, TIncludes>(repo),
        IEntityManager<TEntity, TSearchObject, TSortBy, TIncludes>
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;
public abstract class EntityManagerBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes>(
    IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes> repo)
    : EntityWrappingServiceBase<TEntity, TKey, TSearchObject, TSortBy, TIncludes>(repo),
        IEntityManager<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    protected readonly IEntityRepository<TEntity, TKey, TSearchObject, TSortBy, TIncludes> Repo = repo;
}