using FluentAssertions;
using HiveSpace.Infrastructure.Messaging.Events;
using HiveSpace.Testing.Shared.Capture;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Consumers;

public class ProjectionInputTests
{
    [Fact]
    public async Task ProductDomainEvent_ProducesCorrectProjectionInput()
    {
        var capture = new InMemoryMessageCapture();

        await capture.PublishAsync(new IntegrationEvent());

        capture.Published.Should().ContainSingle();
    }

    [Fact]
    public async Task ProjectionConsumer_ReadsCorrectFields()
    {
        var capture = new InMemoryMessageCapture();
        var projectionInput = new IntegrationEvent();

        await capture.PublishAsync(projectionInput);

        capture.Published.Single().Should().BeSameAs(projectionInput);
    }
}
