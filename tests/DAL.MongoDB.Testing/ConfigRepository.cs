using Regira.DAL.MongoDB;
using Regira.DAL.MongoDB.Abstractions;
using Regira.Serializing.Abstractions;

namespace DAL.MongoDB.Testing;

public class Config
{
    public string? ConfigId { get; set; }
    public string? Key { get; set; }
    public object? Value { get; set; }
}
public class ConfigRepository : MongoDbRepositoryBase<Config>
{
    public ConfigRepository(MongoDbCommunicator communicator, ISerializer serializer)
        : base(communicator, serializer, x => x.ConfigId, (x, id) => x.ConfigId = id, "config") { }
}