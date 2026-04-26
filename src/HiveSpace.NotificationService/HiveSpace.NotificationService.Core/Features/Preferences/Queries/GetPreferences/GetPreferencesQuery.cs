using HiveSpace.Application.Shared.Queries;
using HiveSpace.NotificationService.Core.Features.Preferences.Dtos;

namespace HiveSpace.NotificationService.Core.Features.Preferences.Queries.GetPreferences;

public record GetPreferencesQuery : IQuery<IReadOnlyList<ChannelPreferenceDto>>;
