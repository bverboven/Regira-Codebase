namespace Regira.DependencyInjection;

public interface ITypedServiceProvider<T, out TService>
{
    TService Provide();
}