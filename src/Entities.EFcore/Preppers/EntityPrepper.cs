using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.Preppers.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.Preppers;

public class EntityPrepper<TContext, TEntity, TKey>(TContext dbContext, Func<TEntity, TContext, Task> prepareFunc) : IEntityPrepper<TEntity, TKey>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
{
    public Task Prepare(TEntity modified, TEntity? original) => prepareFunc(modified, dbContext);
}