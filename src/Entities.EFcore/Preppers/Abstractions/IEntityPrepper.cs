using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.Preppers.Abstractions;

public interface IEntityPrepper
{
    Task Prepare(object modified, object? original);
}
public interface IEntityPrepper<in TEntity, TEntityKey> : IEntityPrepper
    where TEntity : class, IEntity<TEntityKey>
{
    Task Prepare(TEntity modified, TEntity? original);
}