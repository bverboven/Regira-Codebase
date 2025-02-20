using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.QueryBuilders;

public class IncludableQueryBuilder<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> addIncludes)
    : IncludableQueryBuilder<TEntity, int>(addIncludes)
    where TEntity : IEntity<int>;

public class IncludableQueryBuilder<TEntity, TKey>(Func<IQueryable<TEntity>, IQueryable<TEntity>> addIncludes)
    : IIncludableQueryBuilder<TEntity, TKey>
    where TEntity : IEntity<TKey>
{
    public virtual IQueryable<TEntity> AddIncludes(IQueryable<TEntity> query)
        => addIncludes(query);
}