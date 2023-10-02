namespace Regira.IO.Abstractions;

public interface IStorageFile : INamedFile
{
    /// <summary>
    /// Identifier in a specific context. Prefix + Filename
    /// </summary>
    string? Identifier { get; set; }
    /// <summary>
    /// The folder structure, except the root folder
    /// </summary>
    string? Prefix { get; set; }
    /// <summary>
    /// The full path/Uri for this file
    /// </summary>
    string? Path { get; set; }
}