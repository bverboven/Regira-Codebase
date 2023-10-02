namespace Regira.IO.Abstractions;

public interface INamedFile : IMemoryFile
{
    /// <summary>
    /// The name of a file without folder information
    /// </summary>
    string? FileName { get; set; }
}