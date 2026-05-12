using FastEndpoints;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Web.FastEndpoints.Endpoints.Helpers;
using Regira.Entities.Web.Models;

namespace Regira.Entities.Web.FastEndpoints.Endpoints.Abstractions;

/// <summary>
/// Base endpoint for modifying an existing entity: <c>PUT {BaseRoute}/{id}</c>.
/// <para>
/// The <c>{id}</c> route parameter is read via <see cref="Route{T}"/>;
/// the request body is deserialized as <typeparamref name="TInputDto"/>.
/// Returns <c>404</c> when the entity does not exist.
/// </para>
/// </summary>
public abstract class EntityModifyEndpointBase<TEntity, TKey, TDto, TInputDto>
    : EndpointWithoutRequest<SaveResult<TDto>>
    where TEntity : class, IEntity<TKey>
    where TInputDto : class
    where TDto : class
{
    protected abstract string BaseRoute { get; }

    public override void Configure()
    {
        Put($"{BaseRoute}/{{id}}");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<TKey>("id")!;
        var model = await HttpContext.Request.ReadFromJsonAsync<TInputDto>(ct);
        if (model == null)
        {
            await Send.ErrorsAsync(400, ct);
            return;
        }

        var (result, errors) = await EntityEndpointHelper.SaveAsync<TEntity, TKey, TDto, TInputDto>(
            HttpContext.RequestServices, model, id, ct);

        if (errors != null)
        {
            foreach (var error in errors)
            {
                AddError(new ValidationFailure(error.Key, error.Value));
            }

            await Send.ErrorsAsync(400, ct);
            return;
        }

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
public abstract class EntityModifyEndpointBase<TEntity, TDto, TInputDto>
    : EntityModifyEndpointBase<TEntity, int, TDto, TInputDto>
    where TEntity : class, IEntity<int>
    where TInputDto : class
    where TDto : class;
