using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Web.Attachments.Services;

public class AttachmentUriResolver<TEntity>(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    : AttachmentUriResolver<TEntity, int, int, int, Attachment>(linkGenerator, httpContextAccessor)
    where TEntity : IEntityAttachment<int, int, int, Attachment>;
public class AttachmentUriResolver<TEntity, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    where TEntity : IEntity<TEntityAttachmentKey>, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
{
    public virtual string? Resolve(TEntity source)
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
                controller: typeof(TEntity).Name,
                values: new { objectId = source.ObjectId, filename = source.Attachment?.FileName, inline = true },
                scheme: scheme,
                host: host,
                pathBase: path)
            : linkGenerator.GetUriByAction(
                action: "GetFile",
                controller: typeof(TEntity).Name,
                values: new { id = source.Id, inline = true },
                scheme: scheme,
                host: host,
                pathBase: path);
    }
}
