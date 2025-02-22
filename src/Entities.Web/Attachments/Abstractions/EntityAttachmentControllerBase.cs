using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Regira.DAL.Paging;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Extensions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Web.Attachments.Models;
using Regira.Entities.Web.Controllers;
using Regira.Entities.Web.Models;
using Regira.Web.IO;
using System.Diagnostics;
using static Regira.Web.Extensions.ControllerExtensions;

namespace Regira.Entities.Web.Attachments.Abstractions;

public abstract class EntityAttachmentControllerBase<TEntity> : EntityAttachmentControllerBase<TEntity, EntityAttachmentDto, EntityAttachmentInputDto>
    where TEntity : class, IEntityAttachment<int, int, int, Attachment>, IEntity<int>;
public abstract class EntityAttachmentControllerBase<TEntity, TDto, TInputDto> : ControllerBase
    where TEntity : class, IEntityAttachment<int, int, int, Attachment>, IEntity<int>
    where TInputDto : class, IEntityAttachmentInput
{
    // Details
    [HttpGet("attachments/{id}")]
    public virtual async Task<ActionResult<DetailsResult<TDto>>> Details([FromRoute] int id)
        => await this.Details<TEntity, int, TDto>(id) ?? NotFound();
    // List
    [HttpGet("attachments")]
    public virtual Task<ActionResult<ListResult<TDto>>> List([FromQuery] EntityAttachmentSearchObject so, [FromQuery] PagingInfo? pagingInfo = null)
        => this.List<TEntity, int, EntityAttachmentSearchObject, TDto>(so, pagingInfo);
    [HttpGet("{objectId}/attachments")]
    public virtual Task<ActionResult<ListResult<TDto>>> List([FromRoute] int objectId, [FromQuery] EntityAttachmentSearchObject so, [FromQuery] PagingInfo? pagingInfo = null)
    {
        so.ObjectId = [objectId];
        return List(so, pagingInfo);
    }

    // Save (Update)
    [HttpPut("{objectId}/attachments/{id}")]
    public virtual async Task<ActionResult<SaveResult<TDto>>?> Update([FromRoute] int objectId, [FromRoute] int id, [FromBody] TInputDto model)
    {
        var sw = new Stopwatch();
        sw.Start();

        try
        {
            var mapper = HttpContext.RequestServices.GetRequiredService<IMapper>();
            var item = mapper.Map<TEntity>(model);

            var service = HttpContext.RequestServices.GetRequiredService<IEntityService<TEntity>>();
            var original = await FetchItem(item.Id);
            if (original == null)
            {
                return null;
            }

            await service.Save(item);
            var affected = await service.SaveChanges();

            var savedItem = await FetchItem(item.Id);
            var savedModel = mapper.Map<TDto>(savedItem)!;

            sw.Stop();

            return this.SaveResult(savedModel, affected, false, sw.ElapsedMilliseconds);
        }
        catch (EntityInputException<TEntity> ex)
        {
            foreach (var error in ex.InputErrors)
            {
                ModelState.AddModelError(error.Key, error.Value);
            }

            return BadRequest(ModelState);
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<EntityAttachmentControllerBase<TEntity>>>();
            logger.LogError(ex, "Updating entity failed");
            throw;
        }
    }

    // Delete
    [HttpDelete("attachments/{id}")]
    public virtual async Task<ActionResult<DeleteResult<TDto>>?> Delete([FromRoute] int id)
        => await this.Delete<TEntity, int, TDto>(id) ?? NotFound();

    // Download
    [HttpGet("files/{id}")]
    public virtual async Task<IActionResult> GetFile([FromRoute] int id, bool inline = true)
    {
        var service = HttpContext.RequestServices.GetRequiredService<IEntityService<TEntity>>();
        var item = await service.Details(id);

        if (item == null)
        {
            return NotFound();
        }

        return this.File(item.Attachment!, inline);
    }
    [HttpGet("{objectId}/files/{filename}")]
    public virtual async Task<IActionResult> GetFile([FromRoute] int objectId, [FromRoute] string fileName, bool inline = true)
    {
        var decodedFileName = Uri.UnescapeDataString(fileName);// necessary for comparing file names with subfolders (%2F, ...)
        var service = HttpContext.RequestServices.GetRequiredService<IEntityService<TEntity>>();
        var items = await service.List(new { objectId = new[] { objectId }, fileName = decodedFileName }, new PagingInfo { PageSize = 1 });

        if (!items.Any())
        {
            return NotFound();
        }

        return await GetFile(items.First().Id);
    }
    // Upload
    [HttpPost("{objectId}/files")]
    public virtual async Task<ActionResult<SaveResult<TDto>>> Add([FromRoute] int objectId, IFormFile file, [FromForm] TInputDto model)
    {
        var sw = new Stopwatch();
        sw.Start();

        var service = HttpContext.RequestServices.GetRequiredService<IEntityService<TEntity>>();
        var mapper = HttpContext.RequestServices.GetRequiredService<IMapper>();

        var item = mapper.Map<TEntity>(model);
        item.ObjectId = objectId;
        item.Attachment = file.ToNamedFile().ToAttachment();

        await service.Save(item);
        var affected = await service.SaveChanges();
        var savedModel = mapper.Map<TDto>(item);

        sw.Stop();

        return this.SaveResult(savedModel, affected, true, sw.ElapsedMilliseconds);
    }
    [HttpPut("{objectId}/files/{id}")]
    public virtual async Task<ActionResult<SaveResult<TDto>>> Modify([FromRoute] int objectId, [FromRoute] int id, IFormFile file, [FromForm] TInputDto model)
    {
        var sw = new Stopwatch();
        sw.Start();

        var service = HttpContext.RequestServices.GetRequiredService<IEntityService<TEntity>>();
        var mapper = HttpContext.RequestServices.GetRequiredService<IMapper>();

        var original = (await service.List(new { id }, new PagingInfo { PageSize = 1 })).SingleOrDefault();
        if (original == null)
        {
            return NotFound();
        }
        if (original.ObjectId != objectId)
        {
            return BadRequest($"Bad {nameof(original.ObjectId)}");
        }

        original.ObjectId = objectId;
        original.Attachment = file.ToNamedFile().ToAttachment(original.Attachment);
#if NETSTANDARD2_0
        using var fileStream = file.OpenReadStream();
#else
        await using var fileStream = file.OpenReadStream();
#endif

        await service.Save(original);
        var affected = await service.SaveChanges();
        var savedModel = mapper.Map<TDto>(original);

        sw.Stop();

        return this.SaveResult(savedModel, affected, false, sw.ElapsedMilliseconds);
    }


    /// <summary>
    /// Fetches item with related Attachment, but without file contents
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    protected async Task<TEntity?> FetchItem(int id)
    {
        var service = HttpContext.RequestServices.GetRequiredService<IEntityService<TEntity>>();
        return (await service.List(new { id }, new PagingInfo { PageSize = 1 })).SingleOrDefault();
    }
}