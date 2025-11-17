using System.Text.Json.Serialization;
using HiveSpace.Infrastructure.Messaging.Abstractions;

namespace HiveSpace.Infrastructure.Messaging.Events;

/// <summary>
/// Base integration event implementation used by the outbox pattern.
/// </summary>
public record IntegrationEvent : IIntegrationEvent
{
    public IntegrationEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
    }

    [JsonInclude]
    public Guid EventId { get; init; }

    [JsonInclude]
    public DateTimeOffset OccurredOn { get; init; }
}
