using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Utilities;

namespace Regira.Entities.EFcore.Attachments;

public class DefaultFileIdentifierGenerator<TAttachmentKey, TAttachment>(IAttachmentFileService<TAttachment, TAttachmentKey> fileService)
    : IFileIdentifierGenerator
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
{
    public virtual Task<string> Generate(IEntityAttachment entity)
    {
        var entityType = entity.GetType();
        var idProp = entityType.GetProperty(nameof(EntityAttachment.ObjectId))!;
        var objectName = entityType.Name.Replace("Attachment", string.Empty);
        var entityFolder = $"{objectName}/Attachments/{idProp.GetValue(entity)}";
        var extension = Path.GetExtension(entity.Attachment!.FileName);
        var sanitizedFileName = Path.GetFileNameWithoutExtension(entity.Attachment!.FileName).ToKebabCase();
        var fileName = $"{entityFolder}/{sanitizedFileName}-{Guid.NewGuid():N}{extension}";
        return Task.FromResult(fileService.GetIdentifier(fileName));
    }
}