namespace Regira.ProjectFilesProcessor.Models;

public class Project
{
    public string? Id { get; set; }
    public string ProjectFile { get; set; } = null!;
    public string? RootNamespace { get; set; }
    public string? AssemblyName { get; set; }
    public ICollection<string> TargetFrameworks { get; set; } = new HashSet<string> { "net8.0" };
    public ICollection<string> Authors { get; set; } = new HashSet<string>() { "B. Verboven" };
    public Version Version { get; set; } = new("1.0.0");
    public Version? PublishedVersion { get; set; }
    public bool? IsClassLibrary { get; set; }
    public bool GeneratePackageOnBuild { get; set; }
    public bool HasChanged { get; set; }

    public ICollection<string> Dependencies { get; set; } = Array.Empty<string>();
}