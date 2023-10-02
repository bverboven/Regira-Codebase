namespace Regira.IO.Storage.FileSystem;

public class TextFileService : BinaryFileService, Abstractions.ITextFileService
{
    public TextFileService(string rootFolder)
        : base(rootFolder)
    {
    }


    public Task<string?> GetContents(string identifier)
    {
        var uri = FileNameUtility.GetAbsoluteUri(identifier, RootFolder);
        if (!File.Exists(uri))
        {
            return Task.FromResult((string?)null);
        }
        var content = File.ReadAllText(uri);
        return Task.FromResult(content)!;
    }
    public Task<string> Save(string identifier, string contents, string? contentType = null)
    {
        var uri = FileNameUtility.GetAbsoluteUri(identifier, RootFolder);
        File.WriteAllText(uri, contents);
        return Task.FromResult(uri);
    }
}