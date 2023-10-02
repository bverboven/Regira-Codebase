using MongoDB.Driver;

namespace Regira.DAL.MongoDB;

public static class MongoDbSettingsExtensions
{
    public static MongoClientSettings ToMongoClientSettings(this MongoDbSettings settings)
    {
        var mongoSettings = new MongoClientSettings
        {
            Server = new MongoServerAddress(settings.Host, int.Parse(settings.Port)),
            UseTls = settings.UseSecure,
            //UseSsl = settings.UseSsl
        };

        return mongoSettings;
    }
}