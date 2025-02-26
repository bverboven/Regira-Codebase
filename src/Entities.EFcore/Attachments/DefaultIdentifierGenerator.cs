using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Utilities;

namespace Regira.Entities.EFcore.Attachments;

public class DefaultIdentifierGenerator : DefaultIdentifierGenerator<int, int, int, Attachment>, IIdentifierGenerator;

public class DefaultIdentifierGenerator<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
    : IIdentifierGenerator<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
{
    public string Generate(IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment> entity, string? rootFolder = null)
    {
        var extension = Path.GetExtension(entity.Attachment!.FileName);
        var sanitizedFileName = Path.GetFileNameWithoutExtension(entity.Attachment!.FileName).ToKebabCase();
        var identifier = $"{entity.ObjectId}/{sanitizedFileName}-{Guid.NewGuid():N}{extension}";
        return !string.IsNullOrWhiteSpace(rootFolder)
            ? $"{rootFolder.Trim('/')}/{identifier}"
            : identifier;
    }
}
