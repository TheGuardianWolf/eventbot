using EventBot.Repositories;
using Microsoft.Extensions.Caching.Distributed;

namespace EventBot.Services
{
    public interface IEventService
    {

    }

    public class EventService : IEventService
    {
        private readonly IDistributedCache _cache;
        private readonly ICalendarEventRepository _eventRepository;
        private readonly IEventUserRepository _userRepository;

        public EventService(ICalendarEventRepository calendarEventRepository, IEventUserRepository eventUserRepository, IDistributedCache cache)
        {
            _cache = cache;
            _eventRepository = calendarEventRepository;
            _userRepository = eventUserRepository;
        }
    }
}
