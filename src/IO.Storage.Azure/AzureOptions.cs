namespace Regira.IO.Storage.Azure;

public class AzureOptions
{
    public string? ContainerName { get; set; }
    public string? ConnectionString { get; set; }
    /// <summary>
    /// When true (default), the blob container is created automatically if it does not exist.
    /// Set to false to surface a clear error when the container is missing instead of silently creating it.
    /// </summary>
    public bool AutoCreate { get; set; } = true;
}