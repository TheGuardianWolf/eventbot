using EventBot.Data.Events;
using EventBot.Repositories.DataSources;
using MongoDB.Driver;

namespace EventBot.Repositories
{
    public class GUDRepositoryBase<T> where T : IMongoDataType
    {
        protected readonly IMongoCollection<T> _collection;
        private readonly ILogger _logger;

        public GUDRepositoryBase(ILogger logger, IMongoDataSource mongoDataSource)
            :this(typeof(T).Name, logger, mongoDataSource)
        {
        }

        public GUDRepositoryBase(string collectionName, ILogger logger, IMongoDataSource mongoDataSource)
        {
            _logger = logger;
            _collection = mongoDataSource.Database.GetCollection<T>(collectionName);
        }

        public async Task<T?> Get(Guid id)
        {
            var cursor = await _collection.FindAsync(c => c.Id == id);
            var entity = cursor.FirstOrDefault();
            if (entity is null)
            {
                return default(T);
            }

            return entity;
        }

        public async Task<bool> Update(T entity)
        {
            var result = await _collection.ReplaceOneAsync(c => c.Id == entity.Id, entity, new ReplaceOptions
            {
                IsUpsert = true
            });

            return result is not null;
        }

        public async Task<bool> Delete(Guid id)
        {
            var result = await _collection.DeleteOneAsync(c => c.Id == id);

            return result is not null;
        }
    }
}
