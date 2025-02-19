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
        IEntityReadService<Attachment<int>, int, AttachmentSearchObject<int>> readService,
        IEntityWriteService<Attachment<int>, int> writeService)
    : AttachmentRepository<TContext, int>(dbContext, fileService, readService, writeService), IAttachmentService
    where TContext : DbContext;
public class AttachmentRepository<TContext, TKey>(
    TContext dbContext, IFileService fileService,
    IEntityReadService<Attachment<TKey>, TKey, AttachmentSearchObject<TKey>> readService,
    IEntityWriteService<Attachment<TKey>, TKey> writeService)
    : EntityRepository<Attachment<TKey>, TKey, AttachmentSearchObject<TKey>>(readService, writeService), IAttachmentService<TKey>
    where TContext : DbContext
{
    public virtual DbSet<Attachment<TKey>> DbSet => dbContext.Set<Attachment<TKey>>();

    public override async Task<Attachment<TKey>?> Details(TKey id)
    {
        var item = await base.Details(id);
        if (item != null)
        {
            item.Bytes = await GetBytes(item);
            ProcessItem(item);
        }

        return item;
    }
    public override async Task<IList<Attachment<TKey>>> List(AttachmentSearchObject<TKey>? so = null, PagingInfo? pagingInfo = null)
    {
        var items = await base.List(so, pagingInfo);
        foreach (var item in items)
        {
            ProcessItem(item);
        }
        return items;
    }


    public override async Task Add(Attachment<TKey> item)
    {
        PrepareItem(item);

        await SaveFile(item);
        await base.Add(item);
    }
    public override async Task<Attachment<TKey>?> Modify(Attachment<TKey> item)
    {
        PrepareItem(item);

        await SaveFile(item);
        var original = await base.Modify(item);

        return original;
    }
    public override async Task Remove(Attachment<TKey> item)
    {
        await RemoveFile(item);
        await base.Remove(item);
    }

    public async Task<byte[]?> GetBytes(IAttachment<TKey> item)
    {
        if (string.IsNullOrWhiteSpace(item.Identifier))
        {
            throw new ArgumentNullException(nameof(item.Identifier));
        }

        return await fileService.GetBytes(item.Identifier);
    }
    public async Task SaveFile(Attachment<TKey> item)
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
    public async Task RemoveFile(IAttachment<TKey> item)
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

    public virtual void PrepareItem(Attachment<TKey> item)
    {
        if (string.IsNullOrWhiteSpace(item.ContentType))
        {
            item.ContentType = ContentTypeUtility.GetContentType(item.FileName);
        }
    }
    public virtual void ProcessItem(IAttachment<TKey> item)
    {
        if (!string.IsNullOrWhiteSpace(item.Path))
        {
            item.Identifier = fileService.GetIdentifier(item.Path);
            item.Prefix = fileService.GetRelativeFolder(item.Identifier);
        }
    }
}