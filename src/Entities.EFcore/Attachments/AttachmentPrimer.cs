using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Primers.Abstractions;
using Regira.IO.Abstractions;
using Regira.IO.Utilities;

namespace Regira.Entities.EFcore.Attachments;

public class AttachmentPrimer : EntityPrimerBase<IBinaryFile>
{
    public override Task PrepareAsync(IBinaryFile entity, EntityEntry entry)
    {
        if (entity is { Length: 0, Bytes: not null })
        {
            entity.Length = entity.Bytes?.Length ?? 0;
        }
        if (string.IsNullOrWhiteSpace(entity.ContentType) && !string.IsNullOrWhiteSpace(entity.FileName))
        {
            entity.ContentType = ContentTypeUtility.GetContentType(entity.FileName);
        }

        return Task.CompletedTask;
    }
}