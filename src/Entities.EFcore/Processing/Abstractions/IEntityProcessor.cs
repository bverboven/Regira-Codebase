namespace Regira.Entities.EFcore.Processing.Abstractions;

public interface IEntityProcessor<TEntity, TIncludes>
    where TIncludes : struct, Enum
{
    Task Process(IList<TEntity> items, TIncludes? includes);
}
