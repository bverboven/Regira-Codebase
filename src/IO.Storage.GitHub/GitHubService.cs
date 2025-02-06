using System.Net.Http.Headers;
using System.Reflection;
using Regira.IO.Storage.Abstractions;
using Regira.IO.Utilities;
using Regira.Serializing.Abstractions;

namespace Regira.IO.Storage.GitHub;

/// <summary>
/// Based on https://docs.github.com/en/rest/repos/contents?apiVersion=2022-11-28
/// </summary>
public class GitHubService(GitHubOptions options, ISerializer serializer) : IFileService
{
    public string Root { get; } = options.Uri.TrimEnd('/') + "/contents/"; // Remove trailing slash when using api trees (https://docs.github.com/en/rest/git/trees)
    public string RootFolder => Root;

    public async Task<bool> Exists(string identifier)
    {
        using var httpClient = GetClient();
        var uri = GetIdentifier(identifier);
        var response = await httpClient.GetAsync(uri);
        return response.IsSuccessStatusCode;
    }
    public async Task<byte[]?> GetBytes(string identifier)
    {
#if NETSTANDARD2_0
        using var ms = await GetStream(identifier);
#else
        await using var ms = await GetStream(identifier);
#endif
        return FileUtility.GetBytes(ms);
    }
    public async Task<Stream?> GetStream(string identifier)
    {
        using var httpClient = GetClient();
        var gitUri = GetAbsoluteUri(identifier);
        var folder = GetRelativeFolder(identifier);
        var downloadUri = gitUri.ToDownloadUri(folder);
        var response = await httpClient.GetAsync(downloadUri);
        return await response.Content.ReadAsStreamAsync();
    }
    public async Task<IEnumerable<string>> List(FileSearchObject? so = null)
    {
        using var httpClient = GetClient();

        var listUri = $"{so?.FolderUri?.TrimStart(@"\/".ToCharArray())}";
        var response = await httpClient.GetAsync(listUri);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var items = serializer.Deserialize<GitHubItem[]>(content)!;

        var files = new List<string>();
        foreach (var item in items)
        {
            if (item.Type == GitHubItemType.Dir)
            {
                if (so == null || so.Type == FileEntryTypes.All || so.Type == FileEntryTypes.Directories)
                {
                    if (so?.Extensions == null)
                    {
                        var folder = string.Join(
                            "/",
                            new[] { Root, so?.FolderUri, item.Path }
                                .Where(x => x != null)
                                .Select(x => x!.TrimEnd(@"\/".ToCharArray()))
                        ) + '/';
                        files.Add(folder);
                    }
                }
                if (so?.Recursive == true)
                {
                    var dirSo = new FileSearchObject
                    {
                        FolderUri = $"{so.FolderUri}/{item.Name}".Trim(@"\/".ToCharArray()),
                        Type = so.Type,
                        Extensions = so.Extensions,
                        Recursive = true
                    };
                    var dirFiles = await List(dirSo);
                    files.AddRange(dirFiles);
                }
            }

            if (item.Type == GitHubItemType.File)
            {
                if (so == null || so.Type == FileEntryTypes.All || so.Type == FileEntryTypes.Files)
                {
                    if (so == null || (so.Extensions?.Any() ?? false) == false || so.Extensions?.Any(e => item.Name.EndsWith(e, StringComparison.InvariantCultureIgnoreCase)) == true)
                    {
                        var uri = item.ToTokenUri();
                        var identifier = GetIdentifier(uri);
                        files.Add(identifier);
                    }
                }
            }
        }

        return files;
    }

    public string GetAbsoluteUri(string identifier)
    {
        return FileNameUtility.GetUri(identifier, Root);
    }
    public string GetIdentifier(string uri)
    {
        return FileNameUtility.GetRelativeUri(uri, Root);
    }
    public string? GetRelativeFolder(string identifier)
    {
        return FileNameUtility.GetRelativeFolder(identifier, Root);
    }


    protected HttpClient GetClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(Root)
        };

        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent ?? Assembly.GetExecutingAssembly().GetName().Name);
        if (!string.IsNullOrEmpty(options.Key))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Key);
        }

        return httpClient;
    }


    #region NotSupported
    public Task Move(string sourceIdentifier, string targetIdentifier)
    {
        throw new NotImplementedException();
    }
    public Task<string> Save(string identifier, byte[] bytes, string? contentType = null)
    {
        throw new NotImplementedException();
    }
    public Task<string> Save(string identifier, Stream stream, string? contentType = null)
    {
        throw new NotImplementedException();
    }
    public Task Delete(string identifier)
    {
        throw new NotImplementedException();
    }
    #endregion
}