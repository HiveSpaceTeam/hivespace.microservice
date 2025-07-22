using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.Infrastructure.Persistence.Outbox;

/// <summary>
/// Represents an event message stored in the outbox table,
/// waiting to be published to a message broker.
/// </summary>
public class OutboxMessage : IAuditable 
{
    public Guid Id { get; set; }
    public DateTime OccurredOnUtc { get; set; }
    public string Type { get; set; } = string.Empty; // Full type name of the event
    public string Content { get; set; } = string.Empty; // Serialized event payload (JSON)
    public DateTime? ProcessedOnUtc { get; set; } // When the message was successfully published
    public string? Error { get; set; } // Any error during publishing
    public int Attempts { get; set; } // Number of publishing attempts

    // IAuditable properties (can be populated by AuditableInterceptor)
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public OutboxMessage(Guid id, DateTime occurredOnUtc, string type, string content)
    {
        Id = id;
        OccurredOnUtc = occurredOnUtc;
        Type = type;
        Content = content;
        Attempts = 0;
        CreatedAt = DateTimeOffset.UtcNow; // Set initially, interceptor will update
    }
} 