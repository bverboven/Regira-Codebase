using Regira.IO.Abstractions;
using Regira.IO.Extensions;
using Regira.IO.Utilities;
using Regira.Media.Drawing.Abstractions;
using Regira.Media.Drawing.Core;
using Regira.Utilities;

namespace Regira.Media.Drawing.Utilities;

public static class ImageFileUtility
{
    public static IImageFile Load(this IImageFile img, string path)
    {
        var bytes = File.ReadAllBytes(path);
        img.Bytes = bytes;
        img.Length = bytes.Length;
        img.ContentType = ContentTypeUtility.GetContentType(path);
        return img;
    }
    public static IImageFile ToImageFile(this IBinaryFile file)
    {
        if (!file.HasBytes() && !file.HasStream())
        {
            if (file.HasPath())
            {
                return new ImageFile().Load(file.Path!);
            }
        }
        return new ImageFile
        {
            Bytes = file.Bytes,
            Stream = file.Stream,
            Length = file.GetLength(),
            ContentType = file.ContentType,
        };
    }


    public static string ToBase64(IMemoryFile file, string? contentType = null)
    {
        var bytes = file.GetBytes();
        if (bytes?.Any() != true)
        {
            throw new Exception("Could not get contents of file");
        }

        contentType ??= file.ContentType ?? "image/png";
        return $"data:{contentType};base64,{bytes.GetBase64String()}";
    }
    public static IImageFile FromBase64(string contents)
    {
        string? contentType = null;
        var firstChars = contents.Truncate(64)!;
        if (firstChars.Contains(','))
        {
            contentType = firstChars.Split(';').FirstOrDefault()?.Split(':').LastOrDefault();
        }

        var bytes = FileUtility.GetBytes(contents);
        return new ImageFile
        {
            Bytes = bytes,
            Length = bytes.Length,
            ContentType = contentType,
        };
    }
}