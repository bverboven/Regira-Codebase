using Regira.Entities.Attachments.Abstractions;

namespace Regira.Entities.EFcore.Attachments;

public interface IAttachmentQuerySetDescriptor
{
    string ObjectType { get; }
    IQueryable<IEntityAttachment<int, int, int>> QuerySet { get; }
}
public class AttachmentQuerySetDescriptor<T> : IAttachmentQuerySetDescriptor
{
    public string ObjectType => typeof(T).Name;
    public IQueryable<IEntityAttachment<int, int, int>> QuerySet { get; set; } = null!;
}