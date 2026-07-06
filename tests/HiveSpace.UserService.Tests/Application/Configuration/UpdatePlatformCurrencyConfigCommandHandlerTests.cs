using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Application.Configuration.Commands.UpdatePlatformCurrencyConfig;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Domain.Aggregates.Configuration;
using HiveSpace.UserService.Infrastructure.Repositories;
using HiveSpace.UserService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Configuration;

public class UpdatePlatformCurrencyConfigCommandHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public UpdatePlatformCurrencyConfigCommandHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_DisablingCurrentDefaultWithoutReplacement_ThrowsConflict()
    {
        var config = PlatformConfig.CreateCurrencyPolicy("VND");
        _fixture.DbContext.PlatformConfigs.Add(config);
        _fixture.DbContext.PlatformCurrencies.AddRange(
            PlatformCurrency.CreateCurrency("VND", true, 0),
            PlatformCurrency.CreateCurrency("USD", false, 1),
            PlatformCurrency.CreateCurrency("EUR", false, 2));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdatePlatformCurrencyConfigCommandHandler(
            new SqlPlatformConfigRepository(_fixture.DbContext),
            new SqlPlatformCurrencyRepository(_fixture.DbContext),
            new FakePublisher());

        var act = () => handler.Handle(new UpdatePlatformCurrencyConfigCommand(
            [
                new PlatformCurrencyConfigItemRequest("VND", false),
                new PlatformCurrencyConfigItemRequest("USD", true),
                new PlatformCurrencyConfigItemRequest("EUR", false)
            ],
            "VND",
            1), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_ValidConfig_PersistsAndPublishesEvent()
    {
        var config = PlatformConfig.CreateCurrencyPolicy("VND");
        _fixture.DbContext.PlatformConfigs.Add(config);
        _fixture.DbContext.PlatformCurrencies.AddRange(
            PlatformCurrency.CreateCurrency("VND", true, 0),
            PlatformCurrency.CreateCurrency("USD", false, 1),
            PlatformCurrency.CreateCurrency("EUR", false, 2));
        await _fixture.DbContext.SaveChangesAsync();

        var publisher = new FakePublisher();
        var handler = new UpdatePlatformCurrencyConfigCommandHandler(
            new SqlPlatformConfigRepository(_fixture.DbContext),
            new SqlPlatformCurrencyRepository(_fixture.DbContext),
            publisher);

        var result = await handler.Handle(new UpdatePlatformCurrencyConfigCommand(
            [
                new PlatformCurrencyConfigItemRequest("VND", true),
                new PlatformCurrencyConfigItemRequest("USD", true),
                new PlatformCurrencyConfigItemRequest("EUR", false)
            ],
            "USD",
            1), CancellationToken.None);

        result.DefaultCurrencyCode.Should().Be("USD");
        result.Version.Should().Be(2);
        publisher.Published.Should().BeTrue();
    }

    private sealed class FakePublisher : IPlatformCurrencyConfigEventPublisher
    {
        public bool Published { get; private set; }

        public Task PublishPolicyUpdatedAsync(PlatformConfig config, IReadOnlyCollection<PlatformCurrency> currencies, CancellationToken cancellationToken = default)
        {
            Published = true;
            return Task.CompletedTask;
        }
    }
}
