using EventBot.Data.Events;
using EventBot.Repositories.DataSources;
using MongoDB.Driver;

namespace EventBot.Repositories
{
    public interface IEventUserRepository
    {
        Task<EventUser?> Get(Guid id);
        Task<Guid> Update(EventUser entity);
        Task<bool> Delete(Guid id);
        Task<EventUser?> GetByAccessToken(string accessToken);
        Task<EventUser?> GetByServiceName(string serviceName, string userId);
    }

    public class EventUserRepository : GUDRepositoryBase<EventUser>, IEventUserRepository
    {

        public EventUserRepository(ILogger<CalendarEventRepository> logger, IMongoDataSource mongoDataSource)
            : base(logger, mongoDataSource) { }

        public async Task<EventUser?> GetByAccessToken(string accessToken)
        {
            var cursor = await _collection.FindAsync(c => string.CompareOrdinal(c.AccessToken, accessToken) == 0);
            var entity = cursor.FirstOrDefault();
            if (entity is null)
            {
                return null;
            }

            return entity;
        }

        public async Task<EventUser?> GetByServiceName(string serviceName, string userId)
        {
            var cursor = await _collection.FindAsync(c => c.ServiceName == serviceName && c.UserId == userId);
            var entity = await cursor.FirstOrDefaultAsync();

            return entity;
        }
    }
}
