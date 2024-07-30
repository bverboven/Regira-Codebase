using Regira.IO.Abstractions;
using Regira.IO.Models;

namespace Regira.IO.Extensions;

public static class BinaryFileExtensions
{
    public static IBinaryFile ToBinaryFile(this byte[] bytes, string? contentType = null)
        => new BinaryFileItem { Bytes = bytes, Length = bytes?.Length ?? 0, ContentType = contentType };
    public static IBinaryFile ToBinaryFile(this Stream stream, string? contentType = null)
        => new BinaryFileItem { Stream = stream, Length = stream?.Length ?? 0, ContentType = contentType };
    /// <summary>
    /// Retains the source file's properties (no stream copy is created)
    /// </summary>
    /// <param name="file"></param>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static IBinaryFile ToBinaryFile(this IMemoryFile file, string? filename = null)
    {
        if (file is IBinaryFile binaryFile)
        {
            binaryFile.FileName = filename ?? binaryFile.FileName;
            return binaryFile;
        }

        return new BinaryFileItem
        {
            Bytes = file.Bytes,
            Stream = file.Stream,
            Length = file.Length,
            ContentType = file.ContentType,
            FileName = filename
        };
    }

    public static bool HasPath(this IBinaryFile file)
        => !string.IsNullOrWhiteSpace(file.Path);

    public static byte[]? GetBytes(this IBinaryFile file)
        => MemoryFileExtensions.GetBytes(file) ?? (file.HasPath() ? File.ReadAllBytes(file.Path!) : null);
    public static Stream? GetStream(this IBinaryFile file)
        => MemoryFileExtensions.GetStream(file) ?? (file.HasPath() ? File.OpenRead(file.Path!) : null);
    /// <summary>
    /// Gets the full filename if present, otherwise a temporary file is written to disk and its path is returned
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static string GetPath(this IBinaryFile file)
    {
        if (HasPath(file))
        {
            return file.Path!;
        }

        var path = Path.GetTempFileName();
        File.WriteAllBytes(path, GetBytes(file) ?? Array.Empty<byte>());
        return path;
    }

    public static long GetLength(this IBinaryFile file)
        => file.HasPath()
            ? new FileInfo(file.Path!).Length
            : MemoryFileExtensions.GetLength(file);
}