using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Infrastructure.Messaging.Events;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace HiveSpace.Infrastructure.Persistence.Outbox;

/// <summary>
/// Represents an event message stored in the outbox table,
/// waiting to be published to a message broker.
/// </summary>
public class OutboxMessage : IAuditable
{
    private static readonly JsonSerializerOptions s_indentedOptions = new() { WriteIndented = true };
    private static readonly JsonSerializerOptions s_caseInsensitiveOptions = new() { PropertyNameCaseInsensitive = true };

    private OutboxMessage() { }

    public OutboxMessage(IntegrationEvent @event, Guid transactionId)
    {
        EventId = @event.EventId;
        EventCreationTime = @event.OccurredOn;
        EventTypeName = @event.GetType().FullName ?? string.Empty;
        Content = JsonSerializer.Serialize(@event, @event.GetType(), s_indentedOptions);
        State = EventStateEnum.NotPublished;
        TimesSent = 0;
        OperationId = transactionId;
    }

    public Guid EventId { get; private set; }
    
    [Required]
    public string EventTypeName { get; private set; } = string.Empty;

    [NotMapped]
    public string EventTypeShortName => EventTypeName.Split('.')?.Last() ?? string.Empty;

    [NotMapped]
    public IntegrationEvent? IntegrationEvent { get; private set; }

    public EventStateEnum State { get; set; }
    public int TimesSent { get; set; }

    public DateTimeOffset EventCreationTime { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    [Required]
    public string Content { get; private set; } = string.Empty;

    public Guid OperationId { get; private set; }

    /// <summary>
    /// Deserializes the JSON content to an IntegrationEvent instance.
    /// </summary>
    public OutboxMessage DeserializeJsonContent(Type type)
    {
        IntegrationEvent = JsonSerializer.Deserialize(Content, type, s_caseInsensitiveOptions) as IntegrationEvent;
        return this;
    }
}