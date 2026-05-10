using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.Paging;
using Regira.Entities.Extensions;
using Regira.Entities.Mapping.Abstractions;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Services.Abstractions;
using Regira.Entities.Web.Models;
using Regira.Utilities;
using System.Diagnostics;

namespace Regira.Entities.Web.Controllers;

public static class ControllerExtensions
{
    // Details
    public static OkObjectResult DetailsResult<TDto>(this ControllerBase _, TDto item, long? duration = null) =>
        new(new DetailsResult<TDto> { Item = item, Duration = duration });

    public static Task<ActionResult<DetailsResult<TDto>>?> Details<TEntity, TDto>(this ControllerBase ctrl, int id)
        where TEntity : class, IEntity<int>
        => ctrl.Details<TEntity, int, TDto>(id);
    public static async Task<ActionResult<DetailsResult<TDto>>?> Details<TEntity, TKey, TDto>(this ControllerBase ctrl, TKey id)
        where TEntity : class, IEntity<TKey>
    {
        var sw = new Stopwatch();
        sw.Start();

        var service = ctrl.GetRequiredEntityService<IEntityService<TEntity, TKey>>();
        var item = await service.Details(id);
        if (item == null)
        {
            return null;
        }

        var mapper = ctrl.HttpContext.RequestServices.GetRequiredService<IEntityMapper>();
        var model = mapper.Map<TDto>(item);
        sw.Stop();
        return ctrl.DetailsResult(model, sw.ElapsedMilliseconds);
    }

    // List
    public static OkObjectResult ListResult<TDto>(this ControllerBase _, IList<TDto> items, long? duration = null) =>
        new(new ListResult<TDto> { Items = items, Duration = duration });
    // simple
    public static async Task<ActionResult<ListResult<TDto>>> List<TEntity, TKey, TSearchObject, TDto>(this ControllerBase ctrl, TSearchObject? so = null, PagingInfo? pagingInfo = null)
        where TEntity : class, IEntity<TKey>
        where TSearchObject : class, ISearchObject<TKey>
    {
        var sw = new Stopwatch();
        sw.Start();

        var service = ctrl.GetRequiredEntityService<IEntityService<TEntity, TKey>>();
        var items = await service.List(so, pagingInfo);

        var mapper = ctrl.HttpContext.RequestServices.GetRequiredService<IEntityMapper>();
        var models = mapper.Map<List<TDto>>(items);

        sw.Stop();
        return ctrl.ListResult(models, sw.ElapsedMilliseconds);
    }
    // complex
    public static async Task<ActionResult<ListResult<TDto>>> List<TEntity, TKey, TSo, TSortBy, TIncludes, TDto>(this ControllerBase ctrl,
        TSo[] so, PagingInfo pagingInfo, TIncludes[] includes, TSortBy[] sortBy)
        where TEntity : class, IEntity<TKey>
        where TSo : class, ISearchObject<TKey>, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
    {
        var sw = new Stopwatch();
        sw.Start();

        var service = ctrl.GetRequiredEntityService<IEntityService<TEntity, TKey, TSo, TSortBy, TIncludes>>();
        var items = await service
            .List(so, sortBy, includes.ToBitmask(), pagingInfo);

        var mapper = ctrl.HttpContext.RequestServices.GetRequiredService<IEntityMapper>();
        var models = mapper.Map<List<TDto>>(items);

        sw.Stop();
        return ctrl.ListResult(models, sw.ElapsedMilliseconds);
    }

    // Search
    public static OkObjectResult SearchResult<TDto>(this ControllerBase _, IList<TDto> items, long count, long? duration = null) =>
        new(new SearchResult<TDto> { Items = items, Count = count, Duration = duration });
    // simple
    public static async Task<ActionResult<SearchResult<TDto>>> Search<TEntity, TKey, TDto>(this ControllerBase ctrl, SearchObject<TKey>? so = null, PagingInfo? pagingInfo = null)
        where TEntity : class, IEntity<TKey>
    {
        var service = ctrl.GetRequiredEntityService<IEntityService<TEntity, TKey>>();

        var sw = new Stopwatch();
        sw.Start();

        var count = await service.Count(so);

        IList<TEntity> items = count == 0
            ? Array.Empty<TEntity>()
            : await service.List(so, pagingInfo);

        var mapper = ctrl.HttpContext.RequestServices.GetRequiredService<IEntityMapper>();
        var models = mapper.Map<List<TDto>>(items);

        sw.Stop();
        return ctrl.SearchResult(models, count, sw.ElapsedMilliseconds);

    }
    // complex
    public static async Task<ActionResult<SearchResult<TDto>>> Search<TEntity, TKey, TSo, TSortBy, TIncludes, TDto>(this ControllerBase ctrl,
        TSo[] so, PagingInfo pagingInfo, TIncludes[] includes, TSortBy[] sortBy)
        where TEntity : class, IEntity<TKey>
        where TSo : class, ISearchObject<TKey>, new()
        where TSortBy : struct, Enum
        where TIncludes : struct, Enum
    {
        var service = ctrl.GetRequiredEntityService<IEntityService<TEntity, TKey, TSo, TSortBy, TIncludes>>();

        var sw = new Stopwatch();
        sw.Start();

        var count = await service.Count(so);

        IList<TEntity> items = count == 0
            ? Array.Empty<TEntity>()
            : await service.List(so, sortBy, includes.ToBitmask(), pagingInfo);

        var mapper = ctrl.HttpContext.RequestServices.GetRequiredService<IEntityMapper>();
        var models = mapper.Map<List<TDto>>(items);

        sw.Stop();
        return ctrl.SearchResult(models, count, sw.ElapsedMilliseconds);
    }

    // Save
    public static OkObjectResult SaveResult<TDto>(this ControllerBase _, TDto item, int affected, bool isNew, long? duration = null) =>
        new(new SaveResult<TDto> { Item = item, Affected = affected, IsNew = isNew, Duration = duration });
    public static async Task<ActionResult<SaveResult<TDto>>?> Save<TEntity, TKey, TDto, TInputDto>(this ControllerBase ctrl, TInputDto model, TKey? id = default)
        where TEntity : class, IEntity<TKey>
    {
        var sw = new Stopwatch();
        sw.Start();

        try
        {
            var mapper = ctrl.HttpContext.RequestServices.GetRequiredService<IEntityMapper>();
            var item = mapper.Map<TEntity>(model!);
            if (!id?.Equals(default(TKey)) ?? false)
            {
                item.Id = id;
            }
            var isNew = item.IsNew();

            var service = ctrl.GetRequiredEntityService<IEntityService<TEntity, TKey>>();
            if (!isNew)
            {
                var exists = await service.Count(new { item.Id }) == 1;
                if (!exists)
                {
                    return null;
                }
            }

            await service.Save(item);
            var affected = await service.SaveChanges();

            var savedItem = await service.Details(item.Id);
            var savedModel = mapper.Map<TDto>(savedItem!);

            sw.Stop();

            return ctrl.SaveResult(savedModel, affected, isNew, sw.ElapsedMilliseconds);
        }
        catch (EntityInputException<TEntity> ex)
        {
            foreach (var error in ex.InputErrors)
            {
                ctrl.ModelState.AddModelError(error.Key, error.Value);
            }

            return ctrl.BadRequest(ctrl.ModelState);
        }
    }
    // Delete
    public static OkObjectResult DeleteResult<TDto>(this ControllerBase _, TDto item, long? duration = null) =>
        new(new SaveResult<TDto> { Item = item, Duration = duration });
    public static async Task<ActionResult<DeleteResult<TDto>>?> Delete<TEntity, TKey, TDto>(this ControllerBase ctrl, TKey id)
        where TEntity : class, IEntity<TKey>
    {
        var sw = new Stopwatch();
        sw.Start();

        var service = ctrl.GetRequiredEntityService<IEntityService<TEntity, TKey>>();
        var item = (await service.List(new { id })).SingleOrDefault();
        if (item == null)
        {
            return null;
        }

        await service.Remove(item);
        await service.SaveChanges();

        var mapper = ctrl.HttpContext.RequestServices.GetRequiredService<IEntityMapper>();
        var model = mapper.Map<TDto>(item);

        sw.Stop();

        return ctrl.DeleteResult(model, sw.ElapsedMilliseconds);
    }

    public static TService GetRequiredEntityService<TService>(this ControllerBase ctrl)
        where TService : notnull
    {
        try
        {
            return ctrl.HttpContext.RequestServices.GetRequiredService<TService>();
        }
        catch (InvalidOperationException ex)
        {
            var requestedType = typeof(TService);

            if (requestedType.IsGenericType)
            {
                var services = ctrl.HttpContext.RequestServices.GetService<IServiceCollection>();
                if (services != null)
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

                    var registered = services
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
                            $"Make sure all generic parameters in .For<>() exactly match what the controller extension is requesting.",
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