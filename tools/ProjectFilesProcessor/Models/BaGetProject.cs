namespace Regira.ProjectFilesProcessor.Models;

public class BaGetResponse
{
    public ICollection<BaGetProject> Data { get; set; } = null!;
}
public class BaGetProject
{
    public string Id { get; set; } = null!;
    public string Version { get; set; } = null!;
    public string Registration { get; set; } = null!;
}