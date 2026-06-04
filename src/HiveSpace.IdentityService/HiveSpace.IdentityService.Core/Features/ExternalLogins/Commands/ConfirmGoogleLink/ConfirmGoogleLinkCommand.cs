using HiveSpace.Application.Shared.Commands;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;

namespace HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.ConfirmGoogleLink;

public record ConfirmGoogleLinkCommand(
    bool ConsentAccepted,
    string Password,
    string App,
    string? ReturnUrl,
    string? Culture,
    string LinkToken) : ICommand<SessionResponse>;
