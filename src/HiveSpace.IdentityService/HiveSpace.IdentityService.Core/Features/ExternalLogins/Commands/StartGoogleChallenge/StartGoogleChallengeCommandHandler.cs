using HiveSpace.Application.Shared.Handlers;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands;
using HiveSpace.IdentityService.Core.Features.ExternalLogins.Dtos;

namespace HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.StartGoogleChallenge;

public class StartGoogleChallengeCommandHandler
    : ICommandHandler<StartGoogleChallengeCommand, GoogleChallengeResult>
{
    public Task<GoogleChallengeResult> Handle(StartGoogleChallengeCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(new GoogleChallengeResult(
            AccountSessionHandlerBase.NormalizeApp(command.App),
            command.ReturnUrl,
            NormalizeCulture(command.Culture)));
    }

    private static string? NormalizeCulture(string? culture)
        => string.IsNullOrWhiteSpace(culture) ? null : culture.Trim().ToLowerInvariant();
}
