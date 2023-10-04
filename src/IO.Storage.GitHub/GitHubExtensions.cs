using Regira.Utilities;
using Regira.Web.Utilities;

namespace Regira.IO.Storage.GitHub;

public static class GitHubExtensions
{
    public static string ToTokenUri(this GitHubItem item)
    {
        var uri = item.Url;
        var downloadSegments = item.Download_Url!.Split('?');
        if (downloadSegments.Length == 2)
        {
            var query = UriUtility.ToQueryDictionary(downloadSegments.Last());
            if (query.TryGetValue("token", out var token))
            {
                uri += $"&token={token}";
            }
        }
        return uri;
    }

    public static string ToDownloadUri(this string gitUrl, string? folder = null)
    {
        var gitSegments = gitUrl
            .Substring("https://api.github.com/repos/".Length)// to be replaced below
            .Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        var owner = gitSegments.First();
        var repo = gitSegments.Skip(1).First();
        var file = $"{folder}/{gitSegments.Last().Split("?".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).First()}".TrimStart('/');

        var query = UriUtility.ToQueryDictionary(gitSegments.Last()
            .Split("?".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last());
        query.TryGetValue("ref", out var branch);

        var downloadUri = $"https://raw.githubusercontent.com/{owner}/{repo}/{branch}/{file}";
        if (query.TryGetValue("token", out var token))
        {
            downloadUri += $"?token={token}";
        }

        return downloadUri;
    }
}