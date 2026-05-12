using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Mapping.Abstractions;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Services.Abstractions;
using Regira.Entities.Web.FastEndpoints.Endpoints.Models;
using Regira.Entities.Web.FastEndpoints.Extensions;
using Regira.Entities.Web.Models;
using System.Diagnostics;

namespace Regira.Entities.Web.FastEndpoints.Endpoints.Abstractions;

/// <summary>
/// Base endpoint for retrieving a single entity: <c>GET {BaseRoute}/{id}</c>
/// </summary>
public abstract class EntityDetailsEndpointBase<TEntity, TKey, TDto>
    : Endpoint<EntityIdRequest<TKey>, DetailsResult<TDto>>
    where TEntity : class, IEntity<TKey>
    where TDto : class
{
    protected abstract string BaseRoute { get; }

    public override void Configure()
    {
        Get($"{BaseRoute}/{{id}}");
    }

    public override async Task HandleAsync(EntityIdRequest<TKey> req, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

        var service = HttpContext.RequestServices.GetRequiredEntityService<IEntityService<TEntity, TKey>>();
        var item = await service.Details(req.Id, ct);
        if (item == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var mapper = HttpContext.RequestServices.GetRequiredService<IEntityMapper>();
        var model = mapper.Map<TDto>(item);
        sw.Stop();

        await Send.OkAsync(new DetailsResult<TDto> { Item = model, Duration = sw.ElapsedMilliseconds }, ct);
    }
}

/// <summary>
/// Convenience variant for entities with an <c>int</c> key.
/// </summary>
public abstract class EntityDetailsEndpointBase<TEntity, TDto>
    : EntityDetailsEndpointBase<TEntity, int, TDto>
    where TEntity : class, IEntity<int>
    where TDto : class;
