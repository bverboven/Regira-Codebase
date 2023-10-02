using Regira.DAL.Abstractions;

namespace Regira.DAL.Models;

public abstract class DbSettingsBase : IDbSettings
{
    public string Host { get; set; }
    public string? DatabaseName { get; set; }
    public string Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool UseSecure { get; set; }


    protected DbSettingsBase(string host, string? databaseName, string port, string? username = null, string? password = null, bool useSecure = true)
    {
        Host = host;
        DatabaseName = databaseName;
        Port = port;
        Username = username;
        Password = password;
        UseSecure = useSecure;
    }

    public abstract string BuildConnectionString(params KeyValuePair<string, string>[] extraOptions);
    public virtual bool EqualsConnectionString<T>(T other)
        where T : class, IDbSettings, new()
    {
        return BuildConnectionString().Equals(other.BuildConnectionString());
    }

    public abstract T Clone<T>()
        where T : class, IDbSettings, new();
}