using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.Attachments.Abstractions;

public interface IEntityAttachmentSearchObject;
public interface IEntityAttachmentSearchObject<TKey, TObjectKey> : IEntityAttachmentSearchObject, ISearchObject<TKey>, IAttachmentSearchObject
{
    ICollection<TObjectKey>? ObjectId { get; set; }
}