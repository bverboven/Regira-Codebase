namespace Regira.Entities.Attachments.Abstractions;

public interface IIdentifierGenerator<TEntityAttachmentKey, TObjectKey, TAttachmentKey>
{
    string Generate(IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey> entity, string? rootFolder = null);
}

public interface IIdentifierGenerator : IIdentifierGenerator<int, int, int>
{
}