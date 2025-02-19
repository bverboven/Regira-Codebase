using System.Diagnostics;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Regira.DAL.Paging;
using Regira.Entities.Abstractions;
using Regira.Entities.Extensions;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Web.Models;
using Regira.Utilities;

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

        var service = ctrl.HttpContext.RequestServices.GetRequiredService<IEntityService<TEntity, TKey>>();
        var item = await service.Details(id);
        if (item == null)
        {
            return null;
        }

        var mapper = ctrl.HttpContext.RequestServices.GetRequiredService<IMapper>();
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

        var service = ctrl.HttpContext.RequestServices.GetRequiredService<IEntityService<TEntity, TKey>>();
        var items = await service.List(so, pagingInfo);

        var mapper = ctrl.HttpContext.RequestServices.GetRequiredService<IMapper>();
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

        var service = ctrl.HttpContext.RequestServices.GetRequiredService<IEntityService<TEntity, TKey, TSo, TSortBy, TIncludes>>();
        var items = await service
            .List(so, sortBy, includes.ToBitmask(), pagingInfo);

        var mapper = ctrl.HttpContext.RequestServices.GetRequiredService<IMapper>();
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
        var service = ctrl.HttpContext.RequestServices.GetRequiredService<IEntityService<TEntity, TKey>>();

        var sw = new Stopwatch();
        sw.Start();

        var count = await service.Count(so);

        IList<TEntity> items = count == 0
            ? Array.Empty<TEntity>()
            : await service.List(so, pagingInfo);

        var mapper = ctrl.HttpContext.RequestServices.GetRequiredService<IMapper>();
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
        var service = ctrl.HttpContext.RequestServices.GetRequiredService<IEntityService<TEntity, TKey, TSo, TSortBy, TIncludes>>();

        var sw = new Stopwatch();
        sw.Start();

        var count = await service.Count(so);

        IList<TEntity> items = count == 0
            ? Array.Empty<TEntity>()
            : await service.List(so, sortBy, includes.ToBitmask(), pagingInfo);

        var mapper = ctrl.HttpContext.RequestServices.GetRequiredService<IMapper>();
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
            var mapper = ctrl.HttpContext.RequestServices.GetRequiredService<IMapper>();
            var item = mapper.Map<TEntity>(model);
            var isNew = item.IsNew();

            var service = ctrl.HttpContext.RequestServices.GetRequiredService<IEntityService<TEntity, TKey>>();
            if (!isNew)
            {
                if (id != null && !id.Equals(default(TKey)))
                {
                    item.Id = id;
                }

                var exists = (await service.Count(new { item.Id })) == 1;
                if (!exists)
                {
                    return null;
                }
            }

            await service.Save(item);
            var affected = await service.SaveChanges();

            var savedItem = await service.Details(item.Id);
            var savedModel = mapper.Map<TDto>(savedItem)!;

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
        catch (Exception ex)
        {
            return ctrl.StatusCode(500, ex);
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

        var service = ctrl.HttpContext.RequestServices.GetRequiredService<IEntityService<TEntity, TKey>>();
        var item = (await service.List(new { id })).SingleOrDefault();
        if (item == null)
        {
            return null;
        }

        await service.Remove(item);
        await service.SaveChanges();

        var mapper = ctrl.HttpContext.RequestServices.GetRequiredService<IMapper>();
        var model = mapper.Map<TDto>(item);

        sw.Stop();

        return ctrl.DeleteResult(model, sw.ElapsedMilliseconds);
    }
}