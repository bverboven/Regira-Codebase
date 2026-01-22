using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Mapping.Abstractions;
using Regira.Entities.Mapping.Models;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Attachments;

public class EntityAttachmentUriAfterMapper<TEntity, TEntityAttachment, TEntityAttachmentDto, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
    (IAttachmentUriResolver<TEntityAttachment> uriResolver) : EntityAfterMapperBase<TEntity, object>
    where TEntity : class, IEntity<TObjectKey>, IHasAttachments<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
    where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
    where TEntityAttachmentDto : EntityAttachmentDto
{
    public override void AfterMap(TEntity source, object target)
    {
        var attachmentsProp = source.GetType().GetProperty(nameof(IHasAttachments.Attachments));
        var attachmentsValues = attachmentsProp?.GetValue(source);
        if (attachmentsValues == null)
        {
            return;
        }

        var dtoAttachmentsProp = target.GetType().GetProperty(nameof(IHasAttachments.Attachments));
        var dtoAttachments = (ICollection<TEntityAttachmentDto>?)dtoAttachmentsProp?.GetValue(target);
        if (dtoAttachments == null)
        {
            return;
        }

        var attachments = (ICollection<TEntityAttachment>)attachmentsValues;
        for (var i = 0; i < attachments.Count; i++)
        {
            var attachment = attachments.ElementAt(i);
            var dtoAttachment = dtoAttachments.ElementAt(i);
            if (target != null)
            {
                dtoAttachment.Uri = uriResolver.Resolve(attachment);
            }
        }
    }
}
