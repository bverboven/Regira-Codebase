using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.EFcore.Processing;
using Regira.IO.Storage.Abstractions;

namespace Regira.Entities.EFcore.Attachments;

public class AttachmentProcessor(IFileService fileService) : AttachmentProcessor<Attachment, int>(fileService);
public class AttachmentProcessor<TAttachment, TKey>(IFileService fileService) : EntityProcessor<TAttachment>
    where TAttachment : class, IAttachment<TKey>, new()
{
    public override Task Process(IList<TAttachment> items)
    {
        foreach (var item in items)
        {
            item.Identifier = fileService.GetIdentifier(item.Path!);
            item.Prefix = fileService.GetRelativeFolder(item.Identifier);
        }

        return Task.CompletedTask;
    }
}