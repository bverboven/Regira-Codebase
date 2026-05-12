using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Mapping.Abstractions;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Services.Abstractions;
using Regira.Entities.Web.FastEndpoints.Endpoints.Helpers;
using Regira.Entities.Web.FastEndpoints.Extensions;
using Regira.Entities.Web.Models;
using Regira.Utilities;
using System.Diagnostics;

namespace Regira.Entities.Web.FastEndpoints.Endpoints.Abstractions;

/// <summary>
/// Base endpoint for listing entities with sort/includes support using a body array of search
/// objects: <c>POST {BaseRoute}/list</c>.
/// <para>
/// The request body must be a JSON array of <typeparamref name="TSo"/>.
/// <c>sortBy</c> and <c>includes</c> are repeated query parameters whose string values are
/// parsed as the respective enum types.
/// Paging uses optional <c>page</c> and <c>pageSize</c> query parameters.
/// </para>
/// </summary>
public abstract class ComplexEntityListPostEndpointBase<TEntity, TKey, TSo, TSortBy, TIncludes, TDto>
    : EndpointWithoutRequest<ListResult<TDto>>
    where TEntity : class, IEntity<TKey>
    where TSo : class, ISearchObject<TKey>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
    where TDto : class
{
    protected abstract string BaseRoute { get; }

    public override void Configure()
    {
        Post($"{BaseRoute}/list");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

        var so = await HttpContext.Request.ReadFromJsonAsync<TSo[]>(ct) ?? [];
        var pagingInfo = EntityEndpointHelper.ExtractPagingInfo(Query<int?>("page"), Query<int?>("pageSize"));
        var includes = EntityEndpointHelper.ParseEnumValues<TIncludes>(HttpContext.Request.Query["includes"]!);
        var sortBy = EntityEndpointHelper.ParseEnumValues<TSortBy>(HttpContext.Request.Query["sortBy"]!);

        var service = HttpContext.RequestServices.GetRequiredEntityService<IEntityService<TEntity, TKey, TSo, TSortBy, TIncludes>>();
        var items = await service.List(so, sortBy, includes.ToBitmask(), pagingInfo, ct);

        var mapper = HttpContext.RequestServices.GetRequiredService<IEntityMapper>();
        var models = mapper.Map<List<TDto>>(items);

        sw.Stop();

        await Send.OkAsync(new ListResult<TDto> { Items = models, Duration = sw.ElapsedMilliseconds }, ct);
    }
}

/// <summary>
/// Convenience variant for entities with an <c>int</c> key.
/// </summary>
public abstract class ComplexEntityListPostEndpointBase<TEntity, TSo, TSortBy, TIncludes, TDto>
    : ComplexEntityListPostEndpointBase<TEntity, int, TSo, TSortBy, TIncludes, TDto>
    where TEntity : class, IEntity<int>
    where TSo : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
    where TDto : class;
