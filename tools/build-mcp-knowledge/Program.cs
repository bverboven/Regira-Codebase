using System.Text.Json;
using System.Text.RegularExpressions;

var repoRoot = args.Length > 0 ? args[0] : FindRepoRoot();
var srcDir = Path.Combine(repoRoot, "src");
var agentsFile = Path.Combine(repoRoot, "ai", "AGENTS.md");
var outputFile = Path.Combine(repoRoot, "tools", "mcp-server", "knowledge-base.json");

var packages = CollectPackages(srcDir).OrderBy(p => p.Id).ToList();
var bootstrap = File.ReadAllText(agentsFile);
var withDocs = packages.Count(p => p.Files.Count > 0);

var knowledgeBase = new
{
    generated = DateTime.UtcNow.ToString("O"),
    packageCount = packages.Count,
    packagesWithDocs = withDocs,
    packages,
    bootstrap,
};

Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
File.WriteAllText(outputFile, JsonSerializer.Serialize(knowledgeBase, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Generated knowledge-base.json: {packages.Count} packages ({withDocs} with AI docs)");

// ── Helpers ──────────────────────────────────────────────────────────────────

static List<Package> CollectPackages(string srcDir)
{
    var result = new List<Package>();
    foreach (var dir in Directory.EnumerateDirectories(srcDir))
    {
        var csprojFiles = Directory.GetFiles(dir, "*.csproj");
        if (csprojFiles.Length == 0) continue;

        var meta = ReadCsprojMeta(csprojFiles[0]);
        var files = new Dictionary<string, string>();

        var aiDir = Path.Combine(dir, "ai");
        if (Directory.Exists(aiDir))
        {
            foreach (var file in Directory.GetFiles(aiDir, "*.md"))
            {
                var name = Path.GetFileName(file);
                // Skip subagent files — those go to .claude/agents/, not consumer guidance
                if (name.EndsWith("-agent.md", StringComparison.OrdinalIgnoreCase)) continue;
                var key = Path.GetFileNameWithoutExtension(file);
                files[key] = File.ReadAllText(file);
            }
        }

        result.Add(meta with { Files = files });
    }
    return result;
}

static Package ReadCsprojMeta(string path)
{
    var content = File.ReadAllText(path);
    string Get(string tag) =>
        Regex.Match(content, $@"<{tag}>([^<]*)</{tag}>").Groups[1].Value.Trim();

    return new Package(
        Id: Get("PackageId").Length > 0 ? Get("PackageId") : Path.GetFileNameWithoutExtension(path),
        Version: Get("Version").Length > 0 ? Get("Version") : "0.0.0",
        Description: Get("Description"),
        Tags: Get("PackageTags").Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
        Files: []
    );
}

static string FindRepoRoot()
{
    var dir = Directory.GetCurrentDirectory();
    while (dir is not null)
    {
        if (File.Exists(Path.Combine(dir, "Regira-Codebase.slnx")) ||
            Directory.Exists(Path.Combine(dir, "src")) && Directory.Exists(Path.Combine(dir, "ai")))
            return dir;
        dir = Path.GetDirectoryName(dir);
    }
    throw new InvalidOperationException("Could not locate repo root. Run from within the Regira-Codebase directory, or pass the path as an argument.");
}

record Package(string Id, string Version, string Description, List<string> Tags, Dictionary<string, string> Files);
