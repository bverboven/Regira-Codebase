using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Regira.Entities.Extensions;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Services.Abstractions;
using Regira.Entities.Web.FastEndpoints.Endpoints.Helpers;
using Regira.Entities.Web.FastEndpoints.Models;
using Regira.Entities.Web.Models;
using System.Reflection;

namespace Regira.Entities.Web.FastEndpoints.Extensions;

/// <summary>
/// Extension methods on <see cref="WebApplication"/> for auto-registering minimal-API CRUD
/// endpoints for every <see cref="IEntityService{TEntity,TKey}"/> found in the DI container.
/// </summary>
public static class WebApplicationEntityExtensions
{
    private static readonly MethodInfo _registerMethod =
        typeof(WebApplicationEntityExtensions)
            .GetMethod(nameof(MapCrudEndpointsFor), BindingFlags.Static | BindingFlags.NonPublic)!;

    /// <summary>
    /// Scans all <see cref="IEntityService{TEntity,TKey}"/> registrations in the DI container and
    /// automatically maps a set of minimal-API CRUD endpoints for each discovered entity type.
    /// <para>
    /// The following routes are registered per entity (using
    /// <c>{routePrefix}/{entityTypePlural}</c> as the base):
    /// <list type="bullet">
    ///   <item><c>GET  {base}/{id}</c> — Details</item>
    ///   <item><c>GET  {base}</c>      — List (supports <c>?page</c> / <c>?pageSize</c>)</item>
    ///   <item><c>POST {base}</c>      — Create</item>
    ///   <item><c>POST {base}/save</c> — Save (create or update)</item>
    ///   <item><c>PUT  {base}/{id}</c> — Modify</item>
    ///   <item><c>DELETE {base}/{id}</c> — Delete</item>
    /// </list>
    /// </para>
    /// <para>
    /// Requires <c>services.TryAddSingleton&lt;IServiceCollection&gt;(services)</c> to have been
    /// called during service registration (this is done automatically by
    /// <c>UseEntities&lt;TContext&gt;()</c>).
    /// </para>
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="routePrefix">Default route prefix (default: <c>"api"</c>).</param>
    /// <param name="configure">Optional callback to customise routes or add per-entity overrides.</param>
    public static WebApplication MapEntityEndpoints(
        this WebApplication app,
        string routePrefix = "",
        Action<EntityAutoEndpointsOptions>? configure = null)
    {
        var options = new EntityAutoEndpointsOptions { RoutePrefix = routePrefix };
        configure?.Invoke(options);

        var serviceCollection = app.Services.GetService<IServiceCollection>();
        if (serviceCollection == null)
        {
            app.Logger.LogWarning(
                "{Method} could not find {IServiceCollection} in the DI container. " +
                "Ensure UseEntities<TContext>() has been called during service registration.",
                nameof(MapEntityEndpoints), nameof(IServiceCollection));
            return app;
        }

        foreach (var (entityType, keyType) in GetEntityServiceRegistrations(serviceCollection))
        {
            var route = options.GetRouteFor(entityType);
            _registerMethod
                .MakeGenericMethod(entityType, keyType)
                .Invoke(null, [app, route]);
        }

        return app;
    }

    // ── Internal helpers ───────────────────────────────────────────────────────

    private static IEnumerable<(Type entityType, Type keyType)> GetEntityServiceRegistrations(
        IServiceCollection services)
    {
        var twoArgGeneric = typeof(IEntityService<,>);

        return services
            .Where(d =>
                d.ServiceType.IsGenericType &&
                d.ServiceType.GetGenericTypeDefinition() == twoArgGeneric)
            .Select(d => (
                entityType: d.ServiceType.GetGenericArguments()[0],
                keyType: d.ServiceType.GetGenericArguments()[1]))
            .DistinctBy(x => x.entityType);
    }

    /// <summary>
    /// Registers all CRUD minimal-API endpoints for a single <typeparamref name="TEntity"/>
    /// under <paramref name="route"/>.
    /// </summary>
    private static void MapCrudEndpointsFor<TEntity, TKey>(WebApplication app, string route)
        where TEntity : class, IEntity<TKey>
    {
        var group = app.MapGroup(route);

        // ── GET /{id} — Details ────────────────────────────────────────────────
        group.MapGet("/{id}", async (TKey id,
            IEntityService<TEntity, TKey> service,
            CancellationToken ct) =>
        {
            var item = await service.Details(id, ct);
            return item == null
                ? Results.NotFound()
                : Results.Ok(new DetailsResult<TEntity> { Item = item });
        });

        // ── GET / — List ───────────────────────────────────────────────────────
        group.MapGet("/", async (
            IEntityService<TEntity, TKey> service,
            CancellationToken ct,
            int? page = null,
            int? pageSize = null) =>
        {
            var pagingInfo = EntityEndpointHelper.ExtractPagingInfo(page, pageSize);
            var items = await service.List((object?)null, pagingInfo, ct);
            return Results.Ok(new ListResult<TEntity> { Items = (IList<TEntity>)items });
        });

        // ── POST / — Create ────────────────────────────────────────────────────
        group.MapPost("/", async (TEntity input,
            IEntityService<TEntity, TKey> service,
            CancellationToken ct) =>
        {
            try
            {
                var isNew = input.IsNew();
                await service.Save(input, ct);
                var affected = await service.SaveChanges(ct);
                var saved = await service.Details(input.Id, ct);
                return Results.Ok(new SaveResult<TEntity> { Item = saved, Affected = affected, IsNew = isNew });
            }
            catch (EntityInputException<TEntity> ex)
            {
                return Results.ValidationProblem(
                    ex.InputErrors.ToDictionary(x => x.Key, x => new[] { x.Value }));
            }
        });

        // ── POST /save — Save (create or update) ───────────────────────────────
        group.MapPost("/save", async (TEntity input,
            IEntityService<TEntity, TKey> service,
            CancellationToken ct) =>
        {
            try
            {
                var isNew = input.IsNew();
                if (!isNew)
                {
                    var exists = await service.Count(new { input.Id }, ct) == 1;
                    if (!exists)
                    {
                        return Results.NotFound();
                    }
                }

                await service.Save(input, ct);
                var affected = await service.SaveChanges(ct);
                var saved = await service.Details(input.Id, ct);
                return Results.Ok(new SaveResult<TEntity> { Item = saved, Affected = affected, IsNew = isNew });
            }
            catch (EntityInputException<TEntity> ex)
            {
                return Results.ValidationProblem(
                    ex.InputErrors.ToDictionary(x => x.Key, x => new[] { x.Value }));
            }
        });

        // ── PUT /{id} — Modify ─────────────────────────────────────────────────
        group.MapPut("/{id}", async (TKey id,
            TEntity input,
            IEntityService<TEntity, TKey> service,
            CancellationToken ct) =>
        {
            input.Id = id;
            var exists = await service.Count(new { id }, ct) == 1;
            if (!exists)
            {
                return Results.NotFound();
            }

            try
            {
                await service.Save(input, ct);
                var affected = await service.SaveChanges(ct);
                var saved = await service.Details(id, ct);
                return Results.Ok(new SaveResult<TEntity> { Item = saved, Affected = affected, IsNew = false });
            }
            catch (EntityInputException<TEntity> ex)
            {
                return Results.ValidationProblem(
                    ex.InputErrors.ToDictionary(x => x.Key, x => new[] { x.Value }));
            }
        });

        // ── DELETE /{id} — Delete ──────────────────────────────────────────────
        group.MapDelete("/{id}", async (TKey id,
            IEntityService<TEntity, TKey> service,
            CancellationToken ct) =>
        {
            var item = await service.Details(id, ct);
            if (item == null)
            {
                return Results.NotFound();
            }

            await service.Remove(item, ct);
            await service.SaveChanges(ct);
            return Results.Ok(new DeleteResult<TEntity> { Item = item });
        });
    }
}
