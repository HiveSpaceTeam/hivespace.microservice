using HiveSpace.Application.Shared.Commands;
using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Core.Features.Preferences.Commands.UpsertChannelPreference;

public class UpsertChannelPreferenceCommandHandler(
    IUserPreferenceRepository prefs,
    IUserContext userContext) : ICommandHandler<UpsertChannelPreferenceCommand>
{
    public async Task Handle(UpsertChannelPreferenceCommand request, CancellationToken cancellationToken)
    {
        var pref = UserChannelPreference.Create(userContext.UserId, request.Channel, request.Enabled);
        await prefs.UpsertChannelAsync(pref, cancellationToken);
    }
}
