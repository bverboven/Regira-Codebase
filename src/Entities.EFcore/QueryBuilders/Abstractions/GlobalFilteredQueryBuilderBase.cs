using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.QueryBuilders.Abstractions;

public abstract class GlobalFilteredQueryBuilderBase<TEntity> : GlobalFilteredQueryBuilderBase<TEntity, int>;
public abstract class GlobalFilteredQueryBuilderBase<TEntity, TKey> : FilteredQueryBuilderBase<TEntity, TKey, ISearchObject<TKey>>,
    IGlobalFilteredQueryBuilder<TEntity, TKey>
{
    IQueryable<TEntity> IGlobalFilteredQueryBuilder<TEntity, TKey>.Build(IQueryable<TEntity> query, ISearchObject<TKey>? so)
        => Build(query, so);
    IQueryable<T> IGlobalFilteredQueryBuilder.Build<T, TK>(IQueryable<T> query, ISearchObject<TK>? so)
        => Build(query.Cast<TEntity>(), so as ISearchObject<TKey>).Cast<T>();
}