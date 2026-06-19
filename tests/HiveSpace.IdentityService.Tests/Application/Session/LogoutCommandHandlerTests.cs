using FluentAssertions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.SignOut;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using NSubstitute;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Application.Session;

public class LogoutCommandHandlerTests
{
    [Fact]
    public async Task Handle_ClearsTokenCookieAndCsrfToken()
    {
        var tokenCookieService = Substitute.For<ITokenCookieService>();
        var csrfTokenService = Substitute.For<ICsrfTokenService>();

        var handler = new SignOutCommandHandler(tokenCookieService, csrfTokenService);
        await handler.Handle(new SignOutCommand(), CancellationToken.None);

        await tokenCookieService.Received(1).ClearAsync(Arg.Any<CancellationToken>());
        csrfTokenService.Received(1).Clear();
    }

    [Fact]
    public async Task Handle_CompletesSuccessfully()
    {
        var handler = new SignOutCommandHandler(
            Substitute.For<ITokenCookieService>(),
            Substitute.For<ICsrfTokenService>());

        var act = () => handler.Handle(new SignOutCommand(), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
