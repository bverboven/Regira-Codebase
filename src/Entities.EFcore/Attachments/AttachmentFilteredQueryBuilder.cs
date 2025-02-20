using Microsoft.EntityFrameworkCore;
using Regira.Entities.Attachments.Models;
using Regira.Entities.EFcore.QueryBuilders.Abstractions;
using Regira.Entities.Keywords.Abstractions;

namespace Regira.Entities.EFcore.Attachments;

public class AttachmentFilteredQueryBuilder(IQKeywordHelper qHelper) : AttachmentFilteredQueryBuilder<int>(qHelper),
    IFilteredQueryBuilder<Attachment, int, AttachmentSearchObject>
{
    public IQueryable<Attachment> Build(IQueryable<Attachment> query, AttachmentSearchObject? so)
    {
        return base.Build(query, so)
            .Cast<Attachment>();
    }
}

public class AttachmentFilteredQueryBuilder<TKey>(IQKeywordHelper qHelper) : FilteredQueryBuilderBase<Attachment<TKey>, TKey, AttachmentSearchObject<TKey>>
{
    public override IQueryable<Attachment<TKey>> Build(IQueryable<Attachment<TKey>> query, AttachmentSearchObject<TKey>? so)
    {
        if (!string.IsNullOrWhiteSpace(so?.FileName))
        {
            var kw = qHelper.ParseKeyword(so.FileName);
            query = kw.HasWildcard
                ? query.Where(x => EF.Functions.Like(x.FileName!, kw.Q!))
                : query.Where(x => x.FileName == so.FileName);
        }

        if (!string.IsNullOrWhiteSpace(so?.Extension))
        {
            query = query.Where(x => EF.Functions.Like(x.FileName!, $"*{so.Extension}"));
        }

        if (so?.MinSize.HasValue == true)
        {
            query = query.Where(x => x.Length >= so.MinSize);
        }
        if (so?.MaxSize.HasValue == true)
        {
            query = query.Where(x => x.Length <= so.MaxSize);
        }

        return query;
    }
}