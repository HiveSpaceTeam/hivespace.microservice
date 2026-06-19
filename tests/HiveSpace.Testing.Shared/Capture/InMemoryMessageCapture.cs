using HiveSpace.Infrastructure.Messaging.Abstractions;

namespace HiveSpace.Testing.Shared.Capture;

public sealed class InMemoryMessageCapture
{
    private readonly List<IIntegrationEvent> _published = [];

    public IReadOnlyList<IIntegrationEvent> Published => _published;

    public Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _published.Add(integrationEvent);
        return Task.CompletedTask;
    }
}
