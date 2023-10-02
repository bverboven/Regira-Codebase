using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Abstractions;

public interface IEntityService<TEntity> : IEntityService<TEntity, int>
    where TEntity : class, IEntity<int>
{
}
public interface IEntityService<TEntity, in TKey> : IEntityReadService<TEntity, TKey>, IEntityWriteService<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
}


public interface IEntityService<TEntity, TSearchObject, TSortBy, TIncludes> : IEntityService<TEntity, int, TSearchObject, TSortBy, TIncludes>, IEntityService<TEntity>
    where TEntity : class, IEntity<int>
    where TSearchObject : ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
}
public interface IEntityService<TEntity, in TKey, TSearchObject, TSortBy, TIncludes>
    : IEntityReadService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, IEntityService<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
}