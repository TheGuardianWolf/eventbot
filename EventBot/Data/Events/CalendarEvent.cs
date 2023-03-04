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
        public string EventUserId { get; set; } = ""; // Poke this data store for user details
        public string ServiceEventId { get; set; } = ""; // Id of the event in the chat service, probably the message id
        public string? ServiceContextId { get; set; } = null; // Id of the context in the chat service, probably the group id
        public string EventName { get; set; } = "";
        public OffsetDate EventDate { get; set; }
        public OffsetTime EventTime { get; set; }
        public string Location { get; set; } = "";
    }
}
