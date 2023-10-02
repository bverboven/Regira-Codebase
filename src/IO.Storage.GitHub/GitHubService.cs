using Regira.IO.Storage.Abstractions;
using Regira.IO.Utilities;
using Regira.Serializing.Abstractions;
using System.Reflection;

namespace Regira.IO.Storage.GitHub;

/// <summary>
/// Based on https://docs.github.com/en/rest/repos/contents?apiVersion=2022-11-28
/// </summary>
public class GitHubService : IFileService
{
    public string RootFolder { get; }

    private readonly GitHubOptions _options;
    private readonly ISerializer _serializer;
    public GitHubService(GitHubOptions options, ISerializer serializer)
    {
        _options = options;
        _serializer = serializer;
        // Remove trailing slash when using api trees (https://docs.github.com/en/rest/git/trees)
        RootFolder = _options.Uri.TrimEnd('/') + '/';
    }

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
        var uri = GetIdentifier(identifier);
        var response = await httpClient.GetAsync(uri);
        return await response.Content.ReadAsStreamAsync();
    }
    public async Task<IEnumerable<string>> List(FileSearchObject? so = null)
    {
        using var httpClient = GetClient();

        var listUri = $"contents/{so?.FolderUri?.TrimStart(@"\/".ToCharArray())}";
        //if (_options.PageSize > 0)
        //{
        //    // not supported?
        //    listUri = $"{listUri}?per_page={_options.PageSize}";
        //}
        var response = await httpClient.GetAsync(listUri);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var items = _serializer.Deserialize<GitHubItem[]>(content)!;

        var files = new List<string>();
        foreach (var item in items)
        {
            if (_options.PageSize != 0 && files.Count >= _options.PageSize)
            {
                break;
            }

            if (item.Type == GitHubItemType.Dir)
            {
                if (so == null || so.Type == FileEntryTypes.All || so.Type == FileEntryTypes.Directories)
                {
                    if (so?.Extensions == null)
                    {
                        var folder = string.Join(
                            "/",
                            new[] { RootFolder, so?.FolderUri, item.Path }
                                .Where(x => x != null)
                                .Select(x => x!.TrimEnd(@"\/".ToCharArray()))
                        ) + '/';
                        files.Add(folder);
                    }
                }
                if (so?.Recursive == true && (_options.PageSize == 0 || files.Count < _options.PageSize))
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
                        files.Add(item.Download_Url!);
                    }
                }
            }
        }

        if (_options.PageSize > 0)
        {
            return files.Take(_options.PageSize);
        }

        return files;
    }

    public string GetAbsoluteUri(string identifier)
    {
        return FileNameUtility.GetUri(identifier, RootFolder);
    }
    public string GetIdentifier(string uri)
    {
        return FileNameUtility.GetRelativeUri(uri, RootFolder);
    }
    public string? GetRelativeFolder(string identifier)
    {
        return FileNameUtility.GetRelativeFolder(identifier, RootFolder);
    }

    protected HttpClient GetClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(RootFolder)
        };

        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent ?? Assembly.GetExecutingAssembly().GetName().Name);
        if (!string.IsNullOrEmpty(_options.Key))
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.Key);
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