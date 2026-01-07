using MassTransit;

namespace HiveSpace.Infrastructure.Messaging.Abstractions;

/// <summary>
/// Base contract for any event emitted inside the platform.
/// </summary>
[ExcludeFromTopology]

public interface IEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredOn { get; }
}


