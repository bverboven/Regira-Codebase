namespace Regira.Entities.Attachments.Abstractions;


public interface IHasAttachments
{
    ICollection<IEntityAttachment>? Attachments { get; set; }
    public bool? HasAttachment { get; set; }
}

public interface IHasAttachments<TEntityAttachment> : IHasAttachments<TEntityAttachment, int, int, int>
    where TEntityAttachment : IEntityAttachment<int, int, int>;

public interface IHasAttachments<TEntityAttachment, TKey, TObjectKey, TAttachmentKey>
    where TEntityAttachment : IEntityAttachment<TKey, TObjectKey, TAttachmentKey>
{
    ICollection<TEntityAttachment>? Attachments { get; set; }
    public bool? HasAttachment { get; set; }
}