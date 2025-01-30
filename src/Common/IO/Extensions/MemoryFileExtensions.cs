using Regira.IO.Abstractions;
using Regira.IO.Models;
using Regira.IO.Utilities;

namespace Regira.IO.Extensions;

public static class MemoryFileExtensions
{
    public static IMemoryFile ToMemoryFile(this byte[] bytes, string? contentType = null)
        => new BinaryFileItem { Bytes = bytes, Length = bytes.Length, ContentType = contentType };
    public static IMemoryFile ToMemoryFile(this Stream stream, string? contentType = null)
        => new BinaryFileItem { Stream = stream, Length = stream.Length, ContentType = contentType };

    public static bool HasBytes(this IMemoryFile file)
        => file.Bytes != null;
    public static bool HasStream(this IMemoryFile file)
        => file.Stream != null && file.Stream != Stream.Null;
    /// <summary>
    /// Checks if a file has bytes or a stream as backing store
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static bool HasContent(this IMemoryFile file)
        => file.HasBytes() || file.HasStream();

    /// <summary>
    /// If file has no bytes but does have a stream, <see cref="GetStream"/> is called to get bytes.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static byte[]? GetBytes(this IMemoryFile file)
        => file.Bytes ?? (file.HasStream() ? FileUtility.GetBytes(GetStream(file)) : null);
    /// <summary>
    /// Always creates a new stream for this file
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static Stream? GetStream(this IMemoryFile file)
    {
        if (file.HasStream())
        {
            var currentPos = file.Stream!.Position;
            // make a 2nd stream so both file and stream can be disposed without error
            file.Stream!.Position = 0;
            var ms = new MemoryStream();
            file.Stream.CopyTo(ms);
            ms.Position = currentPos;
            return ms;
        }
        return file.Bytes != null ? FileUtility.GetStream(file.Bytes) : null;
    }

    /// <summary>
    /// First checks length of <see cref="IMemoryBytesFile.Bytes"/>, then the length of the <see cref="IMemoryStreamFile.Stream" /> is checked
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static long GetLength(this IMemoryFile file)
        => file.Bytes?.Length ?? file.Stream?.Length ?? 0;


    /// <summary>
    /// Primarily used for internal use, testing, debugging (uses File.WriteAllBytes internally)
    /// </summary>
    /// <param name="file"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async Task<FileInfo> SaveAs(this IMemoryFile file, string path)
    {
        var bytes = file.GetBytes() ?? throw new NullReferenceException("File has no content");

#if NET5_0_OR_GREATER
        await File.WriteAllBytesAsync(path, bytes);
#else
            File.WriteAllBytes(path, bytes);
#endif
        return new FileInfo(path);
    }
}