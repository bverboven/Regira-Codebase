using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.IO.Abstractions;
using Regira.Utilities;

namespace Regira.Entities.Attachments.Extensions;

public static class AttachmentExtensions
{
    public static Attachment ToAttachment(this INamedFile file, Attachment? src = null)
        => file.ToAttachment<Attachment, int>();
    public static TAttachment ToAttachment<TAttachment, TKey>(this INamedFile file, TAttachment? src = null)
        where TAttachment : class, IAttachment<TKey>, new()
    {
        src ??= new TAttachment();
        src.Bytes = file.Bytes;
        src.Stream = file.Stream;
        src.Length = file.Length;
        src.ContentType = file.ContentType;
        src.FileName = file.FileName;
        return src;
    }
    public static TAttachment ToAttachment<TAttachment, TKey>(this IBinaryFile file, TAttachment? src = null)
        where TAttachment : class, IAttachment<TKey>, new()
    {
        var attachment = ((INamedFile)file).ToAttachment<TAttachment, TKey>(src);
        attachment.Identifier = file.Identifier;
        attachment.Path = file.Path;
        attachment.Prefix = file.Prefix;
        return attachment;
    }


    public static EntityAttachmentSearchObject? ToSearchObject(this object? so)
        => so != null
            ? so as EntityAttachmentSearchObject ?? ObjectUtility.Create<EntityAttachmentSearchObject>(so)
            : null;
}