﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.EFcore.Primers.Abstractions;

namespace Regira.Entities.EFcore.Attachments;

public class EntityAttachmentPrimer(IFileIdentifierGenerator fileIdentifierGenerator) : EntityPrimerBase<IEntityAttachment>
{
    public override async Task PrepareAsync(IEntityAttachment entity, EntityEntry entry)
    {
        if ((entry.State == EntityState.Added || entry.State == EntityState.Modified) && entity.Attachment != null)
        {
            entity.Attachment.Identifier ??= await fileIdentifierGenerator.Generate(entity);
        }

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
    }
}