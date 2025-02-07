using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.QueryBuilders.Abstractions;

public interface IFilteredQueryBuilder<TEntity, in TSearchObject> : IFilteredQueryBuilder<TEntity, int, TSearchObject>
    where TSearchObject : ISearchObject<int>;
public interface IFilteredQueryBuilder<TEntity, TKey, in TSearchObject>
    where TSearchObject : ISearchObject<TKey>
{
    IQueryable<TEntity> Build(IQueryable<TEntity> query, TSearchObject? so);
}

public interface IGlobalFilteredQueryBuilder<TEntity> : IGlobalFilteredQueryBuilder<TEntity, int>;
public interface IGlobalFilteredQueryBuilder<TEntity, TKey> : IGlobalFilteredQueryBuilder
{
    IQueryable<TEntity> Build(IQueryable<TEntity> query, ISearchObject<TKey>? so);
}
public interface IGlobalFilteredQueryBuilder
{
    IQueryable<TEntity> Build<TEntity, TKey>(IQueryable<TEntity> query, ISearchObject<TKey>? so);
}