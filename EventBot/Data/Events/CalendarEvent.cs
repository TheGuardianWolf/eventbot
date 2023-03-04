using EventBot.Repositories.DataSources;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using NodaTime;

namespace EventBot.Data.Events
{
    public class CalendarEvent : IMongoDataType
    {
        [BsonId]
        [BsonIgnoreIfDefault]
        public string Id { get; set; } = "";
        public string EventOwnerId { get; set; } = ""; // Poke this data store for user details
        public IEnumerable<string> ServiceEventIds { get; set; } = new List<string>(); // Id of the event in the chat service, will be multiple locations if shared
        public string? ServiceContextId { get; set; } = null; // Id of the context in the chat service, probably the group id
        public string EventName { get; set; } = "";
        public OffsetDate EventDate { get; set; }
        public OffsetTime EventTime { get; set; }
        public IEnumerable<string> Attendees = new List<string>();
        public string Location { get; set; } = "";
        public CalendarEventVisibility Visibility { get; set; }
        public bool Sharing { get; set; }
    }

    public enum CalendarEventVisibility
    {
        Private,
        Public
    }
}
