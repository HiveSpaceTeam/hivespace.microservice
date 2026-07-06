using FluentAssertions;
using HiveSpace.UserService.Application;
using HiveSpace.UserService.Application.Configuration.Dtos;
using HiveSpace.UserService.Application.Configuration.Queries.GetActivePlatformCurrencyConfig;
using HiveSpace.UserService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace HiveSpace.UserService.Tests.Application.Configuration;

public class PlatformCurrencyConfigQueryRegistrationTests
{
    [Fact]
    public void AddApplication_RegistersActiveCurrencyPolicyQueryHandler()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => Substitute.For<IPlatformConfigRepository>());
        services.AddScoped(_ => Substitute.For<IPlatformCurrencyRepository>());
        services.AddApplication();

        using var provider = services.BuildServiceProvider();

        var handler = provider.GetService<IRequestHandler<GetActivePlatformCurrencyConfigQuery, PlatformCurrencyConfigDto>>();

        handler.Should().NotBeNull();
    }
}
