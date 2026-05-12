using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Services.Abstractions;

namespace Regira.Entities.Web.FastEndpoints.Extensions;

public static class EndpointExtensions
{
    /// <summary>
    /// Resolves an entity service from the DI container, producing descriptive error messages
    /// when the registration is missing or mismatched — equivalent to
    /// <c>ControllerExtensions.GetRequiredEntityService</c>.
    /// </summary>
    public static TService GetRequiredEntityService<TService>(this IServiceProvider services)
        where TService : notnull
    {
        try
        {
            return services.GetRequiredService<TService>();
        }
        catch (InvalidOperationException ex)
        {
            var requestedType = typeof(TService);

            if (requestedType.IsGenericType)
            {
                var serviceCollection = services.GetService<IServiceCollection>();
                if (serviceCollection != null)
                {
                    var entityType = requestedType.GetGenericArguments()[0];

                    var entityServiceOpenGenerics = new HashSet<Type>
                    {
                        typeof(IEntityService<>),
                        typeof(IEntityService<,>),
                        typeof(IEntityService<,,>),
                        typeof(IEntityService<,,,>),
                        typeof(IEntityService<,,,,>)
                    };

                    var registered = serviceCollection
                        .Where(d =>
                            d.ServiceType.IsGenericType
                            && entityServiceOpenGenerics.Contains(d.ServiceType.GetGenericTypeDefinition())
                            && d.ServiceType.GetGenericArguments()[0] == entityType)
                        .Select(d => d.ServiceType.Name + FormatTypeArgs(d.ServiceType))
                        .Distinct()
                        .ToList();

                    if (registered.Count > 0)
                    {
                        throw new InvalidOperationException(
                            $"No service of type '{requestedType.Name}{FormatTypeArgs(requestedType)}' was registered. " +
                            $"The following IEntityService registrations exist for '{entityType.Name}': " +
                            string.Join(", ", registered) + ". " +
                            $"Make sure all generic parameters in .For<>() exactly match what the endpoint extension is requesting.",
                            ex);
                    }

                    throw new InvalidOperationException(
                        $"No service of type '{requestedType.Name}{FormatTypeArgs(requestedType)}' was registered, " +
                        $"and no entity services for '{entityType.Name}' were found. " +
                        $"Register it via .For<{entityType.Name}>() or an appropriate overload.",
                        ex);
                }
            }

            throw new InvalidOperationException(
                $"No service of type '{requestedType.Name}' was registered. " +
                $"Register entity services using .For<>() with matching generic type parameters.",
                ex);
        }
    }

    private static string FormatTypeArgs(Type type) =>
        type.IsGenericType
            ? "<" + string.Join(", ", type.GetGenericArguments().Select(a => a.Name)) + ">"
            : string.Empty;
}
