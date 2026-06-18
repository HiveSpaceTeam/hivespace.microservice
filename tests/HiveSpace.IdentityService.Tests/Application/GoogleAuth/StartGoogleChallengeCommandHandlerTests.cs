using FluentAssertions;
using HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.StartGoogleChallenge;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.GoogleAuth;

public class StartGoogleChallengeCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithAppAndReturnUrl_ReturnsNormalizedResult()
    {
        var handler = new StartGoogleChallengeCommandHandler();
        var result = await handler.Handle(
            new StartGoogleChallengeCommand("storefront", "https://app.hivespace.local/home", "VI"),
            CancellationToken.None);

        result.App.Should().Be("storefront");
        result.ReturnUrl.Should().Be("https://app.hivespace.local/home");
        result.Culture.Should().Be("vi", "NormalizeCulture lowercases the culture string");
    }

    [Fact]
    public async Task Handle_WithNullOptionalFields_ReturnsNullsInResult()
    {
        var handler = new StartGoogleChallengeCommandHandler();
        var result = await handler.Handle(
            new StartGoogleChallengeCommand("sellercenter", null, null),
            CancellationToken.None);

        result.App.Should().Be("sellercenter");
        result.ReturnUrl.Should().BeNull();
        result.Culture.Should().BeNull();
    }
}
