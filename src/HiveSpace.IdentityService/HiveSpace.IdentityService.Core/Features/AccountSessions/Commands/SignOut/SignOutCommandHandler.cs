using HiveSpace.Application.Shared.Handlers;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Services;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.SignOut;

public class SignOutCommandHandler(
    ITokenCookieService tokenCookieService,
    ICsrfTokenService csrfTokenService)
    : ICommandHandler<SignOutCommand>
{
    public async Task Handle(SignOutCommand command, CancellationToken cancellationToken)
    {
        await tokenCookieService.ClearAsync(cancellationToken);
        csrfTokenService.Clear();
    }
}
