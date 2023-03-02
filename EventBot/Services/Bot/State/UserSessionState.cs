using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using System.Collections.Concurrent;

namespace EventBot.Services.Bot.State
{
    public interface IUserSessionState
    {
        Instant SessionExpiry { get; }
        string SessionKey { get; }

        T? GetData<T>(string key);
        bool HasData<T>(string key);
        void RefreshSessionExpiry();
        bool IsSessionExpired();
        void RemoveData(string key);
        void SetData<T>(string key, T value);
    }

    public class UserSessionState : IUserSessionState
    {

        public Instant SessionExpiry { get; private set; }
        public string SessionKey { get; private set; }
        private ConcurrentDictionary<string, string> SessionData { get; set; } = new ConcurrentDictionary<string, string>();
        private readonly IClock _clock;

        private object _editLock = new object();

        public UserSessionState(IClock clock, string sessionKey, Instant sessionExpiry)
        {
            _clock = clock;
            SessionKey = sessionKey;
            SessionExpiry = sessionExpiry;
        }

        public bool IsSessionExpired()
        {
            lock (_editLock)
            {
                if (_clock.GetCurrentInstant() > SessionExpiry)
                {
                    return true;
                }

                return false;
            }
        }

        public void RefreshSessionExpiry() => RefreshSessionExpiry(Duration.FromDays(1));

        public void RefreshSessionExpiry(Duration duration)
        {
            if (IsSessionExpired())
            {
                throw new InvalidOperationException($"Session has expired at epoch {SessionExpiry}");
            }

            // Slide by 24h
            lock (_editLock)
            {
                SessionExpiry = _clock.GetCurrentInstant() + duration;
            }
        }

        public bool HasData<T>(string key)
        {
            if (IsSessionExpired())
            {
                throw new InvalidOperationException($"Session has expired at epoch {SessionExpiry}");
            }

            return SessionData.ContainsKey(key);
        }

        public T? GetData<T>(string key)
        {
            if (IsSessionExpired())
            {
                throw new InvalidOperationException($"Session has expired at epoch {SessionExpiry}");
            }

            var val = JsonConvert.DeserializeObject<T>(SessionData[key]);

            return val;
        }

        public void SetData<T>(string key, T value)
        {
            if (IsSessionExpired())
            {
                throw new InvalidOperationException($"Session has expired at epoch {SessionExpiry}");
            }

            SessionData[key] = JsonConvert.SerializeObject(value);
        }

        public void RemoveData(string key)
        {
            if (IsSessionExpired())
            {
                throw new InvalidOperationException($"Session has expired at epoch {SessionExpiry}");
            }

            SessionData.Remove(key, out _);
        }
    }
}
