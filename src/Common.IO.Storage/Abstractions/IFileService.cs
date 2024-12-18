namespace Regira.IO.Storage.Abstractions;

public interface IFileService
{
    string Root { get; }
    [Obsolete("Please use more generic Root", false)]
    string RootFolder { get; }

    Task<bool> Exists(string identifier);
    Task<byte[]?> GetBytes(string identifier);
    Task<Stream?> GetStream(string identifier);
    Task<IEnumerable<string>> List(FileSearchObject? so = null);


    Task Move(string sourceIdentifier, string targetIdentifier);
    Task<string> Save(string identifier, byte[] bytes, string? contentType = null);
    Task<string> Save(string identifier, Stream stream, string? contentType = null);
    Task Delete(string identifier);

    string GetAbsoluteUri(string identifier);
    string GetIdentifier(string uri);
    string? GetRelativeFolder(string identifier);
}