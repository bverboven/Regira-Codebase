namespace Regira.Entities.Web.Attachments.Models;

public class AttachmentDto : AttachmentDto<int>
{
}
public class AttachmentDto<TKey>
{
    public TKey Id { get; set; } = default!;
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long Length { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}