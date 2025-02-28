using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.EFcore.Primers.Abstractions;

namespace Regira.Entities.EFcore.Attachments;

public class EntityAttachmentPrimer : EntityPrimerBase<IEntityAttachment>
{
    public override Task PrepareAsync(IEntityAttachment entity, EntityEntry entry)
    {
        if (entry.State == EntityState.Modified)
        {
            if (entity.Attachment != null)
            {
                if (!string.IsNullOrWhiteSpace(entity.NewFileName))
                {
                    entity.Attachment.FileName = entity.NewFileName;
                }

                if (!string.IsNullOrWhiteSpace(entity.NewContentType))
                {
                    entity.Attachment.ContentType = entity.NewContentType;
                }
            }
        }

        return Task.CompletedTask;
    }
}