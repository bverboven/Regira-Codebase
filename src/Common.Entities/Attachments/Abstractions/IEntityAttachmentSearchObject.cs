using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Attachments.Abstractions;

public interface IEntityAttachmentSearchObject : IEntityAttachmentSearchObject<int, int>, ISearchObject;
public interface IEntityAttachmentSearchObject<TKey, TObjectKey> : ISearchObject<TKey>, IAttachmentSearchObject
{
    ICollection<TObjectKey>? ObjectId { get; set; }
}