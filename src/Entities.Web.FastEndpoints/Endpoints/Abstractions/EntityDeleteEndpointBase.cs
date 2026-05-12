using FastEndpoints;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Web.FastEndpoints.Endpoints.Helpers;
using Regira.Entities.Web.FastEndpoints.Endpoints.Models;
using Regira.Entities.Web.Models;

namespace Regira.Entities.Web.FastEndpoints.Endpoints.Abstractions;

/// <summary>
/// Base endpoint for deleting an entity: <c>DELETE {BaseRoute}/{id}</c>.
/// Returns <c>404</c> when the entity does not exist.
/// </summary>
public abstract class EntityDeleteEndpointBase<TEntity, TKey, TDto>
    : Endpoint<EntityIdRequest<TKey>, DeleteResult<TDto>>
    where TEntity : class, IEntity<TKey>
    where TDto : class
{
    protected abstract string BaseRoute { get; }

    public override void Configure()
    {
        Delete($"{BaseRoute}/{{id}}");
    }

    public override async Task HandleAsync(EntityIdRequest<TKey> req, CancellationToken ct)
    {
        var result = await EntityEndpointHelper.DeleteAsync<TEntity, TKey, TDto>(
            HttpContext.RequestServices, req.Id, ct);

        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }
}

/// <summary>
/// Convenience variant for entities with an <c>int</c> key.
/// </summary>
public abstract class EntityDeleteEndpointBase<TEntity, TDto>
    : EntityDeleteEndpointBase<TEntity, int, TDto>
    where TEntity : class, IEntity<int>
    where TDto : class;
