using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Application.Shared.Queries;
using HiveSpace.Core.Contexts;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Features.Preferences.Dtos;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Core.Features.Preferences.Queries.GetPreferences;

public class GetPreferencesQueryHandler(
    IUserPreferenceRepository prefs,
    IUserContext userContext) : IQueryHandler<GetPreferencesQuery, IReadOnlyList<ChannelPreferenceDto>>
{
    public async Task<IReadOnlyList<ChannelPreferenceDto>> Handle(GetPreferencesQuery request, CancellationToken cancellationToken)
    {
        var channelPrefs = await prefs.GetAllChannelPrefsAsync(userContext.UserId, cancellationToken);
        var groupPrefs   = await prefs.GetAllGroupPrefsAsync(userContext.UserId, cancellationToken);

        var channelDict = channelPrefs.ToDictionary(p => p.Channel);
        var groupDict   = groupPrefs.ToDictionary(p => (p.Channel, p.EventGroup));
        var groups      = NotificationEventGroup.ForRole(userContext);

        return Enum.GetValues<NotificationChannel>()
            .Select(ch => new ChannelPreferenceDto(
                ch,
                channelDict.TryGetValue(ch, out var cp) ? cp.Enabled : ch == NotificationChannel.InApp,
                groups
                    .Select(g => new GroupPreferenceDto(
                        g,
                        groupDict.TryGetValue((ch, g), out var gp) && gp.Enabled))
                    .ToList()))
            .ToList();
    }
}
