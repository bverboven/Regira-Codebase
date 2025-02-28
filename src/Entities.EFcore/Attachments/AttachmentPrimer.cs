using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.EFcore.Primers.Abstractions;
using Regira.IO.Extensions;
using Regira.IO.Utilities;

namespace Regira.Entities.EFcore.Attachments;

public class AttachmentPrimer(IAttachmentFileService<Attachment, int> fileService) : AttachmentPrimer<Attachment, int>(fileService);
public class AttachmentPrimer<TAttachment, TKey>(IAttachmentFileService<TAttachment, TKey> fileService) : EntityPrimerBase<TAttachment>
    where TAttachment : class, IAttachment<TKey>, new()
{
    public override async Task PrepareAsync(TAttachment entity, EntityEntry entry)
    {
        if (entity is { Length: 0, Bytes: not null })
        {
            entity.Length = entity.GetLength();
        }

        if (string.IsNullOrWhiteSpace(entity.ContentType) && !string.IsNullOrWhiteSpace(entity.FileName))
        {
            entity.ContentType = ContentTypeUtility.GetContentType(entity.FileName);
        }

        if (entry.State is EntityState.Added or EntityState.Modified)
        {
            await fileService.SaveFile(entity);
        }

        // delete file if entity is marked as deleted
        if (entry.State == EntityState.Deleted)
        {
            await fileService.RemoveFile(entity);
        }
    }
}