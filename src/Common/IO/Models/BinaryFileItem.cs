using Regira.IO.Abstractions;

namespace Regira.IO.Models;

public class BinaryFileItem : IBinaryFile
{
    /// <summary>
    /// Unique identifier within a given scope (e.g. base folder)
    /// </summary>
    public string? Identifier { get; set; }
    /// <summary>
    /// Filename without folder info
    /// </summary>
    public string? FileName { get; set; }
    /// <summary>
    /// Relative folders (base folder excluded)
    /// </summary>
    public string? Prefix { get; set; }
    public string? ContentType { get; set; }
    public long Length { get; set; }

    public Stream? Stream { get; set; }
    public byte[]? Bytes { get; set; }
    public string? Path { get; set; }

    public BinaryFileItem()
    {
    }
    public BinaryFileItem(string path)
    {
        Path = path;
        FileName = System.IO.Path.GetFileName(path);
    }


    public static implicit operator BinaryFileItem(Stream? stream)
        => new() { Stream = stream, Length = stream?.Length ?? 0 };
    public static implicit operator BinaryFileItem(byte[]? bytes)
        => new() { Bytes = bytes, Length = bytes?.Length ?? 0 };

    public void Dispose()
    {
        Stream?.Dispose();
    }
}