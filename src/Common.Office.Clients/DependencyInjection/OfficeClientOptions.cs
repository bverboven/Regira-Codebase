namespace Regira.Office.Clients.DependencyInjection;

public class OfficeClientOptions
{
    public string BaseUrl { get; set; } = null!;
    public string? ApiKey { get; set; }
}
