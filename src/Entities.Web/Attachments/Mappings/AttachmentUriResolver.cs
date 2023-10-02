using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Web.Attachments.Mappings;

public class AttachmentUriResolver<TEntity, TDto> : AttachmentUriResolver<TEntity, int, int, int, TDto>
    where TEntity : IEntity<int>, IEntityAttachment
{
    public AttachmentUriResolver(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
        : base(linkGenerator, httpContextAccessor)
    {
    }
}
public class AttachmentUriResolver<TEntity, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TDto> : IValueResolver<TEntity, TDto, string?>
    where TEntity : IEntity<TEntityAttachmentKey>, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey>
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AttachmentUriResolver(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    public virtual string Resolve(TEntity source, TDto destination, string? destMember, ResolutionContext context)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null!;
        }

        var scheme = httpContext.Request.Scheme;
        var host = httpContext.Request.Host;
        var path = httpContext.Request.PathBase;
        var url = !string.IsNullOrWhiteSpace(source.Attachment?.FileName)
            ? _linkGenerator.GetUriByAction("GetFile", typeof(TEntity).Name, new { objectId = source.ObjectId, filename = source.Attachment.FileName, inline = true }, scheme, host, path)
            : _linkGenerator.GetUriByAction("GetFile", typeof(TEntity).Name, new { id = source.Id, inline = true }, scheme, host, path);
        return url!;
    }
}