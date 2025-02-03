using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.QueryBuilders.Abstractions;

public interface IIncludableQueryBuilder<TEntity, TSearchObject, TSortBy, TIncludes> : IIncludableQueryBuilder<TEntity, int, TSearchObject, TSortBy, TIncludes>
    where TEntity : IEntity<int>
    where TSearchObject : ISearchObject<int>
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum;

public interface IIncludableQueryBuilder<TEntity, TKey, TSearchObject, TSortBy, TIncludes>
    where TEntity : IEntity<TKey>
    where TSearchObject : ISearchObject<TKey>
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    IQueryable<TEntity> Build(IQueryable<TEntity> query, IList<TSearchObject>? so, IList<TSortBy>? sortByList, TIncludes? includes);
}