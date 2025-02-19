using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Models;

namespace Regira.Entities.Attachments.Models;

public class EntityAttachmentSearchObject : EntityAttachmentSearchObject<int, int>;
public class EntityAttachmentSearchObject<TKey, TObjectKey> : SearchObject<TKey>, IEntityAttachmentSearchObject<TKey, TObjectKey>
{
    public ICollection<TObjectKey>? ObjectId { get; set; }
    public string? Title { get; set; }
    public string? FileName { get; set; }
    public string? Extension { get; set; }
    public long? MinSize { get; set; }
    public long? MaxSize { get; set; }
}