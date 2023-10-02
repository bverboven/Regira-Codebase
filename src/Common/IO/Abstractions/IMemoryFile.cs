namespace Regira.IO.Abstractions;

public interface IMemoryFile : IMemoryBytesFile, IMemoryStreamFile
{
    new string? ContentType { get; set; }
    new long Length { get; set; }
}

public interface IMemoryBytesFile
{
    string? ContentType { get; set; }
    long Length { get; set; }
    byte[]? Bytes { get; set; }
}

public interface IMemoryStreamFile : IDisposable
{
    string? ContentType { get; set; }
    long Length { get; set; }
    Stream? Stream { get; set; }
}