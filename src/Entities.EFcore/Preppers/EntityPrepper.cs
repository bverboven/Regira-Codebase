using Microsoft.EntityFrameworkCore;
using Regira.Entities.EFcore.Preppers.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.Preppers;

public class EntityPrepper<TEntity>(Func<TEntity, Task> prepareFunc) : EntityPrepperBase<TEntity>
    where TEntity : class
{
    public override Task Prepare(TEntity modified, TEntity? original) => prepareFunc(modified);
}

public class EntityPrepper<TContext, TEntity, TKey>(TContext dbContext, Func<TEntity, TContext, Task> prepareFunc) : EntityPrepperBase<TEntity>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
{
    public override Task Prepare(TEntity modified, TEntity? original) => prepareFunc(modified, dbContext);
}