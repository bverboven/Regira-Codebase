namespace Regira.Entities.EFcore.Preppers.Abstractions;

public abstract class EntityPrepperBase<TEntity> : IEntityPrepper<TEntity>
    where TEntity : class
{
    public abstract Task Prepare(TEntity modified, TEntity? original);
    Task IEntityPrepper.Prepare(object modified, object? original)
        => Prepare((TEntity)modified, original as TEntity);
}