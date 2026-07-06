using FluentAssertions;
using HiveSpace.UserService.Application.Configuration.Queries.GetPlatformCurrencyConfig;
using HiveSpace.UserService.Domain.Aggregates.Configuration;
using HiveSpace.UserService.Infrastructure.Repositories;
using HiveSpace.UserService.Tests.Fixtures;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Configuration;

public class GetPlatformCurrencyConfigQueryHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;

    public GetPlatformCurrencyConfigQueryHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_ReturnsItemsDefaultAndVersion()
    {
        var config = PlatformConfig.CreateCurrencyPolicy("VND");
        _fixture.DbContext.PlatformConfigs.Add(config);
        _fixture.DbContext.PlatformCurrencies.AddRange(
            PlatformCurrency.CreateCurrency("VND", true, 0),
            PlatformCurrency.CreateCurrency("USD", false, 1),
            PlatformCurrency.CreateCurrency("EUR", false, 2));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetPlatformCurrencyConfigQueryHandler(
            new SqlPlatformConfigRepository(_fixture.DbContext),
            new SqlPlatformCurrencyRepository(_fixture.DbContext));

        var result = await handler.Handle(new GetPlatformCurrencyConfigQuery(), CancellationToken.None);

        result.DefaultCurrencyCode.Should().Be("VND");
        result.Currencies.Should().HaveCount(3);
        result.Version.Should().Be(1);
    }
}
