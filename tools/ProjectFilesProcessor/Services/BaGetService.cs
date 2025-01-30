using System.Net.Http.Json;
using Regira.ProjectFilesProcessor.Models;

namespace Regira.ProjectFilesProcessor.Services;

public class BaGetService(HttpClient client)
{
    public async Task<IEnumerable<Project>> List()
    {
        var projects = (await client.GetFromJsonAsync<BaGetResponse>("search?take=1000&semVerLevel=2.0.0&prerelease=true"))?.Data ?? Array.Empty<BaGetProject>();
        return projects
            .Select(x => new Project
            {
                Id = x.Id,
                Version = Version.Parse(x.Version),
                PublishedVersion = Version.Parse(x.Version)
            });
    }
}