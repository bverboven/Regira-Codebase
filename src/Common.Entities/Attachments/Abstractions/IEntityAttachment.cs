using Regira.Entities.Attachments.Models;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Attachments.Abstractions;

public interface IEntityAttachment : IEntityAttachment<int, int, int>, IHasObjectId
{
}
public interface IEntityAttachment<TKey, TObjectKey, TAttachmentId> : IEntity<TKey>, IHasObjectId<TObjectKey>, ISortable
{
    TAttachmentId AttachmentId { get; set; }
    string? ObjectType { get; }

    string? NewFileName { get; set; }
    string? NewContentType { get; set; }

    Attachment<TAttachmentId>? Attachment { get; set; }
}