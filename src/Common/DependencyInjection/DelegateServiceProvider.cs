namespace Regira.DependencyInjection;

public class DelegateServiceProvider<T>
{
    public static ITypedServiceProvider<T, TService> Create<TService>(IServiceProvider p, Func<IServiceProvider, TService> func)
        => new TypedServiceProvider<T, TService>(() => func(p));
}