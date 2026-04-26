using HiveSpace.NotificationService.Core.DomainModels;

namespace HiveSpace.NotificationService.Core.Features.Preferences.Dtos;

public record ChannelPreferenceDto(
    NotificationChannel           Channel,
    bool                          Enabled,
    IReadOnlyList<GroupPreferenceDto> Groups);
