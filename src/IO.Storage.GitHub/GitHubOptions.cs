namespace Regira.IO.Storage.GitHub;

public class GitHubOptions
{
    public string Uri { get; set; } = null!;
    public string? Key { get; set; }
    public string? UserAgent { get; set; }
    public string Branch { get; set; } = "main";
    public string? CommitMessage { get; set; }
    public string? ContentPath { get; set; }
}
