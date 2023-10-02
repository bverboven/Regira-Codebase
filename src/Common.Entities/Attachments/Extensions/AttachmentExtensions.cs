using Regira.Entities.Attachments.Models;
using Regira.IO.Abstractions;
using Regira.Utilities;

namespace Regira.Entities.Attachments.Extensions;

public static class AttachmentExtensions
{
    public static Attachment<TKey> ToAttachment<TKey>(this INamedFile file, Attachment<TKey>? src = null)
    {
        src ??= new Attachment<TKey>();
        src.Bytes = file.Bytes;
        src.Stream = file.Stream;
        src.Length = file.Length;
        src.ContentType = file.ContentType;
        src.FileName = file.FileName;
        return src;
    }
    public static Attachment<TKey> ToAttachment<TKey>(this IBinaryFile file, Attachment<TKey>? src = null)
    {
        var attachment = (file as INamedFile).ToAttachment(src);
        attachment.Identifier = file.Identifier;
        attachment.Path = file.Path;
        attachment.Prefix = file.Prefix;
        return attachment;
    }


    public static EntityAttachmentSearchObject? ToSearchObject(this object? so)
        => so != default
            ? so as EntityAttachmentSearchObject ?? ObjectUtility.Create<EntityAttachmentSearchObject>(so)
            : default;
}