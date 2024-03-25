namespace Regira.DependencyInjection;

public interface ITypedServiceProvider<T, TService>
{
    TService Provide();
}
