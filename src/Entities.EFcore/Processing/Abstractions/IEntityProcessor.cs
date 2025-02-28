namespace Regira.Entities.EFcore.Processing.Abstractions;

//public interface IEntityProcessor
//{
//    Task Process<TEntity,TIncludes>(IList<TEntity> items, TIncludes? includes)
//        where TIncludes : struct, Enum;
//}

public interface IEntityProcessor<TEntity, TIncludes> //: IEntityProcessor
where TIncludes : struct, Enum
{
    Task Process(IList<TEntity> items, TIncludes? includes);
}
