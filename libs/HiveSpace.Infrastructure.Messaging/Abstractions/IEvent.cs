namespace HiveSpace.Infrastructure.Messaging.Abstractions;

/// <summary>
/// Base contract for any event emitted inside the platform.
/// </summary>
public interface IEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredOn { get; }
}


