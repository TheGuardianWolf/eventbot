using EventBot.Services.Bot.State;
using Telegram.Bot.Types;

namespace EventBot.Services.Bot.Modules
{
    public interface IBotUpdateModule
    {
        public Task<bool> Process(Update update, IUserSessionState userSessionState);
    }

    public interface IBotMetaUpdateModule : IBotUpdateModule
    {
    }
}
