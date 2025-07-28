using System.Text.Json.Serialization;

namespace HiveSpace.Infrastructure.Messaging.Events;

public record IntegrationEvent
{
    public IntegrationEvent()
    {
        Id = Guid.NewGuid();
        CreationDate = DateTimeOffset.Now;
    }

    [JsonInclude]
    public Guid Id { get; set; }

    [JsonInclude]
    public DateTimeOffset CreationDate { get; set; }
}
