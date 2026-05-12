namespace Regira.Entities.Web.FastEndpoints.Endpoints.Models;

/// <summary>
/// Simple request model that binds the route parameter <c>{id}</c> for endpoints
/// that only need an entity identifier (Details, Delete).
/// </summary>
public class EntityIdRequest<TKey>
{
    public TKey Id { get; set; } = default!;
}
