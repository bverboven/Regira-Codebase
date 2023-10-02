namespace Regira.ProjectFilesProcessor.Models;

public class ProjectOptions
{
    public string Prefix { get; set; } = "Regira";
    public string SourceDirectory { get; set; } = null!;
    public string PackagesUri { get; set; } = null!;
}