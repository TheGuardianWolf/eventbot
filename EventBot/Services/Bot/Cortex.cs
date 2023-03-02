using EventBot.Services.Bot.Modules;
using EventBot.Services.Bot.State;
using NodaTime;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;

namespace EventBot.Services.Bot
{
    public class Cortex
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<IBotUpdateModule> _coreModules;
        private readonly IEnumerable<IBotMetaUpdateModule> _metaModules;
        private readonly IUserSessionProvider _userSessionProvider;

        public Cortex(ILogger<Cortex> logger, 
            IUserSessionProvider userSession, 
            IEnumerable<IBotUpdateModule> coreModules, 
            IEnumerable<IBotMetaUpdateModule> metaModule)
        {
            _logger = logger;
            _coreModules = coreModules;
            _metaModules = metaModule;
            _userSessionProvider = userSession;
        }

        public async Task Process(Update update)
        {
            // Grab the session for each update
            long userId = 0;

            switch (update.Type)
            {
                case UpdateType.Message:
                    userId = update.Message!.From!.Id;
                    break;
                case UpdateType.InlineQuery:
                    userId = update.InlineQuery!.From!.Id;
                    break;
                case UpdateType.ChosenInlineResult:
                    userId = update.ChosenInlineResult!.From!.Id;
                    break;
                case UpdateType.CallbackQuery:
                    userId = update.CallbackQuery!.From!.Id;
                    break;
                case UpdateType.EditedMessage:
                case UpdateType.ChannelPost:
                case UpdateType.EditedChannelPost:
                case UpdateType.ShippingQuery:
                case UpdateType.PreCheckoutQuery:
                case UpdateType.Poll:
                case UpdateType.PollAnswer:
                case UpdateType.MyChatMember:
                case UpdateType.ChatMember:
                case UpdateType.ChatJoinRequest:
                    await UnhandledUpdateHandlerAsync(update);
                    return;
            }
            
            if (userId == 0)
            {
                await HandleErrorAsync(new InvalidOperationException("User id has not been set"));
                return;
            }

            var session = await _userSessionProvider.GetTelegramUserSession(userId);

            var handled = false;
            foreach (var module in _coreModules.Concat(_metaModules))
            {
                try
                {
                    handled = await module.Process(update, session);
                }
                catch (Exception exception)
                {
                    await HandleErrorAsync(exception);
                }

                if (handled)
                {
                    await _userSessionProvider.SetUserSession(session);
                    return;
                }
            }

            await UnhandledUpdateHandlerAsync(update);
        }

        private Task UnhandledUpdateHandlerAsync(Update update)
        {
            _logger?.LogDebug("Unhandled update type: {updateType}", update.Type);
            return Task.CompletedTask;
        }

        private Task HandleErrorAsync(Exception exception)
        {
            var ErrorMessage = exception switch
            {
                //ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger?.LogDebug("HandleError: {ErrorMessage}", ErrorMessage);
            return Task.CompletedTask;
        }
    }

    public static class CortexServiceExtensions
    {
        public static IServiceCollection AddBotCortex(this IServiceCollection services)
        {
            services.AddScoped<IUserSessionProvider, UserSessionProvider>();
            services.AddTransient<Cortex>();
            services.AddTransient<IBotUpdateModule, HelpModule>();
            services.AddTransient<IBotMetaUpdateModule, InlineQueryResultCollectorModule>();

            return services;
        }
    }
}
