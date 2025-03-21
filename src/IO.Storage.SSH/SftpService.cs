﻿using Regira.IO.Storage.Abstractions;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Regira.IO.Storage.SSH;

public class SftpService(SftpCommunicator communicator) : IFileService
{
    public string Root => communicator.ContainerName;
    public string RootFolder => Root;


    public async Task<bool> Exists(string identifier)
    {
        var client = await communicator.Open();
        var fileUri = FileNameUtility.GetUri(identifier, Root);
        return await client.ExistsAsync(fileUri);
    }
    public async Task<byte[]?> GetBytes(string identifier)
    {
        var client = await communicator.Open();
        var fileUri = FileNameUtility.GetUri(identifier, Root);

        return client.ReadAllBytes(fileUri);
    }
    public async Task<Stream?> GetStream(string identifier)
    {
        var client = await communicator.Open();
        var fileUri = FileNameUtility.GetUri(identifier, Root);

        return client.OpenRead(fileUri);
    }
    public async Task<IEnumerable<string>> List(FileSearchObject? so = null)
    {
        so ??= new FileSearchObject();
        var folderUri = !string.IsNullOrWhiteSpace(so.FolderUri) ? FileNameUtility.GetUri(so.FolderUri!, Root) : Root;
        var client = await communicator.Open();
        var sftpFiles = List(client, folderUri, so.Recursive);
        var files = sftpFiles
            .Where(f => so.Type == FileEntryTypes.Files && f.IsRegularFile
                        || so.Type == FileEntryTypes.Directories && f.IsDirectory)
            .Where(f => !(so.Extensions?.Any() ?? false) || so.Extensions.Any(e => e.TrimStart('*') == Path.GetExtension(f.Name)));
        return files.Select(f => FileNameUtility.GetRelativeUri(f.FullName, Root));
    }

    protected IList<ISftpFile> List(SftpClient client, string path, bool recursive = false)
    {
        var list = new List<ISftpFile>();
        var files = client.ListDirectory(path)
            .Where(f => !string.IsNullOrWhiteSpace(f.Name.Trim('.')));
        foreach (var sftpFile in files)
        {
            list.Add(sftpFile);
            if (sftpFile.IsDirectory && recursive)
            {
                list.AddRange(List(client, sftpFile.FullName, true));
            }
        }

        return list;
    }

    public async Task Move(string sourceIdentifier, string targetIdentifier)
    {
        var client = await communicator.Open();
        var sourceUri = FileNameUtility.GetUri(sourceIdentifier, Root);
        var targetUri = FileNameUtility.GetUri(targetIdentifier, Root);
#if NETSTANDARD2_0
        using (var srcStream = client.OpenRead(sourceUri))
        {
            using var targetStream = client.OpenWrite(targetUri);
            await srcStream.CopyToAsync(targetStream);
        }
#else
        await using (var srcStream = client.OpenRead(sourceUri))
        {
            await using var targetStream = client.OpenWrite(targetUri);
            await srcStream.CopyToAsync(targetStream);
        }
#endif
        client.DeleteFile(sourceUri);
    }
    public async Task<string> Save(string identifier, byte[] bytes, string? contentType = null)
    {
        var client = await communicator.Open();
        var fileUri = FileNameUtility.GetUri(identifier, Root);

        await CreateDirectory(identifier);

        client.WriteAllBytes(fileUri, bytes);
        return FileNameUtility.GetUri(fileUri, Root);
    }
    public async Task<string> Save(string identifier, Stream stream, string? contentType = null)
    {
        var client = await communicator.Open();
        var fileUri = FileNameUtility.GetUri(identifier, Root);

        await CreateDirectory(identifier);

#if NETSTANDARD2_0
        using var targetStream = client.Create(fileUri);
#else
        await using var targetStream = client.Create(fileUri);
#endif
        await stream.CopyToAsync(targetStream);
        return FileNameUtility.GetUri(fileUri, Root);
    }

    protected async Task CreateDirectory(string identifier)
    {
        var dir = FileNameUtility.GetRelativeFolder(identifier, Root)!;
        if (!string.IsNullOrWhiteSpace(dir) && !await Exists(dir))
        {
            var dirUri = FileNameUtility.GetUri(dir, Root);
            await CreateDirectory(dirUri);

            var client = await communicator.Open();
            await client.CreateDirectoryAsync(dirUri);
        }
    }
    public async Task Delete(string identifier)
    {
        var client = await communicator.Open();
        var fileUri = FileNameUtility.GetUri(identifier, Root);

        if (!await client.ExistsAsync(fileUri))
        {
            return;
        }

        var sftpFile = client.Get(fileUri);
        if (sftpFile.IsDirectory)
        {
            var files = List(client, fileUri);
            foreach (var file in files)
            {
                await Delete(FileNameUtility.GetUri(file.FullName, Root));
            }
        }
        await sftpFile.DeleteAsync();
    }

    public string GetAbsoluteUri(string identifier)
        => FileNameUtility.GetUri(identifier, Root);
    public string GetIdentifier(string uri)
        => FileNameUtility.GetRelativeUri(uri, Root);
    public string? GetRelativeFolder(string identifier)
        => FileNameUtility.GetRelativeFolder(identifier, Root);
}