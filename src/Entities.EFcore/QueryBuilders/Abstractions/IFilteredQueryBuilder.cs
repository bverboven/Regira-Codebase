using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.QueryBuilders.Abstractions;
public interface IFilteredQueryBuilder<TEntity, TKey, in TSearchObject>
    where TSearchObject : ISearchObject<TKey>
{
    IQueryable<TEntity> Build(IQueryable<TEntity> query, TSearchObject? so);
}

// Without ID type
//public interface IFilteredQueryBuilder<TEntity> 
//    : IFilteredQueryBuilder<TEntity, SearchObject<int>>;
//public interface IFilteredQueryBuilder<TEntity, in TSearchObject> 
//    : IFilteredQueryBuilder<TEntity, int, TSearchObject>
//    where TSearchObject : ISearchObject<int>;