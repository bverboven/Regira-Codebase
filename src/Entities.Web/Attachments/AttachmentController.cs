using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Extensions;
using Regira.Entities.Web.Attachments.Models;
using Regira.Entities.Web.Models;
using Regira.Web.Extensions;
using Regira.Web.IO;

namespace Regira.Entities.Web.Attachments;

[ApiController]
[Route("attachments")]
public class AttachmentController : ControllerBase
{
    private readonly IAttachmentService _service;
    private readonly IMapper _mapper;
    public AttachmentController(IAttachmentService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> GetFile([FromRoute] int id, bool inline = true)
    {
        var item = await _service.Details(id);

        if (item == null)
        {
            return NotFound();
        }

        return this.File(item, inline);
    }
    [HttpPost]
    public virtual async Task<ActionResult<SaveResult<AttachmentDto>>> Save(IFormFile file)
    {
        var item = file.ToNamedFile().ToAttachment<int>();
        await _service.Save(item);
        await _service.SaveChanges();
        var savedModel = _mapper.Map<AttachmentDto>(item);
        return Ok(savedModel);
    }
    [HttpPut("{id}")]
    public virtual async Task<ActionResult<SaveResult<AttachmentDto>>> Save([FromRoute] int id, IFormFile file)
    {
        var item = file.ToNamedFile().ToAttachment<int>();
        item.Id = id;
        await _service.Save(item);
        await _service.SaveChanges();
        var savedModel = _mapper.Map<AttachmentDto>(item);
        return Ok(savedModel);
    }
}