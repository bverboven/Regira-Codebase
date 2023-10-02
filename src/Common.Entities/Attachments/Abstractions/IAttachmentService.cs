using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Models;

namespace Regira.Entities.Attachments.Abstractions;

public interface IAttachmentService : IAttachmentService<int>
{
}
public interface IAttachmentService<TKey> : IEntityService<Attachment<TKey>, TKey>
{
    Task<byte[]?> GetBytes(IAttachment<TKey> item);
    Task SaveFile(Attachment<TKey> item);
    Task RemoveFile(IAttachment<TKey> item);
    void ProcessItem(IAttachment<TKey> item);
}