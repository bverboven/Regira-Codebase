using Regira.IO.Storage.Abstractions;

namespace Regira.IO.Storage.FileSystem;
public class BinaryFileService(FileSystemOptions options) : IFileService
{
    [Obsolete("Please use FileSystemOptions instead", false)]
    public class FileServiceOptions : FileSystemOptions;

    public string Root => options.RootFolder;
    public string RootFolder => Root;


    public Task<bool> Exists(string identifier)
    {
        var uri = FileNameUtility.GetAbsoluteUri(identifier, Root);
        var exists = File.Exists(uri) || Directory.Exists(uri);
        return Task.FromResult(exists);
    }
    public Task<byte[]?> GetBytes(string identifier)
    {
        var uri = FileNameUtility.GetAbsoluteUri(identifier, Root);
        if (!File.Exists(uri))
        {
            return Task.FromResult((byte[]?)null);
        }
        var bytes = File.ReadAllBytes(uri);
        return Task.FromResult(bytes)!;
    }
    public Task<Stream?> GetStream(string identifier)
    {
        var uri = FileNameUtility.GetAbsoluteUri(identifier, Root);
        if (!File.Exists(uri))
        {
            return Task.FromResult((Stream?)null);
        }
        var stream = (Stream)File.OpenRead(uri);
        return Task.FromResult(stream)!;
    }
    public Task<IEnumerable<string>> List(FileSearchObject? so = null)
    {
        so ??= new FileSearchObject();

        var folderUri = so.FolderUri ?? string.Empty;
        var recursive = so.Recursive;
        var extensions = "*";//so.Extensions?.Any() ?? false ? string.Join(",", so.Extensions) : "*.*";

        var listFiles = so.Type == FileEntryTypes.All || so.Type == FileEntryTypes.Files;
        var listDirectories = so.Type == FileEntryTypes.All || so.Type == FileEntryTypes.Directories;

        var searchOptions = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var absoluteFolderUri = FileNameUtility.GetAbsoluteUri(folderUri, Root);
        
        IEnumerable<string> result = Array.Empty<string>();

        if (Directory.Exists(absoluteFolderUri))
        {
            // files
            var files = Directory.EnumerateFiles(absoluteFolderUri, extensions, searchOptions)
                .Select(fileUri => FileNameUtility.GetRelativeUri(fileUri, Root))
                .Where(f => so.Extensions == null || so.Extensions.Any(e => e.TrimStart('*') == Path.GetExtension(f)));
            string[]? fileList = null;
            if (listDirectories && so.Extensions != null)
            {
                fileList = files.ToArray();
            }
            if (listFiles)
            {
                result = result.Concat(fileList ?? files);
            }
            // directories
            if (listDirectories)
            {
                var folders = Directory.EnumerateDirectories(absoluteFolderUri, "", searchOptions)
                    .Select(dirUri => FileNameUtility.GetRelativeUri(dirUri, Root));
                if (so.Extensions != null)
                {
                    folders = folders.Where(d => fileList?.Any(f => f.StartsWith(d)) == true);
                }
                result = result.Concat(folders);
            }

            result = result
                .OrderBy(f => f);
        }

        return Task.FromResult(result);
    }

    public Task Move(string sourceIdentifier, string targetIdentifier)
    {
        var sourceUri = FileNameUtility.GetAbsoluteUri(sourceIdentifier, Root);
        var targetUri = FileNameUtility.GetAbsoluteUri(targetIdentifier, Root);
        File.Move(sourceUri, targetUri);
        return Task.CompletedTask;
    }
    public Task<string> Save(string identifier, byte[] bytes, string? contentType = null)
    {
        var uri = FileNameUtility.GetAbsoluteUri(identifier, Root);
        CreateDirectory(uri);
        File.WriteAllBytes(uri, bytes);
        return Task.FromResult(uri);
    }
    public Task<string> Save(string identifier, Stream stream, string? contentType = null)
    {
        var uri = FileNameUtility.GetAbsoluteUri(identifier, Root);
        CreateDirectory(uri);
        using (var fileStream = File.Create(uri))
        {
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fileStream);
        }

        return Task.FromResult(uri);
    }
    public Task Delete(string identifier)
    {
        var uri = FileNameUtility.GetAbsoluteUri(identifier, Root);
        if (File.Exists(uri))
        {
            File.Delete(uri);
        }
        else if (Directory.Exists(uri))
        {
            var recursiveDelete = Directory.GetFiles(uri, "*", SearchOption.AllDirectories).Length == 0;
            Directory.Delete(uri, recursiveDelete);
        }

        return Task.CompletedTask;
    }

    public string GetAbsoluteUri(string identifier)
        => FileNameUtility.GetAbsoluteUri(identifier, Root);
    public string GetIdentifier(string uri)
        // use forward slashes to keep identifier uniform with other IFileService implementations (e.g. Azure)
        => FileNameUtility.GetRelativeUri(uri, Root).Replace('\\', '/');
    public string? GetRelativeFolder(string identifier)
        => FileNameUtility.GetRelativeFolder(identifier, Root);


    public void CreateDirectory(string uri)
    {
        var dirPath = Path.GetDirectoryName(uri);
        Directory.CreateDirectory(dirPath ?? throw new InvalidOperationException());
    }
}
