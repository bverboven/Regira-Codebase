using System.ComponentModel.DataAnnotations.Schema;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Attachments.Models;

public class EntityAttachment : EntityAttachment<int, int, int>, IEntityWithSerial, IEntityAttachment;
public class EntityAttachment<TKey, TObjectKey, TAttachmentKey> : IEntityAttachment<TKey, TObjectKey, TAttachmentKey>
{
    public TKey Id { get; set; } = default!;

    public TObjectKey ObjectId { get; set; } = default!;
    public TAttachmentKey AttachmentId { get; set; } = default!;
    [NotMapped]
    public string? ObjectType { get; set; }
    public int SortOrder { get; set; }

    [NotMapped]
    public string? NewFileName { get; set; }
    [NotMapped]
    public string? NewContentType { get; set; }

    public Attachment<TAttachmentKey>? Attachment { get; set; }
}