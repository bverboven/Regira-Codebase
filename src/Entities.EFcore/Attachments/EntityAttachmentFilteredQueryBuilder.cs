using Microsoft.EntityFrameworkCore;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Keywords.Abstractions;
using Regira.Entities.Models.Abstractions;

namespace Regira.Entities.EFcore.Attachments;

public class EntityAttachmentFilteredQueryBuilder<TEntityAttachment, TSearchObject>(IQKeywordHelper qHelper)
    : EntityAttachmentFilteredQueryBuilder<int, TEntityAttachment, int, TSearchObject, int>(qHelper),
        IFilteredQueryBuilder<TEntityAttachment, TSearchObject>
    where TEntityAttachment : IEntityAttachment, IEntity
    where TSearchObject : class, IEntityAttachmentSearchObject;
public class EntityAttachmentFilteredQueryBuilder<TObjectKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey>(IQKeywordHelper qHelper)
    : FilteredQueryBuilderBase<TEntityAttachment, TEntityAttachmentKey, TSearchObject>
    where TEntityAttachment : IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey>, IEntity<TEntityAttachmentKey>
    where TSearchObject : class, IEntityAttachmentSearchObject<TEntityAttachmentKey, TObjectKey>
{
    public override IQueryable<TEntityAttachment> Build(IQueryable<TEntityAttachment> query, TSearchObject? so)
    {
        if (so != null)
        {
            if (so.ObjectId?.Any() == true)
            {
                query = query.Where(x => so.ObjectId.Contains(x.ObjectId));
            }

            if (!string.IsNullOrWhiteSpace(so.FileName))
            {
                var kw = qHelper.ParseKeyword(so.FileName);
                query = kw.HasWildcard
                    ? query.Where(x => EF.Functions.Like(x.Attachment!.FileName!, kw.Q!))
                    : query.Where(x => x.Attachment!.FileName == so.FileName);
            }
        }
        return query;
    }
}