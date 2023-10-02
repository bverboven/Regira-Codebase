using System.Net.Http.Json;
using Regira.ProjectFilesProcessor.Models;

namespace Regira.ProjectFilesProcessor.Services;

public class BaGetService
{
    private readonly HttpClient _client;
    public BaGetService(HttpClient client)
    {
        _client = client;
    }


    public async Task<IEnumerable<Project>> List()
    {
        var projects = (await _client.GetFromJsonAsync<BaGetResponse>("search?take=1000&semVerLevel=2.0.0&prerelease=true"))?.Data ?? Array.Empty<BaGetProject>();
        return projects
            .Select(x => new Project
            {
                Id = x.Id,
                Version = Version.Parse(x.Version),
                PublishedVersion = Version.Parse(x.Version)
            });
    }
}