using Regira.Entities.Attachments.Abstractions;
using Regira.Utilities;

namespace Regira.Entities.EFcore.Attachments
{
    public class DefaultIdentifierGenerator : DefaultIdentifierGenerator<int, int, int>, IIdentifierGenerator
    {
    }

    public class DefaultIdentifierGenerator<TEntityAttachmentKey, TObjectKey, TAttachmentKey> : IIdentifierGenerator<TEntityAttachmentKey, TObjectKey, TAttachmentKey>
    {
        public string Generate(IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey> entity, string? rootFolder = null)
        {
            var extension = Path.GetExtension(entity.Attachment!.FileName);
            var sanitizedFileName = Path.GetFileNameWithoutExtension(entity.Attachment!.FileName).ToKebabCase();
            var identifier = $"{entity.ObjectId}/{sanitizedFileName}-{Guid.NewGuid():N}{extension}";
            return !string.IsNullOrWhiteSpace(rootFolder)
                ? $"{rootFolder.Trim('/')}/{identifier}"
                : identifier;
        }
    }
}
