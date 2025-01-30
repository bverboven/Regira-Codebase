using Microsoft.EntityFrameworkCore;
using Regira.DAL.Paging;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Keywords;
using Regira.Entities.Models.Abstractions;
using Regira.IO.Extensions;

namespace Regira.Entities.EFcore.Attachments;

public class EntityAttachmentRepository<TContext, TEntity, TEntityAttachment>(
    TContext dbContext,
    IAttachmentService attachmentService,
    IIdentifierGenerator? identifierGenerator = null)
    : EntityAttachmentRepository<TContext, TEntity, int, TEntityAttachment, int, EntityAttachmentSearchObject, int>(
            dbContext, attachmentService, identifierGenerator),
        IEntityService<TEntityAttachment>
    where TContext : DbContext
    where TEntity : class, IEntity<int>, IHasAttachments, IHasAttachments<TEntityAttachment>
    where TEntityAttachment : class, IEntityAttachment, IEntity<int>;

/// <summary>
/// Default implementation for <see cref="IEntityService{TEntityAttachment}"/>
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TObject"></typeparam>
/// <typeparam name="TObjectKey"></typeparam>
/// <typeparam name="TEntityAttachment"></typeparam>
/// <typeparam name="TEntityAttachmentKey"></typeparam>
/// <typeparam name="TSearchObject"></typeparam>
/// <typeparam name="TAttachmentKey"></typeparam>
public class EntityAttachmentRepository<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey,
    TSearchObject, TAttachmentKey>(
    TContext dbContext,
    IAttachmentService<TAttachmentKey> attachmentService,
    IIdentifierGenerator<TEntityAttachmentKey, TObjectKey, TAttachmentKey>? identifierGenerator = null)
    : EntityRepository<TContext, TEntityAttachment, TEntityAttachmentKey, TSearchObject>(dbContext)
    where TContext : DbContext
    where TObject : class, IEntity<TObjectKey>, IHasAttachments,
    IHasAttachments<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey>
    where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey>,
    IEntity<TEntityAttachmentKey>
    where TSearchObject : class, IEntityAttachmentSearchObject<TEntityAttachmentKey, TObjectKey>, new()
{
    public override IQueryable<TEntityAttachment> Query => DbSet.Include(x => x.Attachment);

    private readonly IIdentifierGenerator<TEntityAttachmentKey, TObjectKey, TAttachmentKey> _identifierGenerator = identifierGenerator ?? new DefaultIdentifierGenerator<TEntityAttachmentKey, TObjectKey, TAttachmentKey>();

    public override async Task<TEntityAttachment?> Details(TEntityAttachmentKey id)
    {
        var item = await base.Details(id);
        if (item != null)
        {
            item.Attachment!.Bytes = await attachmentService.GetBytes(item.Attachment);
        }
        return item;
    }
    public override async Task<IList<TEntityAttachment>> List(TSearchObject? so = null, PagingInfo? pagingInfo = null)
    {
        var items = await base.List(so, pagingInfo);
        foreach (var item in items)
        {
            attachmentService.ProcessItem(item.Attachment!);
        }
        return items;
    }

    public override async Task Add(TEntityAttachment item)
    {
        item.Attachment!.Identifier ??= CreateIdentifier(item);
        await attachmentService.SaveFile(item.Attachment);
        await base.Add(item);
    }
    public override async Task Modify(TEntityAttachment item)
    {
        if (item.Attachment != null)
        {
            item.Attachment.Identifier ??= CreateIdentifier(item);
            if (item.Attachment?.HasContent() == true)
            {
                await attachmentService.SaveFile(item.Attachment);
            }
        }

        await base.Modify(item);
    }
    public override async Task Remove(TEntityAttachment item)
    {
        await attachmentService.RemoveFile(item.Attachment ?? new Attachment<TAttachmentKey> { Id = item.AttachmentId });
        await base.Remove(item);
    }

    public override void Modify(TEntityAttachment item, TEntityAttachment original)
    {
        base.Modify(item, original);

        if (original.Attachment != null)
        {
            if (!string.IsNullOrWhiteSpace(item.NewFileName))
            {
                original.Attachment.FileName = item.NewFileName;
            }

            if (!string.IsNullOrWhiteSpace(item.NewContentType))
            {
                original.Attachment.ContentType = item.NewContentType;
            }
        }
        else if (item.Attachment != null)
        {
            original.Attachment = item.Attachment;
        }
    }

    public override IQueryable<TEntityAttachment> Filter(IQueryable<TEntityAttachment> query, TSearchObject? so)
    {
        var qHelper = QKeywordHelper.Create();

        query = base.Filter(query, so);

        if (so?.ObjectId?.Any() == true)
        {
            query = query.Where(x => so.ObjectId.Contains(x.ObjectId));
        }

        if (!string.IsNullOrWhiteSpace(so?.FileName))
        {
            var kw = qHelper.ParseKeyword(so.FileName);
            query = kw.HasWildcard
                ? query.Where(x => EF.Functions.Like(x.Attachment!.FileName!, kw.Q!))
                : query.Where(x => x.Attachment!.FileName == so.FileName);
        }

        return query;
    }

    public virtual string CreateIdentifier(TEntityAttachment entity)
        => _identifierGenerator.Generate(entity, $"{typeof(TObject).Name}/Attachments");
}