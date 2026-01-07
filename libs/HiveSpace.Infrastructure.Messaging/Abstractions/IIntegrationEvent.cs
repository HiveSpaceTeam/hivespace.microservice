using MassTransit;

namespace HiveSpace.Infrastructure.Messaging.Abstractions;

/// <summary>
/// Marker interface for integration events that leave a bounded context.
/// </summary>
[ExcludeFromTopology]
public interface IIntegrationEvent : IEvent
{
}


