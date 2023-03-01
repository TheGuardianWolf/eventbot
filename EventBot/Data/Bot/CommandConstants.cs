namespace EventBot.Data.Bot
{
    public class CommandType
    {
        public const string Start = "/start";
        public const string MyEvents = "/myevents";
        public const string OldEvents = "/oldevents";
        //public const string Events = "/events";
        //public const string RefreshToken = "/refreshtoken";

        public static readonly CommandInfo[] Info =
        {
            new CommandInfo(Start, "Create an event"),
            new CommandInfo(MyEvents, "List upcoming events you are hosting"),
            new CommandInfo(OldEvents, "List previous events you have hosted"),
            //new CommandInfo(Events, "Access events portal"),
            //new CommandInfo(RefreshToken, "Regenerate your events token if it gets lost")
        };
    }
}
