using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Web.FastEndpoints.Models;

/// <summary>
/// Configuration for <see cref="Extensions.WebApplicationEntityExtensions.MapEntityCrudEndpoints"/>.
/// </summary>
public class EntityAutoEndpointsOptions
{
    private readonly Dictionary<Type, string> _entityRoutes = new();

    /// <summary>
    /// Route prefix prepended to every auto-registered route.
    /// Defaults to <c>"api"</c>.
    /// </summary>
    public string RoutePrefix { get; set; } = "api";

    /// <summary>
    /// Explicitly sets the base route for <typeparamref name="TEntity"/>.
    /// When omitted the default convention <c>{RoutePrefix}/{entityName}s</c> is used.
    /// </summary>
    public EntityAutoEndpointsOptions For<TEntity>(string route)
        where TEntity : class, IEntity
    {
        _entityRoutes[typeof(TEntity)] = route;
        return this;
    }

    /// <summary>
    /// Returns the configured route for <paramref name="entityType"/>, falling back to the
    /// <c>{RoutePrefix}/{entityName}s</c> convention when no override is registered.
    /// </summary>
    public string GetRouteFor(Type entityType)
        => _entityRoutes.TryGetValue(entityType, out var route)
            ? route
            : $"{RoutePrefix}/{Pluralize(entityType.Name.ToLowerInvariant())}";

    private static string Pluralize(string name)
        => name.EndsWith('s') ? name : $"{name}s";
}
