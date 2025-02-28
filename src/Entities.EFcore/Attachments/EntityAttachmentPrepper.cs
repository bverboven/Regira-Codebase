using Microsoft.EntityFrameworkCore;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.EFcore.Preppers.Abstractions;
using Regira.Entities.Extensions;

namespace Regira.Entities.EFcore.Attachments;

public class EntityAttachmentPrepper<TContext, TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>(
    TContext dbContext,
    IFileIdentifierGenerator<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment> identifierGenerator
)
    : IEntityPrepper<TEntityAttachment, TEntityAttachmentKey>
    where TContext : DbContext
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
    where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
{
    public async Task Prepare(TEntityAttachment item, TEntityAttachment? original)
    {
        item.Attachment ??= original?.Attachment;

        if (item.Attachment?.IsNew() == true)
        {
            item.Attachment!.Identifier ??= await identifierGenerator.Generate(item);
            if (original?.Attachment != null)
            {
                dbContext.Entry(original.Attachment).State = EntityState.Deleted;
            }
            dbContext.Entry(item.Attachment).State = EntityState.Added;
        }
    }
}