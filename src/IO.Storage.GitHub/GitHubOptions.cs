namespace Regira.IO.Storage.GitHub
{
    public class GitHubOptions
    {
        public string Uri { get; set; } = null!;
        public string? Key { get; set; }
        public string? UserAgent { get; set; }
    }
}
