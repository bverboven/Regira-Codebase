using Microsoft.Extensions.DependencyInjection;
using Regira.Normalizing.Abstractions;
using Regira.Utilities;

namespace Regira.DAL.EFcore.Normalizing;

public static class ObjectNormalizerContainerExtensions
{
    public static void AddObjectNormalizingContainer(this IServiceCollection services)
    {
        services.AddObjectNormalizingContainer((p, o) => o.ExtractFromServiceCollection(services));
    }
    public static IServiceCollection AddObjectNormalizingContainer(this IServiceCollection services, Action<IServiceProvider, ObjectNormalizerContainer> configure)
    {
        return services.AddTransient(p =>
        {
            var container = new ObjectNormalizerContainer(p);
            configure(p, container);
            return container;
        });
    }
    public static void ExtractFromServiceCollection(this ObjectNormalizerContainer container, IServiceCollection services)
    {
        var normalizers = services.Where(s => TypeUtility.ImplementsInterface(s.ServiceType, typeof(IObjectNormalizer))).ToArray();
        if (!normalizers.Any())
        {
            return;
        }

        foreach (var descriptor in normalizers)
        {
            var entityType = descriptor.ServiceType.GetGenericArguments().FirstOrDefault();
            if (entityType != null)
            {
                container.Register(entityType, p => (IObjectNormalizer)p.GetRequiredService(descriptor.ServiceType));
            }
        }
    }
}