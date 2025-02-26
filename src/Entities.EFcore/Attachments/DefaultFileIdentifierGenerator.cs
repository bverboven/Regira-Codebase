using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Utilities;

namespace Regira.Entities.EFcore.Attachments;

public class DefaultFileIdentifierGenerator<TObject, TEntityAttachment>(IAttachmentFileService<Attachment, int> fileService)
    : DefaultFileIdentifierGenerator<TObject, TEntityAttachment, int, int, int, Attachment>(fileService), IFileIdentifierGenerator<TEntityAttachment>
    where TEntityAttachment : EntityAttachment
    where TObject : IHasAttachments<TEntityAttachment, int, int, int, Attachment>;

public class DefaultFileIdentifierGenerator<TObject, TEntityAttachment, TKey, TObjectKey, TAttachmentKey, TAttachment>(IAttachmentFileService<TAttachment, TAttachmentKey> fileService)
    : IFileIdentifierGenerator<TEntityAttachment, TKey, TObjectKey, TAttachmentKey, TAttachment>
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
    where TObject : IHasAttachments<TEntityAttachment, TKey, TObjectKey, TAttachmentKey, TAttachment>
    where TEntityAttachment : class, IEntityAttachment<TKey, TObjectKey, TAttachmentKey, TAttachment>
{
    public virtual Task<string> Generate(TEntityAttachment entity)
    {
        var entityFolder = $"{typeof(TObject).Name}/Attachments/{entity.ObjectId}";
        var extension = Path.GetExtension(entity.Attachment!.FileName);
        var sanitizedFileName = Path.GetFileNameWithoutExtension(entity.Attachment!.FileName).ToKebabCase();
        var fileName = $"{entityFolder}/{sanitizedFileName}-{Guid.NewGuid():N}{extension}";
        return Task.FromResult(fileService.GetIdentifier(fileName));
    }
}
