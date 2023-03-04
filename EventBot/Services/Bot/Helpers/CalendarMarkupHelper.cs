using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace EventBot.Services.Bot.Helpers
{
    public class CalendarMarkupHelper
    {
        private const string PickDay = "/chPickDay";
        private const string PickMisc = "/chPickMisc";
        private const string PickNext = "/chPickNext";
        private const string PickPrev = "/chPickPrev";
        private const string PickConfirm = "/chPickConfirm";

        private const string SundayFormat = "Sun";
        private const string MondayFormat = "Mon";
        private const string TuesdayFormat = "Tue";
        private const string WednesdayFormat = "Wed";
        private const string ThursdayFormat = "Thu";
        private const string FridayFormat = "Fri";
        private const string SaturdayFormat = "Sat";

        private const string MonthYearFormat = "MMMM yyyy";

        private readonly string[] CalendarCallbacks = new[] { PickDay, PickMisc, PickNext, PickPrev, PickConfirm }; 

        public bool IsCalendarCallback(string? callbackCommand)
        {
            return CalendarCallbacks.Contains(callbackCommand);
        }

        public InlineKeyboardMarkup GetCalendarKeyboardMarkup(DateTime? selectedDate = null)
        {
            var date = selectedDate ?? DateTime.UtcNow;

            var calendarButtons = new List<IEnumerable<InlineKeyboardButton>>
            {
                // Month Year, actions
                new []
                {
                    InlineKeyboardButton.WithCallbackData("<", PickPrev),
                    InlineKeyboardButton.WithCallbackData(date.ToString(MonthYearFormat), PickMisc),
                    InlineKeyboardButton.WithCallbackData(">", PickNext)
                },
                // Weekdays
                new [] { MondayFormat, TuesdayFormat, WednesdayFormat, ThursdayFormat, FridayFormat, SaturdayFormat, SundayFormat }.Select(x => InlineKeyboardButton.WithCallbackData(x, PickMisc))
             };

            // Add weeks
            var currentDay = new DateTime(date.Year, date.Month, 1);
            for (var i = 0; i < 4; i++)
            {
                var week = new List<InlineKeyboardButton>();
                for (var j = 1; j <= 7; j++)
                {
                    if (currentDay.Month != date.Month || (int)currentDay.DayOfWeek != (j % 7))
                    {
                        week.Add(InlineKeyboardButton.WithCallbackData("", PickMisc));
                    }
                    else
                    {
                        var text = currentDay.Day == date.Day ? $"[{currentDay.Day}]" : $"{currentDay.Day}";
                        week.Add(InlineKeyboardButton.WithCallbackData(text, $"{PickDay} {currentDay.Day}"));
                        currentDay = currentDay.AddDays(1);
                    }
                }
                calendarButtons.Add(week);
            }

            // Add confirm
            calendarButtons.Add(new []
            {
                InlineKeyboardButton.WithCallbackData($"Confirm: {date.ToString("F")}", $"{PickConfirm} {date.ToString("O")}")
            });

            return new InlineKeyboardMarkup(calendarButtons);
        }
    }
}
