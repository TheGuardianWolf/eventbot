using EventBot.Data.Bot;
using Microsoft.Extensions.Caching.Distributed;
using NodaTime;

namespace EventBot.Services.Bot.State
{
    public class UserSessionProvider
    {
        private const int cacheTimeHours = 24;

        private readonly ILogger _logger;
        private readonly IClock _clock;
        private readonly IDistributedCache _distributedCache;

        public UserSessionProvider(ILogger<UserSessionProvider> logger, IClock clock, IDistributedCache distributedCache) 
        {
            _logger = logger;
            _clock = clock;
            _distributedCache = distributedCache;
        }

        public UserSessionState GetTelegramUserSession(long userId)
        {
            var serviceName = ServiceName.Telegram;
            var serviceUserId = userId.ToString();

            var sessionKey = CreateSessionKey(serviceName, serviceUserId);

            return GetUserSession(sessionKey);
        }

        private string CreateSessionKey(string serviceName, string userId)
        {
            return $"{serviceName}.{userId}";
        }

        private UserSessionState GetUserSession(string sessionKey)
        {
            // Gets if exists

            // Creates if not exists

        }
    }
}
