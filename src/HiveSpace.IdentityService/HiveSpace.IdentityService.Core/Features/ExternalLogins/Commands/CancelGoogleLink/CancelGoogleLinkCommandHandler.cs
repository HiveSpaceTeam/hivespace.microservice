using HiveSpace.Application.Shared.Handlers;
using HiveSpace.IdentityService.Core.Interfaces.Services;

namespace HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.CancelGoogleLink;

public class CancelGoogleLinkCommandHandler(IPendingGoogleLinkStore pendingGoogleLinkStore)
    : ICommandHandler<CancelGoogleLinkCommand>
{
    public async Task Handle(CancelGoogleLinkCommand command, CancellationToken cancellationToken)
    {
        await pendingGoogleLinkStore.GetRequiredAsync(command.LinkToken, cancellationToken);
        await pendingGoogleLinkStore.ClearAsync(cancellationToken);
    }
}
