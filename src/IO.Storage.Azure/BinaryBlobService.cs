using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Regira.IO.Storage.Abstractions;
using Regira.IO.Storage.Azure.Utilities;
using Regira.IO.Utilities;

namespace Regira.IO.Storage.Azure;

public class BinaryBlobService(AzureCommunicator communicator) : IFileService
{
    protected internal BlobContainerClient Container
    {
        get
        {
            if (communicator.Container == null)
            {
                throw new Exception("ContainerClient is not opened yet!");
            }

            return communicator.Container!;
        }
    }
    public string Root => Container.Uri.ToString();
    public string RootFolder => Root;


    public async Task<bool> Exists(string identifier)
    {
        await communicator.Open();
        var blob = GetBlobReference(identifier);
        return await blob.ExistsAsync();
    }
    public async Task<byte[]?> GetBytes(string identifier)
    {
        var response = await Download(identifier);
        return response?.Value.Content.ToArray();
    }
    public async Task<Stream?> GetStream(string identifier)
    {
        var response = await Download(identifier);
        return response?.Value.Content.ToStream();
    }
    public async Task<IEnumerable<string>> List(FileSearchObject? so = null)
    {
        so ??= new FileSearchObject();

        await communicator.Open();
        var identifiers = new List<string>();
        await foreach (var blob in ListBlobs(so))
        {
            var blobUri = blob.Name;
            var blobIdentifier = FileNameUtility.GetRelativeUri(blobUri, Root);
            identifiers.Add(blobIdentifier);
        }

        var recursive = so.Recursive;
        var relativeFolderUri = FileNameUtility.GetRelativeUri(so.FolderUri, Root);
        var foldersToExclude = GetFolders(relativeFolderUri).Concat([relativeFolderUri]).ToArray();
        var filteredFiles = identifiers
            .Where(f => recursive || (FileNameUtility.GetRelativeFolder(f, Root) ?? string.Empty) == relativeFolderUri)
            .Where(f => so.Extensions == null || so.Extensions.Any(e => e.TrimStart('*') == Path.GetExtension(f)))
            .ToList();

        var files = filteredFiles;

        IEnumerable<string> result = new List<string>();
        var onlyListFiles = so.Type == FileEntryTypes.Files;
        if (onlyListFiles)
        {
            return files;
        }

        result = result.Concat(files);

        var listFolders = so.Type == FileEntryTypes.All || so.Type == FileEntryTypes.Directories;
        if (listFolders)
        {
            var filesForFetchingFolders = so.Extensions == null
                ? identifiers
                : filteredFiles;
            var folders = filesForFetchingFolders
                .SelectMany(GetFolders)
                .Where(d => recursive || (FileNameUtility.GetRelativeFolder(d, Root) ?? string.Empty) == relativeFolderUri)
                .Except(foldersToExclude)
                .Distinct()
                .ToList();
            var onlyListFolders = so.Type == FileEntryTypes.Directories;
            if (onlyListFolders)
            {
                return folders;
            }

            result = result.Concat(folders);
        }

        return result
            .Distinct()
            .OrderBy(f => f)
            .ToList();
    }

    protected IEnumerable<string> GetFolders(string uri)
    {
        var folder = FileNameUtility.GetRelativeFolder(uri, Root);
        if (folder != null && folder != Root)
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
        var relativeFolderUri = FileNameUtility.GetRelativeUri(so.FolderUri, Root);
        var blobPages = Container.GetBlobsAsync(BlobTraits.None, BlobStates.None, relativeFolderUri);
        await foreach (var blob in blobPages)
        {
            yield return blob;
        }
    }
    protected async Task<Response<BlobDownloadResult>?> Download(string identifier)
    {
        await communicator.Open();
        var blob = GetBlobReference(identifier);
        if (!await blob.ExistsAsync())
        {
            return null;
        }

        return await blob.DownloadContentAsync();
    }

    public async Task Move(string sourceIdentifier, string targetIdentifier)
    {
        await communicator.Open();
        var sourceBlob = GetBlobReference(sourceIdentifier);
        var targetBlob = GetBlobReference(targetIdentifier);
        await targetBlob.StartCopyFromUriAsync(GetBlobUri(sourceIdentifier));
        await sourceBlob.DeleteIfExistsAsync();
    }
    public async Task<string> Save(string identifier, byte[] bytes, string? contentType = null)
    {
        await communicator.Open();
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
        await communicator.Open();
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
        await communicator.Open();
        var blob = GetBlobReference(identifier);
        await blob.DeleteIfExistsAsync();
    }



    public string GetAbsoluteUri(string identifier)
    {
        communicator.Open().Wait();
        return FileNameUtility.GetUri(identifier, Root);
    }

    public string GetIdentifier(string uri)
    {
        communicator.Open().Wait();
        return FileNameUtility.GetRelativeUri(uri, Root);
    }

    public string? GetRelativeFolder(string identifier)
    {
        communicator.Open().Wait();
        return FileNameUtility.GetRelativeFolder(identifier, Root);
    }

    public Uri GetBlobUri(string identifier)
        => new(GetAbsoluteUri(identifier));
    protected internal BlobClient GetBlobReference(string identifier, string? contentType = null)
    {
        var prefixedFilename = FileNameUtility.GetRelativeUri(identifier, Root);
        var blob = Container.GetBlobClient(prefixedFilename);
        //blob.Properties.ContentType = contentType;
        return blob;
    }
}