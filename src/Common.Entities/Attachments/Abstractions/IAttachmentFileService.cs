namespace Regira.Entities.Attachments.Abstractions;

public interface IAttachmentFileService<in TAttachment, TKey>
    where TAttachment : class, IAttachment<TKey>, new()
{
    Task<byte[]?> GetBytes(TAttachment item);
    Task SaveFile(TAttachment item);
    Task RemoveFile(TAttachment item);

    string GetIdentifier(string fileName);
    string GetRelativeFolder(TAttachment item);
}