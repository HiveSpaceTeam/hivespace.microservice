using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.CancelGoogleLink;

public record CancelGoogleLinkCommand(string LinkToken) : ICommand;
