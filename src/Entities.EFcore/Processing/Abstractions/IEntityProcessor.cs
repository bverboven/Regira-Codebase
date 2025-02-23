namespace Regira.Entities.EFcore.Processing.Abstractions;

public interface IEntityProcessor
{
    Task Process<TEntity>(IList<TEntity> items);
}

public interface IEntityProcessor<TEntity> : IEntityProcessor
{
    Task Process(IList<TEntity> items);
}
