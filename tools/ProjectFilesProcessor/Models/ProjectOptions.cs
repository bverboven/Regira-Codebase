namespace Regira.ProjectFilesProcessor.Models;

public class ProjectOptions
{
    public string Prefix { get; set; } = "Regira";
    public string? ApiKey { get; set; }
    public string PackagesUri { get; set; } = null!;
    public string? SourceDirectory { get; set; }
}