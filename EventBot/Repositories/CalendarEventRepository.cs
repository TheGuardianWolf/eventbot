using EventBot.Data.Events;
using EventBot.Repositories.DataSources;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using MongoDB.Driver;
using Telegram.Bot.Types;

namespace EventBot.Repositories
{
    public interface ICalendarEventRepository
    {
        Task<CalendarEvent?> Get(Guid id);
        Task<bool> Update(CalendarEvent entity);
        Task<bool> Delete(Guid id);
        Task<IEnumerable<CalendarEvent>> GetByEventUser(Guid EventUserId);
        Task<CalendarEvent?> GetByServiceEvent(Guid EventUserId, string serviceEventId);
    }

    public class CalendarEventRepository : GUDRepositoryBase<CalendarEvent>, ICalendarEventRepository
    {
        public CalendarEventRepository(ILogger<CalendarEventRepository> logger, IMongoDataSource mongoDataSource)
            : base(logger, mongoDataSource) { }

        public async Task<CalendarEvent?> GetByServiceEvent(Guid EventUserId, string serviceEventId)
        {
            if (EventUserId == Guid.Empty)
            {
                throw new ArgumentException("Guid cannot be empty");
            }

            var cursor = await _collection.FindAsync(c => c.EventUserId == EventUserId);
            var entity = await cursor.FirstOrDefaultAsync();

            return entity;
        }

        public async Task<IEnumerable<CalendarEvent>> GetByEventUser(Guid EventUserId)
        {
            if (EventUserId == Guid.Empty)
            {
                throw new ArgumentException("Guid cannot be empty");
            }

            var cursor = await _collection.FindAsync(c => c.EventUserId == EventUserId);
            var entities = await cursor.ToListAsync();

            return entities;
        }
    }
}
