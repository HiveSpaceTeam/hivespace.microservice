namespace HiveSpace.Infrastructure.Messaging.Abstractions;

/// <summary>
/// Strongly typed abstraction for publishing integration events.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TIntegrationEvent>(TIntegrationEvent @event, CancellationToken cancellationToken = default)
        where TIntegrationEvent : class, IIntegrationEvent;
}


