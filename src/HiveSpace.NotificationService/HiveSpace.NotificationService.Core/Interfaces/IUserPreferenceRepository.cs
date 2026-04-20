using HiveSpace.NotificationService.Core.DomainModels;

namespace HiveSpace.NotificationService.Core.Interfaces;

public interface IUserPreferenceRepository
{
    Task<IReadOnlyList<NotificationChannel>> GetEnabledChannelsAsync(Guid userId, string eventType, CancellationToken ct = default);
    Task<IReadOnlyList<UserPreference>> GetAllForUserAsync(Guid userId, CancellationToken ct = default);
    Task UpsertAsync(UserPreference preference, CancellationToken ct = default);
}
