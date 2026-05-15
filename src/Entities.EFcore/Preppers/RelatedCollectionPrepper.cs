using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.EFcore.Preppers.Abstractions;
using Regira.Entities.Models.Abstractions;
using System.Linq.Expressions;

namespace Regira.Entities.EFcore.Preppers;

public class RelatedCollectionPrepper<TContext, TEntity, TRelated, TEntityKey, TRelatedKey>(
    TContext dbContext,
    Expression<Func<TEntity, ICollection<TRelated>?>> navigationExpression,
    IEnumerable<IEntityPrepper<TRelated>>? nestedPreppers = null) : EntityPrepperBase<TEntity>
    where TContext : DbContext
    where TEntity : class, IEntity<TEntityKey>
    where TRelated : class, IEntity<TRelatedKey>
{
    private readonly Func<TEntity, ICollection<TRelated>?> _selectorFunc = navigationExpression.Compile();
    private readonly IReadOnlyList<IEntityPrepper<TRelated>> _nestedPreppers = nestedPreppers?.ToList() ?? [];

    public override async Task Prepare(TEntity modified, TEntity? original, CancellationToken token = default)
    {
        if (original != null)
        {
            var originalItems = _nestedPreppers.Count > 0 ? _selectorFunc(original)?.ToList() : null;
            var modifiedItems = _nestedPreppers.Count > 0 ? _selectorFunc(modified)?.ToList() : null;

            dbContext.UpdateRelatedCollection<TEntity, TRelated, TEntityKey, TRelatedKey>(modified, original, navigationExpression);

            if (_nestedPreppers.Count > 0 && modifiedItems != null)
            {
                foreach (var modifiedItem in modifiedItems)
                {
                    TRelated? originalItem = null;
                    if (modifiedItem.Id != null && !modifiedItem.Id.Equals(default(TRelatedKey)))
                        originalItem = originalItems?.FirstOrDefault(o => o.Id != null && o.Id.Equals(modifiedItem.Id));

                    foreach (var nestedPrepper in _nestedPreppers)
                        await nestedPrepper.Prepare(modifiedItem, originalItem, token);
                }
            }
        }
        else if (_nestedPreppers.Count > 0)
        {
            var modifiedItems = _selectorFunc(modified)?.ToList();
            if (modifiedItems is { Count: > 0 })
            {
                foreach (var modifiedItem in modifiedItems)
                {
                    foreach (var nestedPrepper in _nestedPreppers)
                        await nestedPrepper.Prepare(modifiedItem, null, token);
                }
            }
        }
    }
}