using HiveSpace.Application.Shared.Commands;
using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Exceptions;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Core.Features.Preferences.Commands.UpsertGroupPreference;

public class UpsertGroupPreferenceCommandHandler(
    IUserPreferenceRepository prefs,
    IUserContext userContext) : ICommandHandler<UpsertGroupPreferenceCommand>
{
    public async Task Handle(UpsertGroupPreferenceCommand request, CancellationToken cancellationToken)
    {
        var allowedGroups = NotificationEventGroup.ForRole(userContext);
        if (!allowedGroups.Contains(request.EventGroup))
            throw new InvalidFieldException(NotificationDomainErrorCode.InvalidEventGroup, nameof(request.EventGroup));

        var pref = UserGroupPreference.Create(
            userContext.UserId, request.Channel, request.EventGroup, request.Enabled);
        await prefs.UpsertGroupAsync(pref, cancellationToken);
    }
}
