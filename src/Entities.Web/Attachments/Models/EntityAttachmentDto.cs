namespace Regira.Entities.Web.Attachments.Models;

public class EntityAttachmentDto : EntityAttachmentDto<int, int, int>
{
}
public class EntityAttachmentDto<TKey, TObjectId, TAttachmentId>
{
    public TKey Id { get; set; } = default!;
    public TObjectId ObjectId { get; set; } = default!;
    public TAttachmentId AttachmentId { get; set; } = default!;
    public string? ObjectType { get; set; }
    public int SortOrder { get; set; }

    public string? Uri { get; set; }

    public AttachmentDto<TAttachmentId>? Attachment { get; set; }
}