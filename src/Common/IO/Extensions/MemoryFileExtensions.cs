﻿using Regira.IO.Abstractions;
using Regira.IO.Models;
using Regira.IO.Utilities;

namespace Regira.IO.Extensions;

public static class MemoryFileExtensions
{
    /// <summary>
    /// Converts a byte array into an <see cref="IMemoryFile"/> instance.
    /// </summary>
    /// <param name="bytes">The byte array representing the file content.</param>
    /// <param name="contentType">
    /// The MIME type of the file content. If not specified, the content type will be null.
    /// </param>
    /// <returns>
    /// An instance of <see cref="IMemoryFile"/> containing the provided byte array and optional content type.
    /// </returns>
    public static IMemoryFile ToMemoryFile(this byte[] bytes, string? contentType = null)
        => new BinaryFileItem { Bytes = bytes, Length = bytes.Length, ContentType = contentType };
    /// <summary>
    /// Converts a <see cref="Stream"/> to an <see cref="IMemoryFile"/> representation.
    /// </summary>
    /// <param name="stream">The input stream to be converted.</param>
    /// <param name="contentType">
    /// An optional parameter specifying the content type of the file. 
    /// If not provided, the content type will be null.
    /// </param>
    /// <returns>
    /// An instance of <see cref="IMemoryFile"/> containing the data from the provided <paramref name="stream"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <paramref name="stream"/> is null.
    /// </exception>
    /// <remarks>
    /// The returned <see cref="IMemoryFile"/> will have its <see cref="IMemoryFile.Stream"/> property set to the provided <paramref name="stream"/> 
    /// and its <see cref="IMemoryFile.Length"/> property set to the length of the stream.
    /// </remarks>
    public static IMemoryFile ToMemoryFile(this Stream stream, string? contentType = null)
        => new BinaryFileItem { Stream = stream, Length = stream.Length, ContentType = contentType };

    /// <summary>
    /// Determines whether the specified <see cref="IMemoryFile"/> instance contains a byte array as its backing store.
    /// </summary>
    /// <param name="file">The <see cref="IMemoryFile"/> instance to check.</param>
    /// <returns>
    /// <c>true</c> if the <see cref="IMemoryFile"/> contains a byte array; otherwise, <c>false</c>.
    /// </returns>
    public static bool HasBytes(this IMemoryFile file)
        => file.Bytes != null;
    /// <summary>
    /// Determines whether the specified <see cref="IMemoryFile"/> instance has a valid stream as its backing store.
    /// </summary>
    /// <param name="file">The <see cref="IMemoryFile"/> instance to check.</param>
    /// <returns>
    /// <c>true</c> if the <see cref="IMemoryFile"/> contains a non-null and non-empty stream; otherwise, <c>false</c>.
    /// </returns>
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
        => file.Bytes ?? (file.HasStream() ? GetStream(file).GetBytes() : null);
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
        return file.Bytes?.GetStream();
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
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

#if NET6_0_OR_GREATER
        await File.WriteAllBytesAsync(path, bytes);
#else
        File.WriteAllBytes(path, bytes);
#endif
        return new FileInfo(path);
    }
}