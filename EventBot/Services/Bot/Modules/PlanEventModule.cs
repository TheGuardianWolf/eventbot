using EventBot.Data.Bot;
using EventBot.Data.Events;
using EventBot.Data.Templates;
using EventBot.Services.Bot.Helpers;
using EventBot.Services.Bot.State;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace EventBot.Services.Bot.Modules
{
    public class PlanEventModule : IBotUpdateModule, IBotInlineQueryReceiver
    {
        private const string _planEventStateKey = "planEventState";
        private const string _planEventDataKey = "planEventData";

        private readonly ILogger _logger;
        private readonly ITelegramBotClient _client;
        private readonly TelegramConfiguration _tgConfig;
        private readonly IEventService _eventService;
        private readonly CalendarMarkupHelper _calendarHelper = new CalendarMarkupHelper();
        private readonly TimeMarkupHelper _timeMarkupHelper = new TimeMarkupHelper();

        public PlanEventModule(IOptions<TelegramConfiguration> tgConfig, ITelegramBotClient client, ILogger<PlanEventModule> logger, IEventService eventService)
        {
            _logger = logger;
            _client = client;
            _tgConfig = tgConfig.Value;
            _eventService = eventService;
        }

        public async Task<bool> Process(Update update, IUserSessionState userSessionState)
        {
            if (userSessionState.GetLastHandledModule() != typeof(PlanEventModule).AssemblyQualifiedName)
            {
                return false;
            }

            PlanEventState previousState = PlanEventState.GetName;
            if (userSessionState.HasData(_planEventStateKey))
            {
                previousState = userSessionState.GetData<PlanEventState>(_planEventStateKey);
            }

            switch (update.Type)
            {
                case UpdateType.Message:
                    return await BotOnMessageReceived(update.Message!, previousState, userSessionState);
                case UpdateType.ChosenInlineResult:
                    return await BotOnChosenInlineResultReceived(update.ChosenInlineResult!, previousState);
                case UpdateType.CallbackQuery:
                    return await BotOnCallbackQueryReceived(update.CallbackQuery!, previousState);
            };

            return false;
        }

        private async Task<bool> BotOnMessageReceived(Message message, PlanEventState previousState, IUserSessionState session)
        {
            if (message.Chat.Type != ChatType.Private)
            {
                return false;
            }

            if (message.Type != MessageType.Text)
            {
                return false;
            }

            if (string.IsNullOrEmpty(message.Text))
            {
                return false;
            }

            if (!new Regex($@"^{CommandType.Start}").IsMatch(message.Text))
            {
                return false;
            }


            await ProcessPlanEvent(message, previousState, session);
            return true;
        }

        public async Task<IEnumerable<InlineQueryResult>> BotOnInlineQueryReceived(InlineQuery inlineQuery)
        {
            // Plan new event

            // Share event code


            //var text = new InputTextMessageContent(
            //            string.Format(BotText.CovidPassCheck,
            //            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Program.NZTime).ToString("d"),
            //            inlineQuery.From.Username,
            //            _tgConfig.BotUsername,
            //            "No valid responses").Replace(".", "\\.").Replace("-", "\\-")
            //        );
            //text.ParseMode = ParseMode.MarkdownV2;
            //var response = new InlineQueryResultArticle(
            //        id: "/startcheckin",
            //        title: "Request Covid Pass status",
            //        inputMessageContent: text
            //    ); ;

            //response.ReplyMarkup = new InlineKeyboardMarkup(new[]
            //{
            //    new []
            //    {
            //        InlineKeyboardButton.WithCallbackData("Check in", $"/checkin")
            //    }
            // });

            InlineQueryResult[] results = {
            };

            return await Task.FromResult(results);
        }

        private async Task<bool> BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult, PlanEventState planEventState)
        {
            switch (chosenInlineResult.ResultId)
            {
                //case "/startcheckin":
                //    await CreatePollForCheckIn(chosenInlineResult);
                //    return true;
            }

            return false;
        }

        private async Task SendErrorMessage(Message message, string text)
        {
            await _client.SendTextMessageAsync(chatId: message.Chat.Id,
                            text: text.Replace(".", "\\."),
                            parseMode: ParseMode.MarkdownV2);
        }

        private async Task ProcessPlanEvent(Message message, PlanEventState previousState, IUserSessionState session)
        {
            var calendarEvent = new CalendarEvent();
            if (session.HasData(_planEventDataKey))
            {
                calendarEvent = session.GetData<CalendarEvent>(_planEventDataKey);
            }

            switch (previousState)
            {
                case PlanEventState.Start:
                    {
                        // Choose a name for your event
                        var stepInfo = string.Format(BotText.PlanEventStartInfo);

                        var sentMessage = await _client.SendTextMessageAsync(chatId: message.Chat.Id,
                            text: stepInfo.Replace(".", "\\."),
                            parseMode: ParseMode.MarkdownV2);

                        if (sentMessage != null)
                        {
                            session.SetData(_planEventStateKey, PlanEventState.GetName);
                        }
                    }
                    break;
                case PlanEventState.GetName:
                    {
                        var messageText = message.Text!;
                        if (string.IsNullOrWhiteSpace(messageText))
                        {
                            await SendErrorMessage(message, BotText.PlanEventInvalidNameError);
                            break;
                        }

                        // Accept name, build partial calendar event
                        var user = session.GetEventUser() ?? throw new NullReferenceException();
                        calendarEvent = new CalendarEvent
                        {
                            ServiceContextId = message.Chat.LinkedChatId.ToString(),
                            EventOwnerId = user.Id,
                            EventName = messageText
                        };
                        session.SetData(_planEventDataKey, calendarEvent);

                        // Choose the date if accepted
                        var stepInfo = string.Format(BotText.PlanEventSelectDateInfo);

                        var sentMessage = await _client.SendTextMessageAsync(chatId: message.Chat.Id,
                            text: stepInfo.Replace(".", "\\."),
                            parseMode: ParseMode.MarkdownV2,
                            replyMarkup: _calendarHelper.GetCalendarKeyboardMarkup());

                        if (sentMessage != null)
                        {
                            session.SetData(_planEventStateKey, PlanEventState.GetDate);
                        }
                    }
                    break;
                case PlanEventState.GetDate:
                    {
                        // Choose the Time
                        var stepInfo = string.Format(BotText.PlanEventSelectTimeInfo);

                        var sentMessage = await _client.SendTextMessageAsync(chatId: message.Chat.Id,
                            text: stepInfo.Replace(".", "\\."),
                            parseMode: ParseMode.MarkdownV2,
                            replyMarkup: _timeMarkupHelper.GetTimeKeyboardMarkup());

                        if (sentMessage != null)
                        {
                            session.SetData(_planEventStateKey, PlanEventState.GetTime);
                        }
                    }
                    break;
                case PlanEventState.GetTime:
                    {

                        // Choose the location
                        var stepInfo = string.Format(BotText.PlanEventSelectLocationInfo);

                        var sentMessage = await _client.SendTextMessageAsync(chatId: message.Chat.Id,
                            text: stepInfo.Replace(".", "\\."),
                            parseMode: ParseMode.MarkdownV2);

                        if (sentMessage != null)
                        {
                            session.SetData(_planEventStateKey, PlanEventState.GetLocation);
                        }
                    }
                    break;
                case PlanEventState.GetLocation:
                    {
                        var stepInfo = string.Format(BotText.PlanEventSelectDoneInfo);

                        var sentMessage = await _client.SendTextMessageAsync(chatId: message.Chat.Id,
                            text: stepInfo.Replace(".", "\\."),
                            parseMode: ParseMode.MarkdownV2);

                        if (sentMessage != null)
                        {
                            session.RemoveData(_planEventStateKey);
                        }
                    }
                    break;
            }
            
        }

        private async Task<bool> BotOnCallbackQueryReceived(CallbackQuery callbackQuery, PlanEventState planEventState)
        {
            switch (callbackQuery.Data)
            {
                //case "/checkin":
                //    await CheckIn(callbackQuery);
                //    return true;
            }

            return false;
        }

        //public async Task Check(Message message)
        //{
        //    var confirmKeyboard = new InlineKeyboardMarkup(new[]
        //    {
        //        new []
        //        {
        //            InlineKeyboardButton.WithSwitchInlineQuery("Check Covid pass status"),
        //        }
        //     });

        //    await _client.SendTextMessageAsync(chatId: message.Chat.Id,
        //                                        text: string.Format(BotText.CheckInfo,
        //                                            _tgConfig.BotUsername).Replace(".", "\\."),
        //                                        parseMode: ParseMode.MarkdownV2,
        //                                        replyMarkup: confirmKeyboard);
        //}

        //public async Task CheckIn(CallbackQuery callbackQuery)
        //{
        //    static async Task FailedCheckIn(ITelegramBotClient client, CallbackQuery callbackQuery)
        //    {
        //        await client.AnswerCallbackQueryAsync(
        //                   callbackQueryId: callbackQuery.Id,
        //                   text: "An error has occured :(");
        //    }

        //    if (callbackQuery.InlineMessageId is null)
        //    {
        //        await FailedCheckIn(_client, callbackQuery);
        //        return;
        //    }

        //    var isUserLinked = await _covidPassLinkerService.IsUserLinked(callbackQuery.From.Id);

        //    // Check if sender is verified
        //    if (isUserLinked)
        //    {
        //        // Create check existing poll
        //        var poll = await _covidPassPollService.GetPoll(callbackQuery.InlineMessageId);

        //        if (poll is null)
        //        {
        //            await FailedCheckIn(_client, callbackQuery);
        //            return;
        //        }

        //        var userId = callbackQuery.From.Id;
        //        var username = callbackQuery.From.Username ?? "";
        //        if (!poll.Participants.Any(x => x.Id == userId && x.Username == username))
        //        {
        //            var updatedPoll = await _covidPassPollService.AddParticipantToPoll(callbackQuery.InlineMessageId, callbackQuery.From.Id, callbackQuery.From.Username ?? "");

        //            if (updatedPoll is null)
        //            {
        //                await FailedCheckIn(_client, callbackQuery);
        //                return;
        //            }

        //            await SyncPassPollInfoWithMessage(updatedPoll);
        //        }

        //        await _client.AnswerCallbackQueryAsync(
        //            callbackQueryId: callbackQuery.Id,
        //            text: "You have checked in!");
        //    }
        //    else
        //    {
        //        await _client.AnswerCallbackQueryAsync(
        //            callbackQueryId: callbackQuery.Id,
        //            text: $"Your account is not linked to a Covid pass, please message @{_tgConfig.BotUsername} to link.");
        //    }
        //}

        protected enum PlanEventState
        {
            Start,
            GetName,
            GetDate,
            GetTime,
            GetLocation
        }
    }
}
