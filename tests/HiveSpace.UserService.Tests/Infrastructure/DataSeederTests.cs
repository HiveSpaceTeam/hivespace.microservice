using FluentAssertions;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Domain.Aggregates.Configuration;
using HiveSpace.UserService.Infrastructure;
using HiveSpace.UserService.Tests.Fixtures;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HiveSpace.UserService.Tests.Infrastructure;

public class DataSeederTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public DataSeederTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task SeedPlatformCurrencyPolicyAsync_WhenMissing_PersistsAndPublishesInitialPolicy()
    {
        var publisher = new FakePublisher();

        await DataSeeder.SeedPlatformCurrencyPolicyAsync(
            _fixture.DbContext,
            publisher,
            NullLogger.Instance,
            CancellationToken.None);

        var config = _fixture.DbContext.PlatformConfigs.Single();
        var currencies = _fixture.DbContext.PlatformCurrencies.OrderBy(x => x.SortOrder).ToArray();

        config.DefaultCurrencyCode.Should().Be("VND");
        currencies.Select(x => x.CurrencyCode).Should().Equal("VND", "USD", "EUR");
        publisher.PublishedConfigId.Should().Be(config.Id);
        publisher.PublishedCurrencies.Should().Equal("VND", "USD", "EUR");
    }

    [Fact]
    public async Task SeedPlatformCurrencyPolicyAsync_WhenExisting_RepublishesCurrentPolicy()
    {
        var publisher = new FakePublisher();
        _fixture.DbContext.PlatformCurrencies.RemoveRange(_fixture.DbContext.PlatformCurrencies);
        _fixture.DbContext.PlatformConfigs.RemoveRange(_fixture.DbContext.PlatformConfigs);
        await _fixture.DbContext.SaveChangesAsync();
        var config = PlatformConfig.CreateCurrencyPolicy("VND");
        var currencies = new[]
        {
            PlatformCurrency.CreateCurrency("VND", true, 0),
            PlatformCurrency.CreateCurrency("USD", true, 1)
        };

        _fixture.DbContext.PlatformConfigs.Add(config);
        _fixture.DbContext.PlatformCurrencies.AddRange(currencies);
        await _fixture.DbContext.SaveChangesAsync();

        await DataSeeder.SeedPlatformCurrencyPolicyAsync(
            _fixture.DbContext,
            publisher,
            NullLogger.Instance,
            CancellationToken.None);

        publisher.PublishedConfigId.Should().Be(config.Id);
        publisher.PublishedCurrencies.Should().Equal("VND", "USD");
    }

    private sealed class FakePublisher : IPlatformCurrencyConfigEventPublisher
    {
        public Guid PublishedConfigId { get; private set; }
        public string[] PublishedCurrencies { get; private set; } = [];

        public Task PublishPolicyUpdatedAsync(
            PlatformConfig config,
            IReadOnlyCollection<PlatformCurrency> currencies,
            CancellationToken cancellationToken = default)
        {
            PublishedConfigId = config.Id;
            PublishedCurrencies = currencies.Select(x => x.CurrencyCode).ToArray();
            return Task.CompletedTask;
        }
    }
}
