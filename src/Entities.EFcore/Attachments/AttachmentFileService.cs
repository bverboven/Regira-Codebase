using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Extensions;
using Regira.IO.Extensions;
using Regira.IO.Storage.Abstractions;
using Regira.IO.Storage.Helpers;

namespace Regira.Entities.EFcore.Attachments;

public class AttachmentFileService<TAttachment, TKey>(IFileService fileService) : IAttachmentFileService<TAttachment, TKey>
    where TAttachment : class, IAttachment<TKey>, new()
{
    public async Task<byte[]?> GetBytes(TAttachment item)
    {
        if (item.HasContent())
        {
            return item.GetBytes();
        }

        var identifier = item.Identifier ??= fileService.GetIdentifier(item.Path!);
        if (!string.IsNullOrWhiteSpace(identifier))
        {
            return await fileService.GetBytes(identifier);
        }

        return null;
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
        using var fileStream = item.GetStream()!;
#else
        await using var fileStream = item.GetStream()!;
#endif
        if (item.IsNew())
        {
            var fileNameHelper = new FileNameHelper(fileService);
            // every filename should be unique!
            var identifier = await fileNameHelper.NextAvailableFileName(item.Identifier);
            item.Identifier = identifier;
        }

        var path = await fileService.Save(item.Identifier, fileStream, item.ContentType);
        // don't save full path (increases flexibility for multiple platforms)
        item.Path = fileService.GetIdentifier(path);
        item.Prefix = fileService.GetRelativeFolder(item.Identifier);
        item.FileName ??= Path.GetFileName(item.Identifier);
    }
    public async Task RemoveFile(TAttachment item)
    {
        if (string.IsNullOrWhiteSpace(item.Identifier))
        {
            throw new ArgumentNullException(nameof(item.Identifier));
        }

        await fileService.Delete(item.Identifier);
    }

    public string GetIdentifier(string fileName)
        => fileService.GetIdentifier(fileName);
    public string GetRelativeFolder(TAttachment item)
        => fileService.GetRelativeFolder(item.Path!)!;
}