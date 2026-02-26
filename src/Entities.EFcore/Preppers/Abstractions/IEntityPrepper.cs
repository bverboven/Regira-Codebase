namespace Regira.Entities.EFcore.Preppers.Abstractions;

public interface IEntityPrepper
{
    Task Prepare(object modified, object? original);
}
public interface IEntityPrepper<in TEntity> : IEntityPrepper
{
    Task Prepare(TEntity modified, TEntity? original);
}