namespace Regira.Entities.Web.Attachments.Abstractions
{
    public interface IEntityAttachmentInput : IEntityAttachmentInput<int, int, int>
    {
    }
    public interface IEntityAttachmentInput<TKey, TObjectId, TAttachmentId>
    {
        TKey Id { get; set; }
        TObjectId ObjectId { get; set; }
        TAttachmentId AttachmentId { get; set; }

        string? NewFileName { get; set; }
        string? NewContentType { get; set; }
    }
}
