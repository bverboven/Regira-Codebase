using Regira.Entities.EFcore.Extensions;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models.Abstractions;
using Regira.Utilities;

namespace Regira.Entities.EFcore.QueryBuilders;

public class DefaultFilteredQueryBuilder<TEntity, TKey, TSearchObject> : FilteredQueryBuilderBase<TEntity, TKey, TSearchObject>
    where TEntity : IEntity<TKey>
    where TSearchObject : ISearchObject<TKey>
{
    public override IQueryable<TEntity> Build(IQueryable<TEntity> query, TSearchObject? so)
    {
        if (so != null)
        {
            query = FilterIds(query, so);
            query = FilterTimestamps(query, so);
            query = FilterArchivable(query, so);
        }

        return query;
    }

    public virtual IQueryable<TEntity> FilterIds(IQueryable<TEntity> query, TSearchObject so)
    {
        query = query.FilterId(so.Id);
        query = query.FilterIds(so.Ids);
        query = query.FilterExclude(so.Exclude);

        return query;
    }
    public virtual IQueryable<TEntity> FilterTimestamps(IQueryable<TEntity> query, TSearchObject so)
    {
        if (TypeUtility.ImplementsInterface<IHasCreated>(typeof(TEntity)))
        {
            query = query.Cast<IHasCreated>().FilterCreated(so.MinCreated, so.MaxCreated).Cast<TEntity>();
        }
        if (TypeUtility.ImplementsInterface<IHasLastModified>(typeof(TEntity)))
        {
            query = query.Cast<IHasLastModified>().FilterLastModified(so.MinLastModified, so.MaxLastModified).Cast<TEntity>();
        }

        return query;
    }
    public virtual IQueryable<TEntity> FilterArchivable(IQueryable<TEntity> query, TSearchObject so)
    {
        if (TypeUtility.ImplementsInterface<IArchivable>(typeof(TEntity)))
        {
            query = query.Cast<IArchivable>().FilterArchivable(so.IsArchived).Cast<TEntity>();
        }

        return query;
    }
}
public class DefaultFilteredQueryBuilder<TEntity, TSearchObject> : DefaultFilteredQueryBuilder<TEntity, int, TSearchObject>,
    IFilteredQueryBuilder<TEntity, TSearchObject>
    where TEntity : IEntity<int>
    where TSearchObject : ISearchObject<int>;