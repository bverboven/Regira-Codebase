using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.QueryBuilders;

public class SortedQueryBuilder<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> sortQuery) 
    : SortedQueryBuilder<TEntity, int>(sortQuery), ISortedQueryBuilder<TEntity>
    where TEntity : IEntity<int>;

public class SortedQueryBuilder<TEntity, TKey>(Func<IQueryable<TEntity>, IQueryable<TEntity>> sortQuery) 
    : ISortedQueryBuilder<TEntity, TKey>
    where TEntity : IEntity<TKey>
{
    public virtual IQueryable<TEntity> SortBy(IQueryable<TEntity> query)
        => sortQuery(query);
}