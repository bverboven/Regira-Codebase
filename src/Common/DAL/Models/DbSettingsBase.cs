using Regira.DAL.Abstractions;

namespace Regira.DAL.Models;

public abstract class DbSettingsBase(
    string host,
    string? databaseName,
    string port,
    string? username = null,
    string? password = null,
    bool useSecure = true)
    : IDbSettings
{
    public string Host { get; set; } = host;
    public string? DatabaseName { get; set; } = databaseName;
    public string Port { get; set; } = port;
    public string? Username { get; set; } = username;
    public string? Password { get; set; } = password;
    public bool UseSecure { get; set; } = useSecure;


    public abstract string BuildConnectionString(params KeyValuePair<string, string>[] extraOptions);
    public virtual bool EqualsConnectionString<T>(T other)
        where T : class, IDbSettings, new()
    {
        return BuildConnectionString().Equals(other.BuildConnectionString());
    }

    public abstract T Clone<T>()
        where T : class, IDbSettings, new();
}