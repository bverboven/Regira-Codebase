using Regira.IO.Storage.Abstractions;

namespace Regira.IO.Storage.FileSystem;

public class BinaryFileService : IFileService
{
    public class FileServiceOptions
    {
        public string RootFolder { get; set; } = null!;
    }
    public string RootFolder { get; }
    public BinaryFileService(string rootFolder)
        : this(new FileServiceOptions { RootFolder = rootFolder })
    {
    }
    public BinaryFileService(FileServiceOptions options)
    {
        RootFolder = options.RootFolder;
    }


    public Task<bool> Exists(string identifier)
    {
        var uri = FileNameUtility.GetAbsoluteUri(identifier, RootFolder);
        var exists = File.Exists(uri) || Directory.Exists(uri);
        return Task.FromResult(exists);
    }
    public Task<byte[]?> GetBytes(string identifier)
    {
        var uri = FileNameUtility.GetAbsoluteUri(identifier, RootFolder);
        if (!File.Exists(uri))
        {
            return Task.FromResult((byte[]?)null);
        }
        var bytes = File.ReadAllBytes(uri);
        return Task.FromResult(bytes)!;
    }
    public Task<Stream?> GetStream(string identifier)
    {
        var uri = FileNameUtility.GetAbsoluteUri(identifier, RootFolder);
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

        var options = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var absoluteFolderUri = FileNameUtility.GetAbsoluteUri(folderUri, RootFolder);

        IEnumerable<string> result = Array.Empty<string>();

        if (Directory.Exists(absoluteFolderUri))
        {
            // files
            var files = Directory.EnumerateFiles(absoluteFolderUri, extensions, options)
                .Select(fileUri => FileNameUtility.GetRelativeUri(fileUri, RootFolder))
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
                var folders = Directory.EnumerateDirectories(absoluteFolderUri, "", options)
                    .Select(dirUri => FileNameUtility.GetRelativeUri(dirUri, RootFolder));
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
        var sourceUri = FileNameUtility.GetAbsoluteUri(sourceIdentifier, RootFolder);
        var targetUri = FileNameUtility.GetAbsoluteUri(targetIdentifier, RootFolder);
        File.Move(sourceUri, targetUri);
        return Task.CompletedTask;
    }
    public Task<string> Save(string identifier, byte[] bytes, string? contentType = null)
    {
        var uri = FileNameUtility.GetAbsoluteUri(identifier, RootFolder);
        CreateDirectory(uri);
        File.WriteAllBytes(uri, bytes);
        return Task.FromResult(uri);
    }
    public Task<string> Save(string identifier, Stream stream, string? contentType = null)
    {
        var uri = FileNameUtility.GetAbsoluteUri(identifier, RootFolder);
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
        var uri = FileNameUtility.GetAbsoluteUri(identifier, RootFolder);
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
        => FileNameUtility.GetAbsoluteUri(identifier, RootFolder);
    public string GetIdentifier(string uri)
        => FileNameUtility.GetRelativeUri(uri, RootFolder);
    public string? GetRelativeFolder(string identifier)
        => FileNameUtility.GetRelativeFolder(identifier, RootFolder);


    public void CreateDirectory(string uri)
    {
        var dirPath = Path.GetDirectoryName(uri);
        Directory.CreateDirectory(dirPath ?? throw new InvalidOperationException());
    }
}