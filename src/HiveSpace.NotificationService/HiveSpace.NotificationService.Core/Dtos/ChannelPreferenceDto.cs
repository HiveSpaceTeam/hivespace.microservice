using HiveSpace.NotificationService.Core.DomainModels;

namespace HiveSpace.NotificationService.Core.Dtos;

public record ChannelPreferenceDto(
    NotificationChannel           Channel,
    bool                          Enabled,
    IReadOnlyList<GroupPreferenceDto> Groups);
