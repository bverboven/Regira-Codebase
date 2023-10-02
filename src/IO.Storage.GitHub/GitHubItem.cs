namespace Regira.IO.Storage.GitHub;

public class GitHubItem
{
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public GitHubItemType Type { get; set; }
    // ReSharper disable once InconsistentNaming
    public string? Download_Url { get; set; }
}