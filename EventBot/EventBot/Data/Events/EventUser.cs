using EventBot.Repositories.DataSources;

namespace EventBot.Data.Events
{
    public class EventUser : IMongoDataType
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string ServiceId { get; set; } = "";
        public string? AccessToken { get; set; }
    }
}
