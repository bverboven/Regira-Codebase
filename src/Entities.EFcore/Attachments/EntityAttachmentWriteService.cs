using Microsoft.EntityFrameworkCore;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.EFcore.Preppers.Abstractions;
using Regira.Entities.EFcore.Services;

namespace Regira.Entities.EFcore.Attachments;

public class EntityAttachmentWriteService<TContext, TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
(
    TContext dbContext,
    IEntityReadService<TEntityAttachment, TEntityAttachmentKey> readService,
    IEnumerable<IEntityPrepper<TEntityAttachment, TEntityAttachmentKey>> preppers,
    IFileIdentifierGenerator<TEntityAttachment, TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment> identifierGenerator)
    : EntityWriteService<TContext, TEntityAttachment, TEntityAttachmentKey>(dbContext, readService, preppers)
    where TContext : DbContext
    where TAttachment : class, IAttachment<TAttachmentKey>, new()
    where TEntityAttachment : class, IEntityAttachment<TEntityAttachmentKey, TObjectKey, TAttachmentKey, TAttachment>
{
    //public override async Task PrepareItem(TEntityAttachment item, TEntityAttachment? original)
    //{
    //    await base.PrepareItem(item, original);

    //    if (item.Attachment != null)
    //    {
    //        item.Attachment!.Identifier ??= await identifierGenerator.Generate(item);
    //    }
    //}

    //public override async Task<TEntityAttachment?> Modify(TEntityAttachment item)
    //{
    //    var original = await base.Modify(item);

    //    item.Attachment ??= original?.Attachment;

    //    if (item.Attachment != null)
    //    {
    //        if (!string.IsNullOrWhiteSpace(item.NewFileName))
    //        {
    //            item.Attachment.FileName = item.NewFileName;
    //            DbContext.Set<TAttachment>().Update(item.Attachment!);
    //        }

    //        if (!string.IsNullOrWhiteSpace(item.NewContentType))
    //        {
    //            item.Attachment.ContentType = item.NewContentType;
    //            DbContext.Set<TAttachment>().Update(item.Attachment!);
    //        }
    //    }

    //    if (item.Attachment?.IsNew() == true)
    //    {
    //        if (original?.Attachment != null)
    //        {
    //            DbContext.Entry(original.Attachment).State = EntityState.Deleted;
    //        }
    //        DbContext.Entry(item.Attachment).State = EntityState.Added;
    //    }

    //    return original;
    //}

    public override Task Remove(TEntityAttachment item)
    {
        DbContext.Remove(item.Attachment!);
        return base.Remove(item);
    }
}
