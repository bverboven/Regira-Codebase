using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Abstractions;

public interface IEntityService<TEntity> : IEntityService<TEntity, int>
    where TEntity : class, IEntity<int>;
public interface IEntityService<TEntity, in TKey> : IEntityReadService<TEntity, TKey>, IEntityWriteService<TEntity, TKey>
    where TEntity : class, IEntity<TKey>;
public interface IEntityService<TEntity, in TKey, in TSearchObject>
    : IEntityReadService<TEntity, TKey, TSearchObject>, IEntityService<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new();


public interface IEntityService<TEntity, TSearchObject, TSortBy, TIncludes>
    : IEntityService<TEntity, int, TSearchObject, TSortBy, TIncludes>, IEntityService<TEntity>
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;
public interface IEntityService<TEntity, in TKey, TSearchObject, TSortBy, TIncludes>
    : IEntityReadService<TEntity, TKey, TSearchObject, TSortBy, TIncludes>, IEntityService<TEntity, TKey, TSearchObject>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;