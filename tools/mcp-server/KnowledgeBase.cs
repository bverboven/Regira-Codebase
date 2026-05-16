using System.Text.Json;

namespace Regira.McpServer;

public class KnowledgeBase
{
    public string Generated { get; init; } = "";
    public int PackageCount { get; init; }
    public int PackagesWithDocs { get; init; }
    public List<PackageEntry> Packages { get; init; } = [];
    public string Bootstrap { get; init; } = "";

    public static KnowledgeBase Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"knowledge-base.json not found at {path}.\nRun: cd tools/build-mcp-knowledge && npm run build",
                path);

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<KnowledgeBase>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        }) ?? throw new InvalidOperationException("Failed to deserialize knowledge-base.json");
    }
}

public class PackageEntry
{
    public string Id { get; init; } = "";
    public string Version { get; init; } = "";
    public string Description { get; init; } = "";
    public List<string> Tags { get; init; } = [];
    public Dictionary<string, string> Files { get; init; } = [];
}
