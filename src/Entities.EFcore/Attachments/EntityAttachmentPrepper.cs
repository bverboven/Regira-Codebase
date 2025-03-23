using Microsoft.EntityFrameworkCore;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.EFcore.Preppers.Abstractions;
using Regira.Entities.Extensions;
using Regira.IO.Extensions;

namespace Regira.Entities.EFcore.Attachments;

public class EntityAttachmentPrepper<TContext, TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>(
    TContext dbContext,
    IFileIdentifierGenerator<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment> identifierGenerator
)
    : EntityPrepperBase<TEntityAttachment>
    where TContext : DbContext
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
    where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
{
    public override async Task Prepare(TEntityAttachment item, TEntityAttachment? original)
    {
        item.Attachment ??= original?.Attachment;

        if (item.Attachment?.IsNew() == true)
        {
            if (original?.Attachment != null)
            {
                dbContext.Entry(original.Attachment).State = EntityState.Deleted;
            }

            // Don't add when attachment has no content
            if (item.Attachment.HasContent())
            {
                item.Attachment!.Identifier ??= await identifierGenerator.Generate(item);
                dbContext.Entry(item.Attachment).State = EntityState.Added;
            }
        }
    }
}