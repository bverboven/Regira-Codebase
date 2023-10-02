using Microsoft.EntityFrameworkCore;
using Regira.DAL.Paging;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.EFcore.Services;
using Regira.Entities.Keywords;
using Regira.IO.Extensions;
using Regira.IO.Storage.Abstractions;
using Regira.IO.Storage.Helpers;
using Regira.IO.Utilities;

namespace Regira.Entities.EFcore.Attachments;

public class AttachmentRepository<TContext> : AttachmentRepository<TContext, int>, IAttachmentService
    where TContext : DbContext
{
    public AttachmentRepository(TContext dbContext, IFileService fileService)
        : base(dbContext, fileService)
    {
    }
}
public class AttachmentRepository<TContext, TKey> : EntityRepository<TContext, Attachment<TKey>, TKey, AttachmentSearchObject<TKey>>, IAttachmentService<TKey>
    where TContext : DbContext
{
    private readonly IFileService _fileService;
    public AttachmentRepository(TContext dbContext, IFileService fileService)
        : base(dbContext)
    {
        _fileService = fileService;
    }

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
        await SaveFile(item);
        await base.Add(item);
    }
    public override async Task Modify(Attachment<TKey> item)
    {
        await SaveFile(item);
        await base.Modify(item);
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

        return await _fileService.GetBytes(item.Identifier);
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
        if (IsNew(item))
        {
            var fileNameHelper = new FileNameHelper(_fileService);
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

        var path = await _fileService.Save(item.Identifier, fileStream!, item.ContentType);
        // don't save full path (increases flexibility for multiple platforms)
        item.Path = _fileService.GetIdentifier(path);
        item.Prefix = _fileService.GetRelativeFolder(item.Identifier);
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

        await _fileService.Delete(path);
    }

    public override IQueryable<Attachment<TKey>> Filter(IQueryable<Attachment<TKey>> query, AttachmentSearchObject<TKey>? so)
    {
        var qHelper = QKeywordHelper.Create();

        query = base.Filter(query, so);


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

    public override void PrepareItem(Attachment<TKey> item)
    {
        if (string.IsNullOrWhiteSpace(item.ContentType))
        {
            item.ContentType = ContentTypeUtility.GetContentType(item.FileName);
        }
        base.PrepareItem(item);
    }
    public virtual void ProcessItem(IAttachment<TKey> item)
    {
        if (!string.IsNullOrWhiteSpace(item.Path))
        {
            item.Identifier = _fileService.GetIdentifier(item.Path);
            item.Prefix = _fileService.GetRelativeFolder(item.Identifier);
        }
    }
}