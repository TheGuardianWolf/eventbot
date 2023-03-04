using EventBot.Repositories.DataSources;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using NodaTime;
using shortid;

namespace EventBot.Data.Events
{
    public class EventUser : IMongoDataType
    {
        [BsonId]
        [BsonIgnoreIfDefault]
        public string Id { get; set; } = "";
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string ServiceName { get; set; } = "";
        public string? AccessToken { get; set; }
        public Instant CreationDateUtc { get; set; }

        public static string GenerateAccessToken()
        {
            return ShortId.Generate(new shortid.Configuration.GenerationOptions(length: 12));
        }
    }
}
