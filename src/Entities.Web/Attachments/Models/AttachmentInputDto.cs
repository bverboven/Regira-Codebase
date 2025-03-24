using System.ComponentModel.DataAnnotations;

namespace Regira.Entities.Web.Attachments.Models;

public class AttachmentInputDto : AttachmentInputDto<int>;
public class AttachmentInputDto<TKey>
{
    public TKey Id { get; set; } = default!;
    [MaxLength(256)]
    public string? FileName { get; set; }
    [MaxLength(128)]
    public string? ContentType { get; set; }
    public byte[]? Bytes { get; set; }
}