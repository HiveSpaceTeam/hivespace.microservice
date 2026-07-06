using FluentAssertions;
using HiveSpace.CatalogService.Api.Consumers.Sync;
using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Domain.Repositories.External;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Consumers.Sync;

public class PlatformCurrencyPolicySyncConsumerTests
{
    private readonly IPlatformCurrencyPolicyRefRepository _repository = Substitute.For<IPlatformCurrencyPolicyRefRepository>();
    private readonly PlatformCurrencyPolicySyncConsumer _consumer;

    public PlatformCurrencyPolicySyncConsumerTests()
    {
        _consumer = new PlatformCurrencyPolicySyncConsumer(
            _repository,
            Substitute.For<ILogger<PlatformCurrencyPolicySyncConsumer>>());
    }

    [Fact]
    public async Task Consume_WhenPolicyMissing_AddsProjection()
    {
        _repository.GetCurrentAsync(Arg.Any<CancellationToken>()).Returns((PlatformCurrencyPolicyRef?)null);
        var context = BuildContext(BuildMessage(version: 2));

        await _consumer.Consume(context);

        _repository.Received(1).Add(Arg.Is<PlatformCurrencyPolicyRef>(x =>
            x.DefaultCurrencyCode == "USD" &&
            x.Version == 2 &&
            x.IsCurrencyEnabled("USD") &&
            x.IsCurrencyEnabled("VND")));
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenVersionIsStale_DoesNothing()
    {
        _repository.GetCurrentAsync(Arg.Any<CancellationToken>())
            .Returns(new PlatformCurrencyPolicyRef(Guid.NewGuid(), "VND", 5, DateTimeOffset.UtcNow, ["VND", "USD"]));
        var context = BuildContext(BuildMessage(version: 4));

        await _consumer.Consume(context);

        _repository.DidNotReceive().Add(Arg.Any<PlatformCurrencyPolicyRef>());
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenNewerVersionArrives_UpdatesProjection()
    {
        var existing = new PlatformCurrencyPolicyRef(Guid.NewGuid(), "VND", 1, DateTimeOffset.UtcNow.AddMinutes(-5), ["VND"]);
        _repository.GetCurrentAsync(Arg.Any<CancellationToken>()).Returns(existing);
        var context = BuildContext(BuildMessage(version: 3));

        await _consumer.Consume(context);

        existing.DefaultCurrencyCode.Should().Be("USD");
        existing.Version.Should().Be(3);
        existing.IsCurrencyEnabled("USD").Should().BeTrue();
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static ConsumeContext<PlatformCurrencyPolicyUpdatedIntegrationEvent> BuildContext(
        PlatformCurrencyPolicyUpdatedIntegrationEvent message)
    {
        var context = Substitute.For<ConsumeContext<PlatformCurrencyPolicyUpdatedIntegrationEvent>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }

    private static PlatformCurrencyPolicyUpdatedIntegrationEvent BuildMessage(long version) =>
        new()
        {
            PolicyId = Guid.NewGuid(),
            DefaultCurrencyCode = "USD",
            Version = version,
            UpdatedAt = DateTimeOffset.UtcNow,
            Currencies =
            [
                new PlatformCurrencyPolicyUpdatedIntegrationEvent.CurrencyPolicyItem { CurrencyCode = "VND", IsEnabled = true },
                new PlatformCurrencyPolicyUpdatedIntegrationEvent.CurrencyPolicyItem { CurrencyCode = "USD", IsEnabled = true },
                new PlatformCurrencyPolicyUpdatedIntegrationEvent.CurrencyPolicyItem { CurrencyCode = "EUR", IsEnabled = false }
            ]
        };
}
