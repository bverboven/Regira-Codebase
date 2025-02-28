using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.Preppers.Abstractions;

public interface IEntityPrepper<in TEntity, TEntityKey>
    where TEntity : class, IEntity<TEntityKey>
{
    Task Prepare(TEntity modified, TEntity? original);
}