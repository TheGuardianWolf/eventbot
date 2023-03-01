using EventBot.Repositories.DataSources;

namespace EventBot.Data.Events
{
    public class CalendarEvent : IMongoDataType
    {
        public Guid Id { get; set; }
        public Guid EventUserId { get; set; } // Poke this data store for user details
        public string ServiceEventId { get; set; } = ""; // Id of the event in the chat service, probably the message id
    }
}
