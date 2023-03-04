using EventBot.Services.Bot.State;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace EventBot.Services.Bot.Modules
{
    public interface IBotInlineQueryReceiver
    {
        public Task<IEnumerable<InlineQueryResult>> BotOnInlineQueryReceived(InlineQuery inlineQuery);
    }
}
