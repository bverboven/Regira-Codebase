namespace Regira.DAL.Abstractions;

public interface IDbSettings
{
    string Host { get; set; }
    string? DatabaseName { get; set; }
    string Port { get; set; }
    string? Username { get; set; }
    string? Password { get; set; }
    bool UseSecure { get; set; }

    string BuildConnectionString(params KeyValuePair<string, string>[] extraOptions);
    bool EqualsConnectionString<T>(T other)
        where T : class, IDbSettings, new();
    T Clone<T>()
        where T : class, IDbSettings, new();
}