using MongoDB.Driver;

namespace Regira.DAL.MongoDB;

public class MongoDbCommunicator
{
    private readonly string? _database;
    protected MongoClientSettings Settings { get; }
    private IMongoClient? _client;
    protected IMongoClient Client => _client ??= new MongoClient(Settings);
    private static IMongoDatabase? _dbContext;
    protected internal IMongoDatabase Database => _dbContext ??= Client.GetDatabase(_database);

    public MongoDbCommunicator(MongoDbSettings settings)
    {
        _database = settings.DatabaseName;
        Settings = settings.ToMongoClientSettings();
    }


    public async IAsyncEnumerable<string> ListCollectionNames()
    {
        var names = await Database.ListCollectionNamesAsync();
        while (await names.MoveNextAsync())
        {
            foreach (var name in names.Current)
            {
                yield return name;
            }
        }
    }
}