using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.QueryBuilders.Abstractions;

public interface IFilteredQueryBuilder<TEntity, in TSearchObject> : IFilteredQueryBuilder<TEntity, int, TSearchObject>
    where TEntity : IEntity<int>
    where TSearchObject : ISearchObject<int>;
public interface IFilteredQueryBuilder<TEntity, TKey, in TSearchObject>
    where TEntity : IEntity<TKey>
    where TSearchObject : ISearchObject<TKey>
{
    IQueryable<TEntity> Build(IQueryable<TEntity> query, TSearchObject? so);
}