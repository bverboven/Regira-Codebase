using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;

namespace Regira.McpServer;

[McpServerToolType]
public class PackageTools(KnowledgeBase kb)
{
    [McpServerTool(Name = "list_packages")]
    [Description("List all available Regira NuGet packages, optionally filtered by a category keyword.")]
    public string ListPackages(
        [Description("Filter keyword, e.g. 'office', 'entities', 'storage', 'barcode'")] string? category = null)
    {
        var pkgs = kb.Packages.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(category))
            pkgs = pkgs.Where(p =>
                p.Id.Contains(category, StringComparison.OrdinalIgnoreCase) ||
                p.Tags.Any(t => t.Contains(category, StringComparison.OrdinalIgnoreCase)) ||
                p.Description.Contains(category, StringComparison.OrdinalIgnoreCase));

        var list = pkgs.Select(Summary).ToList();
        return $"{list.Count} packages{(category != null ? $" matching \"{category}\"" : "")}:\n\n{string.Join("\n", list)}";
    }

    [McpServerTool(Name = "get_package")]
    [Description("Get full documentation (instructions, examples, setup, signatures) for a specific Regira package.")]
    public string GetPackage(
        [Description("Package ID, e.g. 'Regira.Entities' or 'Regira.Office'")] string id)
    {
        var pkg = kb.Packages.FirstOrDefault(p =>
            p.Id.Equals(id, StringComparison.OrdinalIgnoreCase) ||
            p.Id.Contains(id, StringComparison.OrdinalIgnoreCase));

        if (pkg is null)
        {
            var suffix = id.Split('.').LastOrDefault() ?? id;
            var suggestions = kb.Packages
                .Where(p => p.Id.Contains(suffix, StringComparison.OrdinalIgnoreCase))
                .Take(5).Select(p => p.Id);
            var hint = suggestions.Any() ? $"\n\nDid you mean: {string.Join(", ", suggestions)}?" : "";
            return $"Package \"{id}\" not found.{hint}";
        }

        return FullText(pkg);
    }

    [McpServerTool(Name = "search_packages")]
    [Description("Search Regira packages by use case, technology, or keyword. Returns ranked results.")]
    public string SearchPackages(
        [Description("Search query, e.g. 'QR code', 'PDF generation', 'file upload', 'authentication'")] string query)
    {
        var results = kb.Packages
            .Select(p => (pkg: p, score: Score(p, query)))
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score)
            .Take(10)
            .ToList();

        if (results.Count == 0)
            return $"No packages found matching \"{query}\".";

        return $"Top matches for \"{query}\":\n\n{string.Join("\n", results.Select(x => Summary(x.pkg)))}";
    }

    [McpServerTool(Name = "recommend_packages")]
    [Description("Given a feature description, recommends which Regira packages to install and explains why.")]
    public string RecommendPackages(
        [Description("Plain-text description of what you want to build, e.g. 'shopping list API with QR codes'")] string feature)
    {
        var results = kb.Packages
            .Select(p => (pkg: p, score: Score(p, feature)))
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score)
            .Take(6)
            .ToList();

        if (results.Count == 0)
            return $"No specific packages found for \"{feature}\". Try list_packages to browse the full catalog.";

        var lines = results.Select(x =>
            $"- **{x.pkg.Id}** v{x.pkg.Version}{(x.pkg.Description.Length > 0 ? $" — {x.pkg.Description}" : "")}");

        return $"""
            Recommended packages for: *{feature}*

            {string.Join("\n", lines)}

            Use `get_package` for full setup instructions and code examples for any of these.
            Use `get_bootstrap_guide` for the consumer project setup workflow.
            """;
    }

    [McpServerTool(Name = "get_bootstrap_guide")]
    [Description("Returns the full Regira consumer bootstrap guide: NuGet config, project setup workflow, and AI guide loading rules.")]
    public string GetBootstrapGuide() => kb.Bootstrap;

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string Summary(PackageEntry p)
    {
        var tags = p.Tags.Count > 0 ? $" [{string.Join(", ", p.Tags)}]" : "";
        var desc = p.Description.Length > 0 ? $" — {p.Description}" : "";
        var docs = p.Files.Count > 0 ? $" ({p.Files.Count} guide files)" : "";
        return $"**{p.Id}** v{p.Version}{desc}{tags}{docs}";
    }

    private static string FullText(PackageEntry p)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {p.Id} v{p.Version}");
        if (p.Description.Length > 0) sb.AppendLine($"> {p.Description}");
        if (p.Tags.Count > 0) sb.AppendLine($"**Tags:** {string.Join(", ", p.Tags)}");
        sb.AppendLine();
        foreach (var (key, content) in p.Files)
        {
            sb.AppendLine($"## {key}");
            sb.AppendLine();
            sb.AppendLine(content);
        }
        return sb.ToString();
    }

    private static int Score(PackageEntry p, string query)
    {
        var terms = query.ToLowerInvariant()
            .Replace('-', ' ').Replace('_', ' ').Replace('.', ' ').Replace('/', ' ')
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var searchable = string.Join(" ",
            new[] { p.Id, p.Description, string.Join(" ", p.Tags) }
            .Concat(p.Files.Values))
            .ToLowerInvariant();

        var score = 0;
        foreach (var term in terms)
        {
            score += CountOccurrences(searchable, term);
            if (p.Id.Contains(term, StringComparison.OrdinalIgnoreCase)) score += 10;
            if (p.Tags.Any(t => t.Contains(term, StringComparison.OrdinalIgnoreCase))) score += 5;
        }
        return score;
    }

    private static int CountOccurrences(string text, string term)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(term, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += term.Length;
        }
        return count;
    }
}
