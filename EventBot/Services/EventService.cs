using EventBot.Data.Bot;
using EventBot.Data.Events;
using EventBot.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using NodaTime;
using shortid;

namespace EventBot.Services
{
    public interface IEventService
    {
        Task<EventUser> CreateTelegramUser(long userId, string userName);
        Task<EventUser?> GetTelegramUser(long userId);
        Task<EventUser> UpdateUser(EventUser user);
    }

    public class EventService : IEventService
    {
        private readonly ICalendarEventRepository _eventRepository;
        private readonly IEventUserRepository _userRepository;
        private readonly IClock _clock;

        public EventService(IClock clock, ICalendarEventRepository calendarEventRepository, IEventUserRepository eventUserRepository)
        {
            _eventRepository = calendarEventRepository;
            _userRepository = eventUserRepository;
            _clock = clock;
        }

        public async Task<EventUser?> GetTelegramUser(long userId)
        {
            return await _userRepository.GetByServiceName(ServiceName.Telegram, userId.ToString());
        }

        public async Task<EventUser> CreateTelegramUser(long userId, string userName)
        {
            return await UpdateUser(new EventUser
            {
                UserId = userId.ToString(),
                UserName = userName,
                ServiceName = ServiceName.Telegram,
                CreationDateUtc = _clock.GetCurrentInstant()
            });
        }

        public async Task<EventUser> UpdateUser(EventUser user)
        {
            var id = await _userRepository.Update(new EventUser
            {
                Id = user.Id,
                ServiceName = user.ServiceName,
                UserName = user.UserName,
                UserId = user.UserId,
                AccessToken = user.AccessToken ?? EventUser.GenerateAccessToken(),
                CreationDateUtc = user.CreationDateUtc,
            });
            user.Id = id;

            return user;
        }
    }
}
