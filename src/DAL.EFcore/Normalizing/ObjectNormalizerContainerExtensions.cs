using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.EFcore.Extensions;
using Regira.Normalizing.Abstractions;

namespace Regira.DAL.EFcore.Normalizing;

public static class ObjectNormalizerContainerExtensions
{
    public static void AddObjectNormalizingContainer(this IServiceCollection services)
        => services.AddObjectNormalizingContainer((_, o) => o.ExtractFromServiceCollection(services));

    public static IServiceCollection AddObjectNormalizingContainer(this IServiceCollection services, Action<IServiceProvider, ObjectNormalizerContainer> configure)
        => services.AddTransient(p =>
        {
            var container = new ObjectNormalizerContainer(p);
            configure(p, container);
            return container;
        });

    public static void ExtractFromServiceCollection(this ObjectNormalizerContainer container, IServiceCollection services)
    {
        var normalizers = services.CollectDescriptors<IObjectNormalizer>();
        foreach (var descriptor in normalizers)
        {
            var entityType = descriptor.ServiceType.GetGenericArguments().FirstOrDefault() ?? typeof(object);
            container.Register(entityType, p => (IObjectNormalizer)p.GetRequiredService(descriptor.ServiceType));
        }
    }
}