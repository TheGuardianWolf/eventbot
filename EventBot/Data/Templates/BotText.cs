namespace EventBot.Data.Templates
{
    public class BotText
    {
        public const string HelpInfo = @"This bot helps schedule events and count attendees.

It is based off of the very reliable @furryplansbot.

The bot will be providing more features for event collation and management through a web interface.

Please visit {0} for more details. Issues can be reported to @{1}.

Usage:
";

        public const string PlanEventStartInfo = @"I'll help you set up your event. First, give me the name of the event:";
        public const string PlanEventSelectDateInfo = @"Pick the date for your event:";
        public const string PlanEventSelectTimeInfo = @"Pick the time for your event:";
        public const string PlanEventSelectLocationInfo = @"Enter the location for your event, the full address is needed for accurate map location:";
        public const string PlanEventSelectDoneInfo = @"Your event is shown below:
**Name:** {0}
**Date:** {1}
**Location:** {2}
**Owner:** {3}";

        public const string PlanEventInvalidNameError = @"The event should have a name that is not blank, please try again.";
    }
}
