using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.QueryBuilders.Abstractions;

public interface ISortedQueryBuilder<TEntity> : ISortedQueryBuilder<TEntity, int>
    where TEntity : IEntity<int>;

public interface ISortedQueryBuilder<TEntity, TKey>
    where TEntity : IEntity<TKey>
{
    IQueryable<TEntity> SortBy(IQueryable<TEntity> query);
}