namespace Entities.TestApi.Infrastructure;

public static class ApiConfiguration
{
    public static string ConnectionString = $"Filename={Path.Combine(Path.GetTempPath(), "test.db")}";
    public static string AttachmentsDirectory = Path.Combine(Path.GetTempPath(), "testing");
}