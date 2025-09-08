using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Mapping.AutoMapper;

public class AttachmentUriResolver<TEntity, TDto>(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    : AttachmentUriResolver<TEntity, int, int, int, Attachment, TDto>(linkGenerator, httpContextAccessor)
    where TEntity : IEntity<int>, IEntityAttachment<int, int, int, Attachment>;

public class AttachmentUriResolver<TEntity, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment, TDto>(
    LinkGenerator linkGenerator,
    IHttpContextAccessor httpContextAccessor)
    : IValueResolver<TEntity, TDto, string?>
    where TEntity : IEntity<TEntityAttachmentKey>, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
{
    public virtual string Resolve(TEntity source, TDto destination, string? destMember, ResolutionContext context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null!;
        }

        var scheme = httpContext.Request.Scheme;
        var host = httpContext.Request.Host;
        var path = httpContext.Request.PathBase;
        var url = !string.IsNullOrWhiteSpace(source.Attachment?.FileName)
            ? linkGenerator.GetUriByAction("GetFile", typeof(TEntity).Name, new { objectId = source.ObjectId, filename = source.Attachment.FileName, inline = true }, scheme, host, path)
            : linkGenerator.GetUriByAction("GetFile", typeof(TEntity).Name, new { id = source.Id, inline = true }, scheme, host, path);
        return url!;
    }
}