using Regira.Entities.Attachments.Mapping.Abstractions;

namespace Regira.Entities.Mapping.Models;

public class EntityAttachmentInputDto : EntityAttachmentInputDto<int, int, int>, IEntityAttachmentInput;
public class EntityAttachmentInputDto<TKey, TObjectId, TAttachmentId> : IEntityAttachmentInput<TKey, TObjectId, TAttachmentId>
{
    public TKey Id { get; set; } = default!;
    public TObjectId ObjectId { get; set; } = default!;
    public TAttachmentId AttachmentId { get; set; } = default!;


    public string? NewFileName { get; set; }
    public string? NewContentType { get; set; }

    public AttachmentInputDto<TAttachmentId>? Attachment { get; set; }
}