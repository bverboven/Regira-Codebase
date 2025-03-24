namespace Regira.Entities.Attachments.Abstractions;

//public interface IFileIdentifierGenerator<in TEntityAttachment, TKey, TObjectKey, TAttachmentKey, TAttachment>
//    where TAttachment : class, IAttachment<TAttachmentKey>, new()
//    where TEntityAttachment : class, IEntityAttachment<TKey, TObjectKey, TAttachmentKey, TAttachment>
//{
//    Task<string> Generate(TEntityAttachment entity);
//}

//public interface IFileIdentifierGenerator<in TEntityAttachment> : IFileIdentifierGenerator<TEntityAttachment, int, int, int, Attachment>
//    where TEntityAttachment : EntityAttachment;


public interface IFileIdentifierGenerator
{
    Task<string> Generate(IEntityAttachment entity);
}