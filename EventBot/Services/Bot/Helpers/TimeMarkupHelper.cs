using Telegram.Bot.Types.ReplyMarkups;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EventBot.Services.Bot.Helpers
{
    public class TimeMarkupHelper
    {
        private const string PickHour = "/thPickHour";
        private const string PickMinute = "/thPickMinute";
        private const string PickAmpm = "/thPickAmpm";
        private const string PickConfirm = "/thPickConfirm";

        private readonly string[] TimeCallbacks = new[] { PickHour, PickMinute, PickAmpm, PickConfirm };

        public bool IsTimeCallback(string? callbackCommand)
        {
            return TimeCallbacks.Contains(callbackCommand);
        }

        public InlineKeyboardMarkup GetTimeKeyboardMarkup(DateTime? selectedTime = null)
        {
            var date = (selectedTime ?? DateTime.UtcNow).AddMinutes(5);
            var nearestMin = 5 * (int)(Math.Floor((double)Math.Abs(date.Minute / 5)));
            date = new DateTime(date.Year, date.Month, date.Day, date.Hour, nearestMin, 0);

            var timeKeyboard = new List<IEnumerable<InlineKeyboardButton>>();

            var row1 = new List<InlineKeyboardButton>();
            for (var i = 1; i <= 6; i++)
            {
                var text = (date.Hour % 12) == i ? $"[{i}]" : $"{i}";

                row1.Add(InlineKeyboardButton.WithCallbackData(text, $"{PickHour} {i}"));
            }
            timeKeyboard.Add(row1);

            var row2 = new List<InlineKeyboardButton>();
            for (var i = 7; i <= 12; i++)
            {
                var twelveHourTime = (date.Hour % 12);

                var text = $"{i}";
                if (twelveHourTime == i || twelveHourTime == 0 && i == 12)
                {
                    text = $"[{i}]";
                }
                row2.Add(InlineKeyboardButton.WithCallbackData(text, $"{PickHour} {i}"));
            }
            timeKeyboard.Add(row2);

            var row3 = new List<InlineKeyboardButton>();
            for (var i = 0; i <= 25; i += 5)
            {
                var text = date.Minute == i ? $"[{i}]" : $"{i}";

                row3.Add(InlineKeyboardButton.WithCallbackData(text, $"{PickMinute} {i}"));
            }
            timeKeyboard.Add(row3);

            var row4 = new List<InlineKeyboardButton>();
            for (var i = 30; i <= 55; i += 5)
            {
                var text = date.Minute == i ? $"[{i}]" : $"{i}";

                row3.Add(InlineKeyboardButton.WithCallbackData(text, $"{PickMinute} {i}"));
            }
            timeKeyboard.Add(row4);

            timeKeyboard.AddRange(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("am", $"${PickAmpm} am"),
                    InlineKeyboardButton.WithCallbackData("pm", $"${PickAmpm} pm")
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData($"Confirm {date.ToString("t")}", $"{PickConfirm} {date.ToString("O")}")
                }
            });

            return new InlineKeyboardMarkup(timeKeyboard);
        }
    }
}
