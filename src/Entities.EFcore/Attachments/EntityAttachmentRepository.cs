using Microsoft.EntityFrameworkCore;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.IO.Extensions;

namespace Regira.Entities.EFcore.Attachments;

public class EntityAttachmentRepository<TContext, TEntity, TEntityAttachment>(
    IAttachmentService<Attachment, int, AttachmentSearchObject<int>> attachmentService,
    IEntityReadService<TEntityAttachment, int, SearchObject<int>> readService,
    IEntityWriteService<TEntityAttachment, int> writeService,
    IIdentifierGenerator? identifierGenerator = null)
    : EntityAttachmentRepository<TContext, TEntity, TEntityAttachment, EntityAttachmentSearchObject>
        (attachmentService, readService, writeService, identifierGenerator)
    where TContext : DbContext
    where TEntity : class, IEntity<int>, IHasAttachments, IHasAttachments<TEntityAttachment>
    where TEntityAttachment : class, IEntity<int>, IEntityAttachment<int, int, int, Attachment>;

public class EntityAttachmentRepository<TContext, TEntity, TEntityAttachment, TSearchObject>(
    IAttachmentService<Attachment, int, AttachmentSearchObject<int>> attachmentService,
    IEntityReadService<TEntityAttachment, int, TSearchObject> readService,
    IEntityWriteService<TEntityAttachment, int> writeService,
    IIdentifierGenerator? identifierGenerator = null)
    : EntityAttachmentRepository<TContext, TEntity, int, TEntityAttachment, int, TSearchObject, int, Attachment>
        (attachmentService, readService, writeService, identifierGenerator),
        IEntityRepository<TEntityAttachment>
    where TContext : DbContext
    where TEntity : class, IEntity<int>, IHasAttachments, IHasAttachments<TEntityAttachment>
    where TEntityAttachment : class, IEntity<int>, IEntityAttachment<int, int, int, Attachment>
    where TSearchObject : class, IEntityAttachmentSearchObject<int, int>, new();

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
/// <typeparam name="TAttachment"></typeparam>
public class EntityAttachmentRepository<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment>(
    IAttachmentService<TAttachment, TAttachmentKey, AttachmentSearchObject<TAttachmentKey>> attachmentService,
    IEntityReadService<TEntityAttachment, TEntityAttachmentKey, TSearchObject> readService,
    IEntityWriteService<TEntityAttachment, TEntityAttachmentKey> writeService,
    IIdentifierGenerator<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>? identifierGenerator = null)
    : EntityRepository<TEntityAttachment, TEntityAttachmentKey, TSearchObject>(readService, writeService)
    where TContext : DbContext
    where TObject : class, IEntity<TObjectKey>, IHasAttachments, IHasAttachments<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
    where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>, IEntity<TEntityAttachmentKey>
    where TSearchObject : class, IEntityAttachmentSearchObject<TEntityAttachmentKey, TObjectKey>, new()
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
{
    private readonly IIdentifierGenerator<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment> _identifierGenerator = identifierGenerator ?? new DefaultIdentifierGenerator<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>();

    public override async Task<TEntityAttachment?> Details(TEntityAttachmentKey id)
    {
        var item = await base.Details(id);
        if (item != null)
        {
            item.Attachment!.Bytes = await attachmentService.GetBytes(item.Attachment);
        }
        return item;
    }

    public override async Task Add(TEntityAttachment item)
    {
        item.Attachment!.Identifier ??= CreateIdentifier(item);
        await attachmentService.SaveFile(item.Attachment);
        await base.Add(item);
    }
    public override async Task<TEntityAttachment?> Modify(TEntityAttachment item)
    {
        if (item.Attachment != null)
        {
            item.Attachment.Identifier ??= CreateIdentifier(item);
            if (item.Attachment?.HasContent() == true)
            {
                await attachmentService.SaveFile(item.Attachment);
            }
        }

        var original = await base.Modify(item);

        Modify(item, original!);

        return original;
    }
    public override async Task Remove(TEntityAttachment item)
    {
        await attachmentService.RemoveFile(item.Attachment ?? new TAttachment { Id = item.AttachmentId });
        await base.Remove(item);
    }

    public virtual void Modify(TEntityAttachment item, TEntityAttachment original)
    {
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

    public virtual string CreateIdentifier(TEntityAttachment entity)
        => _identifierGenerator.Generate(entity, $"{typeof(TObject).Name}/Attachments");
}