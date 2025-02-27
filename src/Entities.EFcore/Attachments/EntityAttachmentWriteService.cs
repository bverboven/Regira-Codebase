using Microsoft.EntityFrameworkCore;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Extensions;

namespace Regira.Entities.EFcore.Attachments;

//public class EntityAttachmentRepository<TContext, TEntity, TEntityAttachment>(
//    IAttachmentService<Attachment, int, AttachmentSearchObject<int>> attachmentService,
//    IEntityReadService<TEntityAttachment, int, SearchObject<int>> readService,
//    IEntityWriteService<TEntityAttachment, int> writeService)
//    : EntityAttachmentRepository<TContext, TEntity, TEntityAttachment, EntityAttachmentSearchObject>
//        (attachmentService, readService, writeService)
//    where TContext : DbContext
//    where TEntity : class, IEntity<int>, IHasAttachments, IHasAttachments<TEntityAttachment>
//    where TEntityAttachment : class, IEntity<int>, IEntityAttachment<int, int, int, Attachment>, new();

//public class EntityAttachmentRepository<TContext, TEntity, TEntityAttachment, TSearchObject>(
//    IAttachmentService<Attachment, int, AttachmentSearchObject<int>> attachmentService,
//    IEntityReadService<TEntityAttachment, int, TSearchObject> readService,
//    IEntityWriteService<TEntityAttachment, int> writeService)
//    : EntityAttachmentRepository<TContext, TEntity, int, TEntityAttachment, int, TSearchObject, int, Attachment>
//        (attachmentService, readService, writeService),
//        IEntityRepository<TEntityAttachment>
//    where TContext : DbContext
//    where TEntity : class, IEntity<int>, IHasAttachments, IHasAttachments<TEntityAttachment>
//    where TEntityAttachment : class, IEntity<int>, IEntityAttachment<int, int, int, Attachment>, new()
//    where TSearchObject : class, IEntityAttachmentSearchObject<int, int>, new();

///// <summary>
///// Default implementation for <see cref="IEntityService{TEntityAttachment}"/>
///// </summary>
///// <typeparam name="TContext"></typeparam>
///// <typeparam name="TObject"></typeparam>
///// <typeparam name="TObjectKey"></typeparam>
///// <typeparam name="TEntityAttachment"></typeparam>
///// <typeparam name="TEntityAttachmentKey"></typeparam>
///// <typeparam name="TSearchObject"></typeparam>
///// <typeparam name="TAttachmentKey"></typeparam>
///// <typeparam name="TAttachment"></typeparam>
//public class EntityAttachmentRepository<TContext, TObject, TObjectKey, TEntityAttachment, TEntityAttachmentKey, TSearchObject, TAttachmentKey, TAttachment>(
//    IEntityService<TAttachment, TAttachmentKey, AttachmentSearchObject<TAttachmentKey>> attachmentService,
//    IEntityReadService<TEntityAttachment, TEntityAttachmentKey, TSearchObject> readService,
//    IEntityWriteService<TEntityAttachment, TEntityAttachmentKey> writeService)
//    : EntityRepository<TEntityAttachment, TEntityAttachmentKey, TSearchObject>(readService, writeService)
//    where TContext : DbContext
//    where TObject : class, IEntity<TObjectKey>, IHasAttachments, IHasAttachments<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
//    where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>, IEntity<TEntityAttachmentKey>, new()
//    where TSearchObject : class, IEntityAttachmentSearchObject<TEntityAttachmentKey, TObjectKey>, new()
//    where TAttachment : class, IAttachment<TAttachmentKey>, new()
//{
//    public override async Task<TEntityAttachment?> Details(TEntityAttachmentKey id)
//    {
//        var item = await base.Details(id);
//        if (item != null)
//        {
//            item.Attachment!.Bytes = await attachmentService.GetBytes(item.Attachment);
//        }
//        return item;
//    }

//    public override async Task Remove(TEntityAttachment item)
//    {
//        await attachmentService.RemoveFile(item.Attachment ?? new TAttachment { Id = item.AttachmentId });
//        await base.Remove(item);
//    }
//}

public class EntityAttachmentWriteService<TContext, TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
(
    TContext dbContext,
    IEntityReadService<TEntityAttachment, TEntityAttachmentKey> readService,
    IFileIdentifierGenerator<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment> identifierGenerator)
    : EntityWriteService<TContext, TEntityAttachment, TEntityAttachmentKey>(dbContext, readService)
    where TContext : DbContext
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
    where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
{
    public override async Task PrepareItem(TEntityAttachment item, TEntityAttachment? original)
    {
        await base.PrepareItem(item, original);

        if (item.Attachment != null)
        {
            item.Attachment!.Identifier ??= await identifierGenerator.Generate(item);
        }
    }

    public override async Task<TEntityAttachment?> Modify(TEntityAttachment item)
    {
        var original = await base.Modify(item);

        if (original is { Attachment: not null })
        {
            if (item.Attachment?.IsNew() == true)
            {
                DbContext.Entry(original.Attachment).State = EntityState.Detached;
                DbContext.Attach(item.Attachment);
                DbContext.Entry(item.Attachment).State = EntityState.Added;
                original.Attachment = item.Attachment;
                original.AttachmentId = item.Attachment.Id;
                DbContext.Update(original);
            }

            if (!string.IsNullOrWhiteSpace(item.NewFileName))
            {
                original.Attachment.FileName = item.NewFileName;
                DbContext.Set<TAttachment>().Update(original.Attachment!);
            }

            if (!string.IsNullOrWhiteSpace(item.NewContentType))
            {
                original.Attachment.ContentType = item.NewContentType;
                DbContext.Set<TAttachment>().Update(original.Attachment!);
            }
        }

        return original;
    }

    public override Task Remove(TEntityAttachment item)
    {
        DbContext.Remove(item.Attachment!);
        return base.Remove(item);
    }
}
