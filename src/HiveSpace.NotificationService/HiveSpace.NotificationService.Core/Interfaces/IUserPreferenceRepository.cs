using HiveSpace.NotificationService.Core.DomainModels;

namespace HiveSpace.NotificationService.Core.Interfaces;

public interface IUserPreferenceRepository
{
    Task<IReadOnlyList<NotificationChannel>> GetEnabledChannelsAsync(
        Guid userId, string eventGroup, CancellationToken ct = default);

    Task<IReadOnlyList<UserChannelPreference>> GetAllChannelPrefsAsync(
        Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<UserGroupPreference>> GetAllGroupPrefsAsync(
        Guid userId, CancellationToken ct = default);

    Task UpsertChannelAsync(UserChannelPreference preference, CancellationToken ct = default);

    Task UpsertGroupAsync(UserGroupPreference preference, CancellationToken ct = default);
}
