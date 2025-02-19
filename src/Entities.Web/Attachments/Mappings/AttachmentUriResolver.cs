using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Web.Attachments.Mappings;

public class AttachmentUriResolver<TEntity, TDto>(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    : AttachmentUriResolver<TEntity, int, int, int, TDto>(linkGenerator, httpContextAccessor)
    where TEntity : IEntity<int>, IEntityAttachment<int, int, int>;
public class AttachmentUriResolver<TEntity, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TDto>(
    LinkGenerator linkGenerator,
    IHttpContextAccessor httpContextAccessor)
    : IValueResolver<TEntity, TDto, string?>
    where TEntity : IEntity<TEntityAttachmentKey>, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey>
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