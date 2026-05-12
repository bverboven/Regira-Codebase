using FastEndpoints;
using FluentValidation.Results;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Web.FastEndpoints.Endpoints.Helpers;
using Regira.Entities.Web.Models;

namespace Regira.Entities.Web.FastEndpoints.Endpoints.Abstractions;

/// <summary>
/// Base endpoint for saving (create or update) an entity: <c>POST {BaseRoute}/save</c>.
/// <para>
/// When the mapped entity already has a non-default key the endpoint verifies its existence
/// and returns <c>404</c> if not found.
/// </para>
/// </summary>
public abstract class EntitySaveEndpointBase<TEntity, TKey, TDto, TInputDto>
    : Endpoint<TInputDto, SaveResult<TDto>>
    where TEntity : class, IEntity<TKey>
    where TInputDto : class
    where TDto : class
{
    protected abstract string BaseRoute { get; }

    public override void Configure()
    {
        Post($"{BaseRoute}/save");
    }

    public override async Task HandleAsync(TInputDto model, CancellationToken ct)
    {
        var (result, errors) = await EntityEndpointHelper.SaveAsync<TEntity, TKey, TDto, TInputDto>(
            HttpContext.RequestServices, model, default, ct);

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
public abstract class EntitySaveEndpointBase<TEntity, TDto, TInputDto>
    : EntitySaveEndpointBase<TEntity, int, TDto, TInputDto>
    where TEntity : class, IEntity<int>
    where TInputDto : class
    where TDto : class;
