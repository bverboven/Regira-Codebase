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

public abstract class EntityPrepperBase<TEntity, TKey> : IEntityPrepper<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    public abstract Task Prepare(TEntity modified, TEntity? original);
    Task IEntityPrepper.Prepare(object modified, object? original)
        => Prepare((TEntity)modified, original as TEntity);
}