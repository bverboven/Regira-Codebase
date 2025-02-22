using Microsoft.EntityFrameworkCore;
using Regira.DAL.Paging;
using Regira.Entities.Abstractions;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Extensions;
using Regira.IO.Extensions;
using Regira.IO.Storage.Abstractions;
using Regira.IO.Storage.Helpers;
using Regira.IO.Utilities;

namespace Regira.Entities.EFcore.Attachments;

public class AttachmentRepository<TContext>
    (
        TContext dbContext, IFileService fileService,
        IEntityReadService<Attachment, int, AttachmentSearchObject> readService,
        IEntityWriteService<Attachment, int> writeService)
    : AttachmentRepository<TContext, Attachment, int, AttachmentSearchObject>(dbContext, fileService, readService, writeService)
    where TContext : DbContext;

public class AttachmentRepository<TContext, TKey>
(
    TContext dbContext, IFileService fileService,
    IEntityReadService<Attachment<TKey>, TKey, AttachmentSearchObject<TKey>> readService,
    IEntityWriteService<Attachment<TKey>, TKey> writeService)
    : AttachmentRepository<TContext, Attachment<TKey>, TKey, AttachmentSearchObject<TKey>>(dbContext, fileService, readService, writeService)
    where TContext : DbContext;

public class AttachmentRepository<TContext, TAttachment, TKey, TAttachmentSearchObject>(
    TContext dbContext, IFileService fileService,
    IEntityReadService<TAttachment, TKey, TAttachmentSearchObject> readService,
    IEntityWriteService<TAttachment, TKey> writeService)
    : EntityRepository<TAttachment, TKey, TAttachmentSearchObject>(readService, writeService), IAttachmentService<TAttachment, TKey, TAttachmentSearchObject>
    where TContext : DbContext
    where TAttachment : class, IAttachment<TKey>, new()
    where TAttachmentSearchObject : AttachmentSearchObject<TKey>, new()
{
    public virtual DbSet<TAttachment> DbSet => dbContext.Set<TAttachment>();

    public override async Task<TAttachment?> Details(TKey id)
    {
        var item = await base.Details(id);
        if (item != null)
        {
            item.Bytes = await GetBytes(item);
            ProcessItem(item);
        }

        return item;
    }
    public override async Task<IList<TAttachment>> List(TAttachmentSearchObject? so = null, PagingInfo? pagingInfo = null)
    {
        var items = await base.List(so, pagingInfo);
        foreach (var item in items)
        {
            ProcessItem(item);
        }
        return items;
    }


    public override async Task Add(TAttachment item)
    {
        PrepareItem(item);

        await SaveFile(item);
        await base.Add(item);
    }
    public override async Task<TAttachment?> Modify(TAttachment item)
    {
        PrepareItem(item);

        await SaveFile(item);
        var original = await base.Modify(item);

        return original;
    }
    public override async Task Remove(TAttachment item)
    {
        await RemoveFile(item);
        await base.Remove(item);
    }

    public async Task<byte[]?> GetBytes(TAttachment item)
    {
        if (string.IsNullOrWhiteSpace(item.Identifier))
        {
            throw new ArgumentNullException(nameof(item.Identifier));
        }

        return await fileService.GetBytes(item.Identifier);
    }
    public async Task SaveFile(TAttachment item)
    {
        if (!item.HasContent())
        {
            throw new ArgumentNullException(nameof(item.Bytes));
        }
        if (string.IsNullOrWhiteSpace(item.Identifier))
        {
            throw new ArgumentNullException(nameof(item.Identifier));
        }
#if NETSTANDARD2_0
                using var fileStream = item.GetStream();
#else
        await using var fileStream = item.GetStream();
#endif
        if (item.IsNew())
        {
            var fileNameHelper = new FileNameHelper(fileService);
            // every filename should be unique!
            var identifier = await fileNameHelper.NextAvailableFileName(item.Identifier);
            item.Identifier = identifier;
        }

        if (item.Length == 0)
        {
            item.Length = item.GetLength();
        }
        if (string.IsNullOrWhiteSpace(item.ContentType))
        {
            item.ContentType = ContentTypeUtility.GetContentType(item.Identifier);
        }

        var path = await fileService.Save(item.Identifier, fileStream!, item.ContentType);
        // don't save full path (increases flexibility for multiple platforms)
        item.Path = fileService.GetIdentifier(path);
        item.Prefix = fileService.GetRelativeFolder(item.Identifier);
        item.FileName ??= Path.GetFileName(item.Identifier);
    }
    public async Task RemoveFile(TAttachment item)
    {
        var path = item.Path;
        if (string.IsNullOrWhiteSpace(path))
        {
            path = await DbSet.Where(x => x.Id!.Equals(item.Id)).Select(x => x.Path).SingleOrDefaultAsync();
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(item.Path));
        }

        await fileService.Delete(path);
    }

    public virtual void PrepareItem(TAttachment item)
    {
        if (string.IsNullOrWhiteSpace(item.ContentType))
        {
            item.ContentType = ContentTypeUtility.GetContentType(item.FileName);
        }
    }
    public virtual void ProcessItem(TAttachment item)
    {
        if (!string.IsNullOrWhiteSpace(item.Path))
        {
            item.Identifier = fileService.GetIdentifier(item.Path);
            item.Prefix = fileService.GetRelativeFolder(item.Identifier);
        }
    }
}