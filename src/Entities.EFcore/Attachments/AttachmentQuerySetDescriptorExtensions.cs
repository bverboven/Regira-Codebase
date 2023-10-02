using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;

namespace Regira.Entities.EFcore.Attachments;

public static class QuerySetDescriptorExtensions
{
    public static IAttachmentQuerySetDescriptor ToDescriptor<T>(this IQueryable<IEntityAttachment> query)
        => new AttachmentQuerySetDescriptor<T> { QuerySet = query };
    public static IQueryable<IEntityAttachment> ToEntityAttachments(this IAttachmentQuerySetDescriptor querySetDescriptor)
        => querySetDescriptor.QuerySet.Select(x => new EntityAttachment
        {
            Id = x.Id,
            AttachmentId = x.AttachmentId,
            ObjectId = x.ObjectId,
            SortOrder = x.SortOrder,
            ObjectType = querySetDescriptor.ObjectType,
            Attachment = x.Attachment
        });
    public static IQueryable<IEntityAttachment> ConcatAll(this IList<IAttachmentQuerySetDescriptor> sets)
    {
        IQueryable<IEntityAttachment> query = sets.First().ToEntityAttachments();
        foreach (var set in sets.Skip(1))
        {
            query = query.Concat(set.ToEntityAttachments());
        }

        return query;
    }
}