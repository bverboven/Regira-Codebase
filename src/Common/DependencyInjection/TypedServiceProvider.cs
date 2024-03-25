namespace Regira.DependencyInjection;

public class TypedServiceProvider<TDefiner, TService>(Func<TService> func) : ITypedServiceProvider<TDefiner, TService>
{
    public TService Provide()
        => func();
}
