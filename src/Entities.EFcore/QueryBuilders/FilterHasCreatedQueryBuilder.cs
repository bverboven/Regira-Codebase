using Regira.Entities.EFcore.Extensions;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.QueryBuilders;

public class FilterHasCreatedQueryBuilder : FilterHasCreatedQueryBuilder<int>;
public class FilterHasCreatedQueryBuilder<TKey> : GlobalFilteredQueryBuilderBase<IHasCreated, TKey>
{
    public override IQueryable<IHasCreated> Build(IQueryable<IHasCreated> query, ISearchObject<TKey>? so)
    {
        return query.FilterCreated(so?.MinCreated, so?.MaxCreated);
    }
}