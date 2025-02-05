using Regira.Entities.EFcore.Extensions;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.QueryBuilders;

public class FilterArchivablesQueryBuilder : FilterArchivablesQueryBuilder<int>;
public class FilterArchivablesQueryBuilder<TKey> : GlobalFilteredQueryBuilderBase<IArchivable, TKey>
{
    public override IQueryable<IArchivable> Build(IQueryable<IArchivable> query, ISearchObject<TKey>? so)
    {
        return query.FilterArchivable(so?.IsArchived);
    }
}