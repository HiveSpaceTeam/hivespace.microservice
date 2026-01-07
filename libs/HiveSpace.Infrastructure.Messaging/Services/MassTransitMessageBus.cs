using HiveSpace.Infrastructure.Messaging.Abstractions;
using MassTransit;

namespace HiveSpace.Infrastructure.Messaging.Services;

/// <summary>
/// Default MassTransit-based implementation of <see cref="IMessageBus"/>.
/// </summary>
public class MassTransitMessageBus : IMessageBus, IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    public MassTransitMessageBus(IPublishEndpoint publishEndpoint, ISendEndpointProvider sendEndpointProvider)
    {
        _publishEndpoint = publishEndpoint;
        _sendEndpointProvider = sendEndpointProvider;
    }

    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class
        => _publishEndpoint.Publish(message, cancellationToken);

    public async Task SendAsync<TCommand>(TCommand command, string? endpointName = null, CancellationToken cancellationToken = default) where TCommand : class, ICommand
    {
        if (string.IsNullOrWhiteSpace(endpointName))
        {
            await _publishEndpoint.Publish(command, cancellationToken);
            return;
        }

        var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{endpointName}"));
        await endpoint.Send(command, cancellationToken);
    }

    Task IEventPublisher.PublishAsync<TIntegrationEvent>(TIntegrationEvent @event, CancellationToken cancellationToken)
        => _publishEndpoint.Publish(@event, cancellationToken);
}

