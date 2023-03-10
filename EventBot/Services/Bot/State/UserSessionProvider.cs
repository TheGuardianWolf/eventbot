using EventBot.Data.Bot;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using NodaTime;

namespace EventBot.Services.Bot.State
{
    public interface IUserSessionProvider
    {
        Task<UserSessionState> GetTelegramUserSession(long userId);
        Task<UserSessionState> GetUserSession(string sessionKey, CancellationToken cancellationToken = default);
        Task SetUserSession(UserSessionState userSession, CancellationToken cancellationToken = default);
    }

    public class UserSessionProvider : IUserSessionProvider
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

        public async Task<UserSessionState> GetTelegramUserSession(long userId)
        {
            var serviceName = ServiceName.Telegram;
            var serviceUserId = userId.ToString();

            var sessionKey = CreateSessionKey(serviceName, serviceUserId);

            return await GetUserSession(sessionKey);
        }

        public async Task SetUserSession(UserSessionState userSession, CancellationToken cancellationToken = default)
        {
            userSession.RefreshSessionExpiry();

            await _distributedCache.SetStringAsync(userSession.SessionKey, JsonConvert.SerializeObject(userSession), cancellationToken);
        }

        private string CreateSessionKey(string serviceName, string userId)
        {
            return $"{serviceName}.{userId}";
        }

        public async Task<UserSessionState> GetUserSession(string sessionKey, CancellationToken cancellationToken = default)
        {
            UserSessionState? state = null;

            // Gets if exists
            try
            {
                var cachedSession = await _distributedCache.GetStringAsync(sessionKey, cancellationToken);
                if (cachedSession != null)
                {
                    var cachedState = JsonConvert.DeserializeObject<UserSessionState>(cachedSession);

                    if (cachedState != null && !cachedState.IsSessionExpired())
                    {
                        state = cachedState;
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Couldn't deserialise key {} from cache", sessionKey);
            }

            if (state == null)
            {
                state = new UserSessionState(
                    _clock, sessionKey, _clock.GetCurrentInstant() + Duration.FromHours(cacheTimeHours));
            }

            await SetUserSession(state);

            return state;
        }
    }
}
