using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.EFcore.Preppers.Abstractions;
using Regira.Entities.Models.Abstractions;
using System.Linq.Expressions;

namespace Regira.Entities.EFcore.Preppers;

public class RelatedCollectionPrepper<TContext, TEntity, TRelated, TEntityKey, TRelatedKey>(TContext dbContext, Expression<Func<TEntity, ICollection<TRelated>?>> navigationExpression) : IEntityPrepper<TEntity, TEntityKey>
    where TContext : DbContext
    where TEntity : class, IEntity<TEntityKey>
    where TRelated : class, IEntity<TRelatedKey>
{
    public Task Prepare(TEntity modified, TEntity? original)
    {
        if (original != null)
        {
            dbContext.UpdateRelatedCollection<TEntity, TRelated, TEntityKey, TRelatedKey>(modified, original, navigationExpression);
        }

        return Task.CompletedTask;
    }
}