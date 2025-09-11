namespace Regira.Entities.Abstractions;

public interface IEntityMapper
{
    TTarget Map<TTarget>(object source);

    TTarget Map<TSource, TTarget>(TSource source);

    TTarget Map<TSource, TTarget>(TSource source, TTarget target);
}