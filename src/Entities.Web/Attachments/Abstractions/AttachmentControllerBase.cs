using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Extensions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Web.Attachments.Models;
using Regira.Entities.Web.Models;
using Regira.Web.Extensions;
using Regira.Web.IO;

namespace Regira.Entities.Web.Attachments.Abstractions;

[ApiController]
[Route("attachments")]
public abstract class AttachmentControllerBase(IEntityService<Attachment, int> service, IEntityMapper mapper) : ControllerBase
{
    [HttpGet("{id}")]
    public virtual async Task<IActionResult> GetFile([FromRoute] int id, bool inline = true)
    {
        var item = await service.Details(id);

        if (item == null)
        {
            return NotFound();
        }

        return this.File(item, inline);
    }
    [HttpPost]
    public virtual async Task<ActionResult<SaveResult<AttachmentDto>>> Save(IFormFile file)
    {
        var item = file.ToNamedFile().ToAttachment();
        await service.Save(item);
        await service.SaveChanges();
        var savedModel = mapper.Map<AttachmentDto>(item);
        return Ok(savedModel);
    }
    [HttpPut("{id}")]
    public virtual async Task<ActionResult<SaveResult<AttachmentDto>>> Save([FromRoute] int id, IFormFile file)
    {
        var item = file.ToNamedFile().ToAttachment();
        item.Id = id;
        await service.Save(item);
        await service.SaveChanges();
        var savedModel = mapper.Map<AttachmentDto>(item);
        return Ok(savedModel);
    }
}