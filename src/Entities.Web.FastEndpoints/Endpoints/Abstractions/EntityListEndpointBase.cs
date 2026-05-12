using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.Paging;
using Regira.Entities.Mapping.Abstractions;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Services.Abstractions;
using Regira.Entities.Web.FastEndpoints.Endpoints.Helpers;
using Regira.Entities.Web.FastEndpoints.Extensions;
using Regira.Entities.Web.Models;
using System.Diagnostics;

namespace Regira.Entities.Web.FastEndpoints.Endpoints.Abstractions;

/// <summary>
/// Base endpoint for listing entities: <c>GET {BaseRoute}</c>.
/// <para>
/// The <typeparamref name="TSearchObject"/> is bound from the query string.
/// Paging is driven by optional <c>page</c> and <c>pageSize</c> query parameters.
/// </para>
/// </summary>
public abstract class EntityListEndpointBase<TEntity, TKey, TSearchObject, TDto>
    : Endpoint<TSearchObject, ListResult<TDto>>
    where TEntity : class, IEntity<TKey>
    where TSearchObject : class, ISearchObject<TKey>, new()
    where TDto : class
{
    protected abstract string BaseRoute { get; }

    public override void Configure()
    {
        Get(BaseRoute);
    }

    public override async Task HandleAsync(TSearchObject so, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

        var pagingInfo = EntityEndpointHelper.ExtractPagingInfo(Query<int?>("page"), Query<int?>("pageSize"));

        var service = HttpContext.RequestServices.GetRequiredEntityService<IEntityService<TEntity, TKey>>();
        var items = await service.List(so, pagingInfo, ct);

        var mapper = HttpContext.RequestServices.GetRequiredService<IEntityMapper>();
        var models = mapper.Map<List<TDto>>(items);

        sw.Stop();

        await Send.OkAsync(new ListResult<TDto> { Items = models, Duration = sw.ElapsedMilliseconds }, ct);
    }
}

/// <summary>
/// Convenience variant using the default <see cref="SearchObject"/> and an <c>int</c> key.
/// </summary>
public abstract class EntityListEndpointBase<TEntity, TDto>
    : EntityListEndpointBase<TEntity, int, SearchObject, TDto>
    where TEntity : class, IEntity<int>
    where TDto : class;
