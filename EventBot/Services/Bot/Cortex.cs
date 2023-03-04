using EventBot.Data.Events;
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
        private readonly IEventService _eventService;

        public Cortex(ILogger<Cortex> logger, 
            IUserSessionProvider userSession,
            IEventService eventService,
            IEnumerable<IBotUpdateModule> coreModules, 
            IEnumerable<IBotMetaUpdateModule> metaModule)
        {
            _logger = logger;
            _coreModules = coreModules;
            _metaModules = metaModule;
            _userSessionProvider = userSession;
            _eventService = eventService;
        }

        public async Task Process(Update update)
        {
            // Grab the session for each update
            long userId = 0;
            string userName = "";

            switch (update.Type)
            {
                case UpdateType.Message:
                    userId = update.Message!.From!.Id;
                    userName = update.Message!.From!.Username ?? "";
                    break;
                case UpdateType.InlineQuery:
                    userId = update.InlineQuery!.From!.Id;
                    userName = update.InlineQuery!.From!.Username ?? "";
                    break;
                case UpdateType.ChosenInlineResult:
                    userId = update.ChosenInlineResult!.From!.Id;
                    userName = update.ChosenInlineResult!.From!.Username ?? "";
                    break;
                case UpdateType.CallbackQuery:
                    userId = update.CallbackQuery!.From!.Id;
                    userName = update.CallbackQuery!.From!.Username ?? "";
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
            var user = await _eventService.GetTelegramUser(userId);
            if (user == null)
            {
                user = await _eventService.CreateTelegramUser(userId, userName);
            }
            if (user.UserName != userName)
            {
                user.UserName = userName;
                await _eventService.UpdateUser(user);
            }

            session.SetEventUser(user);

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
                    session.SetLastHandledModule(module.GetType().AssemblyQualifiedName);
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

    public static class UserSessionStateExtensions
    {
        private const string _lastHandledModuleKey = "_lastHandledModule";
        private const string _eventUserKey = "_eventUser";

        public static string? GetLastHandledModule(this IUserSessionState userSessionState)
        {
            if (!userSessionState.HasData(_lastHandledModuleKey)) 
            { 
                return null;
            }
            return userSessionState.GetData<string?>(_lastHandledModuleKey);
        }

        public static void SetLastHandledModule(this IUserSessionState userSessionState, string? moduleName)
        {
            userSessionState.SetData(_lastHandledModuleKey, moduleName);
        }

        public static EventUser? GetEventUser(this IUserSessionState userSessionState)
        {
            if (!userSessionState.HasData(_eventUserKey))
            {
                return null;
            }
            return userSessionState.GetData<EventUser>(_eventUserKey);
        }

        public static void SetEventUser(this IUserSessionState userSessionState, EventUser eventUser)
        {
            userSessionState.SetData(_eventUserKey, eventUser);
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
