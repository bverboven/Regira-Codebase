using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.EFcore.Processing;
using Regira.Entities.EFcore.Processing.Abstractions;

namespace Regira.Entities.EFcore.Attachments;

public class EntityAttachmentProcessor<TEntityAttachment>(IEntityProcessor<Attachment> attachmentProcessor) : EntityAttachmentProcessor<TEntityAttachment, int, int, int, Attachment>(attachmentProcessor)
    where TEntityAttachment : class, IEntityAttachment<int, int, int, Attachment>, new();
public class EntityAttachmentProcessor<TEntityAttachment, TKey, TObjectKey, TAttachmentKey, TAttachment>(IEntityProcessor<TAttachment> attachmentProcessor) : EntityProcessor<TEntityAttachment>
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
    where TEntityAttachment : class, IEntityAttachment<TKey, TObjectKey, TAttachmentKey, TAttachment>
{
    public override Task Process(IList<TEntityAttachment> items)
    {
        var attachments = items.Select(x => x.Attachment!).ToArray();

        return attachmentProcessor.Process(attachments);
    }
}