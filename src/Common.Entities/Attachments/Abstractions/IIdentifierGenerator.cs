using Regira.Entities.Attachments.Models;

namespace Regira.Entities.Attachments.Abstractions;

public interface IIdentifierGenerator<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
{
    string Generate(IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment> entity, string? rootFolder = null);
}

public interface IIdentifierGenerator : IIdentifierGenerator<int, int, int, Attachment>;