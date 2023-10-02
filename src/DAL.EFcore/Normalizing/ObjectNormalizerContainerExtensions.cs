using Microsoft.Extensions.DependencyInjection;
using Regira.Normalizing.Abstractions;
using Regira.Utilities;

namespace Regira.DAL.EFcore.Normalizing;

public static class ObjectNormalizerContainerExtensions
{
    public static IServiceCollection AddObjectNormalizingContainer(this IServiceCollection services, Action<IServiceProvider, ObjectNormalizerContainer> configure)
    {
        return services.AddTransient(p =>
        {
            var container = new ObjectNormalizerContainer();
            configure(p, container);
            return container;
        });
    }

    public static void ExtractFromServiceCollection(this ObjectNormalizerContainer container, IServiceCollection services)
    {
        var normalizers = services.Where(s => TypeUtility.ImplementsInterface<IObjectNormalizer>(s.ServiceType)).ToArray();
        if (!normalizers.Any())
        {
            return;
        }

        var serviceProvider = services.BuildServiceProvider();
        foreach (var descriptor in normalizers)
        {
            var entityType = descriptor.ServiceType.GetGenericArguments().FirstOrDefault();
            if (entityType != null)
            {
                container.Register(entityType, (IObjectNormalizer)serviceProvider.GetRequiredService(descriptor.ServiceType));
            }
        }
    }
}