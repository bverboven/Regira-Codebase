using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Web.Attachments.Services;

public class AttachmentUriResolver<TEntityAttachment>(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    : AttachmentUriResolver<TEntityAttachment, int, int, int, Attachment>(linkGenerator, httpContextAccessor)
    where TEntityAttachment : IEntityAttachment<int, int, int, Attachment>;


public class AttachmentUriResolver<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor) 
    : IAttachmentUriResolver<TEntityAttachment> 
    where TEntityAttachment : IEntity<TEntityAttachmentKey>, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
{
    public virtual string? Resolve(TEntityAttachment source)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        var scheme = httpContext.Request.Scheme;
        var host = httpContext.Request.Host;
        var path = httpContext.Request.PathBase;

        return !string.IsNullOrWhiteSpace(source.Attachment?.FileName)
            ? linkGenerator.GetUriByAction(
                action: "GetFile",
                controller: typeof(TEntityAttachment).Name,
                values: new { objectId = source.ObjectId, filename = source.Attachment?.FileName, inline = true },
                scheme: scheme,
                host: host,
                pathBase: path)
            : linkGenerator.GetUriByAction(
                action: "GetFile",
                controller: typeof(TEntityAttachment).Name,
                values: new { id = source.Id, inline = true },
                scheme: scheme,
                host: host,
                pathBase: path);
    }
}
