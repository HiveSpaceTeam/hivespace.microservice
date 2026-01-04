namespace HiveSpace.Infrastructure.Messaging.Abstractions;

/// <summary>
/// Abstraction over the underlying transport (MassTransit + RabbitMQ/Kafka).
/// </summary>
public interface IMessageBus
{
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class;

    Task SendAsync<TCommand>(TCommand command, string? endpointName = null, CancellationToken cancellationToken = default)
        where TCommand : class, ICommand;
}


