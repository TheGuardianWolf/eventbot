using MongoDB.Driver;

namespace EventBot.Repositories.DataSources
{
    public interface IMongoDataType
    {
        string Id { get; set; }
    }

    public interface IMongoDataSource
    {
        IMongoDatabase Database { get; }
    }

    public class MongoDataSource : IMongoDataSource
    {
        public IMongoDatabase Database { get; }
        private readonly MongoClient _client;

        public MongoDataSource(string connectionString, string databaseName)
        {
            _client = new MongoClient(connectionString);
            Database = _client.GetDatabase(databaseName);
        }
    }
}
