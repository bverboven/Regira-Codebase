using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.QueryBuilders.Abstractions;

public interface IIncludableQueryBuilder<TEntity> : IIncludableQueryBuilder<TEntity, int>
    where TEntity : IEntity<int>;

public interface IIncludableQueryBuilder<TEntity, TKey>
    where TEntity : IEntity<TKey>
{
    IQueryable<TEntity> AddIncludes(IQueryable<TEntity> query);
}