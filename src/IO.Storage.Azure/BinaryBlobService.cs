using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Regira.IO.Storage.Abstractions;
using Regira.IO.Storage.Azure.Utilities;
using Regira.IO.Utilities;

namespace Regira.IO.Storage.Azure;

public class BinaryBlobService : IFileService
{
    protected internal BlobContainerClient Container
    {
        get
        {
            if (Communicator.Container == null)
            {
                throw new Exception("ContainerClient is not opened yet!");
            }

            return Communicator.Container!;
        }
    }
    public string RootFolder => Container.Uri.ToString();


    protected internal AzureCommunicator Communicator { get; }
    public BinaryBlobService(AzureCommunicator communicator)
    {
        Communicator = communicator;
    }


    public async Task<bool> Exists(string identifier)
    {
        await Communicator.Open();
        var blob = GetBlobReference(identifier);
        return await blob.ExistsAsync();
    }
    public async Task<byte[]?> GetBytes(string identifier)
    {
#if NETSTANDARD2_0
        using var stream = await GetStream(identifier);
#else
        await using var stream = await GetStream(identifier);
#endif
        if (stream == null || stream == Stream.Null)
        {
            return null;
        }
        return FileUtility.GetBytes(stream);
    }
    public async Task<Stream?> GetStream(string identifier)
    {
        await Communicator.Open();
        var blob = GetBlobReference(identifier);
        if (!await blob.ExistsAsync())
        {
            return null;
        }

        var response = await blob.DownloadAsync();
        return response.Value.Content;
    }
    public async Task<IEnumerable<string>> List(FileSearchObject? so = null)
    {
        so ??= new FileSearchObject();

        await Communicator.Open();
        var identifiers = new List<string>();
        await foreach (var blob in ListBlobs(so))
        {
            var blobUri = blob.Name;
            var blobIdentifier = FileNameUtility.GetRelativeUri(blobUri, RootFolder);
            identifiers.Add(blobIdentifier);
        }

        var recursive = so.Recursive;
        var relativeFolderUri = FileNameUtility.GetRelativeUri(so.FolderUri, RootFolder);
        var foldersToExclude = GetFolders(relativeFolderUri).Concat(new[] { relativeFolderUri }).ToArray();
        var filteredFiles = identifiers
            .Where(f => recursive || FileNameUtility.GetRelativeFolder(f, RootFolder) == relativeFolderUri)
            .Where(f => so.Extensions == null || so.Extensions.Any(e => e.TrimStart('*') == Path.GetExtension(f)))
            .ToList();

        var files = filteredFiles;

        IEnumerable<string> result = new List<string>();
        var listFiles = so.Type == FileEntryTypes.All || so.Type == FileEntryTypes.Files;
        if (listFiles)
        {
            result = result.Concat(files);
        }
        var listFolders = so.Type == FileEntryTypes.All || so.Type == FileEntryTypes.Directories;
        if (listFolders)
        {
            var filesForFetchingFolders = so.Extensions == null
                ? identifiers
                : filteredFiles;
            var folders = filesForFetchingFolders
                .SelectMany(GetFolders)
                .Where(d => recursive || FileNameUtility.GetRelativeFolder(d, RootFolder) == relativeFolderUri)
                .Except(foldersToExclude)
                .Distinct()
                .ToList();
            result = result.Concat(folders);
        }

        return result
            .Distinct()
            .OrderBy(f => f)
            .ToList();
    }

    protected IEnumerable<string> GetFolders(string uri)
    {
        var folder = FileNameUtility.GetRelativeFolder(uri, RootFolder);
        if (folder != null && folder != RootFolder)
        {
            foreach (var parent in GetFolders(folder))
            {
                yield return parent;
            }
            yield return folder;
        }
    }

    protected async IAsyncEnumerable<BlobItem> ListBlobs(FileSearchObject so)
    {
        var relativeFolderUri = FileNameUtility.GetRelativeUri(so.FolderUri, RootFolder);
        var blobPages = Container.GetBlobsAsync(BlobTraits.None, BlobStates.None, relativeFolderUri);
        await foreach (var blob in blobPages)
        {
            yield return blob;
        }
    }

    public async Task Move(string sourceIdentifier, string targetIdentifier)
    {
        await Communicator.Open();
        var sourceBlob = GetBlobReference(sourceIdentifier);
        var targetBlob = GetBlobReference(targetIdentifier);
        await targetBlob.StartCopyFromUriAsync(GetBlobUri(sourceIdentifier));
        await sourceBlob.DeleteIfExistsAsync();
    }
    public async Task<string> Save(string identifier, byte[] bytes, string? contentType = null)
    {
        await Communicator.Open();
        var blob = GetBlobReference(identifier, contentType ?? ContentTypeUtility.GetContentType(identifier));
        var headers = new BlobHttpHeaders
        {
            ContentEncoding = FileUtility.GetEncoding(bytes).HeaderName,
            ContentType = ContentTypeUtility.GetContentType(identifier)
        };
        await blob.UploadAsync(new BinaryData(bytes), new BlobUploadOptions { HttpHeaders = headers });
        return blob.Uri.ToString();
    }
    public async Task<string> Save(string identifier, Stream stream, string? contentType = null)
    {
        await Communicator.Open();
        var blob = GetBlobReference(identifier, contentType ?? ContentTypeUtility.GetContentType(identifier));
        stream.Position = 0;
        var bomBytes = new byte[4];
        var _ = await stream.ReadAsync(bomBytes, 0, 4);
        var headers = new BlobHttpHeaders
        {
            ContentEncoding = FileUtility.GetEncoding(bomBytes).ToString(),
            ContentType = ContentTypeUtility.GetContentType(identifier)
        };
        stream.Position = 0;
        await blob.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = headers });
        return blob.Uri.ToString();
    }
    public async Task Delete(string identifier)
    {
        await Communicator.Open();
        var blob = GetBlobReference(identifier);
        await blob.DeleteIfExistsAsync();
    }



    public string GetAbsoluteUri(string identifier)
    {
        Communicator.Open().Wait();
        return FileNameUtility.GetUri(identifier, RootFolder);
    }

    public string GetIdentifier(string uri)
    {
        Communicator.Open().Wait();
        return FileNameUtility.GetRelativeUri(uri, RootFolder);
    }

    public string? GetRelativeFolder(string identifier)
    {
        Communicator.Open().Wait();
        return FileNameUtility.GetRelativeFolder(identifier, RootFolder);
    }

    public Uri GetBlobUri(string identifier)
        => new(GetAbsoluteUri(identifier));
    protected internal BlobClient GetBlobReference(string identifier, string? contentType = null)
    {
        var prefixedFilename = FileNameUtility.GetRelativeUri(identifier, RootFolder);
        var blob = Container.GetBlobClient(prefixedFilename);
        //blob.Properties.ContentType = contentType;
        return blob;
    }
}