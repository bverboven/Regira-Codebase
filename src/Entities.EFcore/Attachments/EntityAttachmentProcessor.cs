using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.EFcore.Processing;
using Regira.IO.Storage.Abstractions;

namespace Regira.Entities.EFcore.Attachments;

public class EntityAttachmentProcessor(IFileService fileService) : EntityAttachmentProcessor<int, int, int, Attachment>(fileService);
public class EntityAttachmentProcessor<TKey, TObjectKey, TAttachmentKey, TAttachment>(IFileService fileService) : EntityProcessor<EntityAttachment<TKey, TObjectKey, TAttachmentKey, TAttachment>>
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
{
    public override Task Process(IList<EntityAttachment<TKey, TObjectKey, TAttachmentKey, TAttachment>> items)
    {
        foreach (var item in items)
        {
            if (item.Attachment != null)
            {
                item.Attachment.Identifier = fileService.GetIdentifier(item.Attachment.Path!);
                item.Attachment.Prefix = fileService.GetRelativeFolder(item.Attachment.Identifier);
            }
        }

        return Task.CompletedTask;
    }
}