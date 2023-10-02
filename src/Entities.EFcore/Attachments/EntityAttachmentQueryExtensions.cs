using Microsoft.EntityFrameworkCore;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.Attachments;

public static class EntityAttachmentQueryExtensions
{
    public static IQueryable<TEntityAttachment> Filter<TEntityAttachment>(this IQueryable<TEntityAttachment> query, EntityAttachmentSearchObject? aso)
        where TEntityAttachment : class, IEntityAttachment, IEntity<int>
    {
        if (aso != null)
        {
            if (aso.ObjectId?.Any() == true)
            {
                query = query.Where(x => aso.ObjectId.Contains(x.ObjectId));
            }

            if (!string.IsNullOrWhiteSpace(aso.FileName))
            {
                query = query.Where(x => x.Attachment!.FileName == aso.FileName);
            }
            if (!string.IsNullOrWhiteSpace(aso.Extension))
            {
                query = query.Where(x => EF.Functions.Like(x.Attachment!.FileName!, $"%{aso.Extension}"));
            }

            if (aso.MinSize.HasValue)
            {
                query = query.Where(x => x.Attachment!.Length >= aso.MinSize);
            }
            if (aso.MaxSize.HasValue)
            {
                query = query.Where(x => x.Attachment!.Length <= aso.MaxSize);
            }
        }

        return query;
    }
}