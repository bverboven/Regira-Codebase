namespace Regira.DependencyInjection;

public interface ITypedWrapper<out TService>
{
    TService Value { get; }
}

public class TypedWrapper<TDefiner, TService>(TService instance) : ITypedWrapper<TService>
{
    public TService Value => instance;
}