using EventBot.Data.Bot;
using EventBot.Data.Templates;
using EventBot.Services.Bot.State;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace EventBot.Services.Bot.Modules
{
    public class HelpModule : IBotUpdateModule
    {
        private readonly ILogger _logger;
        private readonly ITelegramBotClient _client;
        private readonly TelegramConfiguration _tgConfig;

        public HelpModule(IOptions<TelegramConfiguration> tgConfig, 
            ITelegramBotClient client, 
            ILogger<HelpModule> logger)
        {
            _logger = logger;
            _client = client;
            _tgConfig = tgConfig.Value;
        }

        public async Task<bool> Process(Update update, IUserSessionState userSessionState)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    return await BotOnMessageReceived(update.Message ?? throw new NullReferenceException());
            };

            return false;
        }

        private async Task<bool> BotOnMessageReceived(Message message)
        {
            if (message.Chat.Type != ChatType.Private)
            {
                return false;
            }

            if (message.Type == MessageType.Text)
            {
                switch (message.Text!.Split(' ')[0])
                {
                    case CommandType.Start:
                        await Help(message);
                        return true;
                };
            }

            return false;
        }

        public async Task Help(Message message)
        {
            var userCommandInfo = CommandType.Info.AsEnumerable();

            var usage = string.Format(BotText.HelpInfo, _tgConfig.Hostname) + string.Join('\n', userCommandInfo.Select(x => $"{x.Command} - {x.Description}"));

            await _client.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: usage,
                                                  replyMarkup: new ReplyKeyboardRemove());
        }
    }
}
