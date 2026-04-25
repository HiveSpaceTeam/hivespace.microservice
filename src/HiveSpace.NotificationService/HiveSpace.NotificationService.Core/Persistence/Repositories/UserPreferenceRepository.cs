using Microsoft.EntityFrameworkCore;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Core.Persistence.Repositories;

public class UserPreferenceRepository(NotificationDbContext db) : IUserPreferenceRepository
{
    private static readonly IReadOnlyList<NotificationChannel> DefaultChannels =
        [NotificationChannel.InApp];

    public async Task<IReadOnlyList<NotificationChannel>> GetEnabledChannelsAsync(
        Guid userId, string eventGroup, CancellationToken ct = default)
    {
        var disabledChannelMasters = await db.UserChannelPreferences
            .Where(p => p.UserId == userId && !p.Enabled)
            .Select(p => p.Channel)
            .ToListAsync(ct);

        var disabledGroups = await db.UserGroupPreferences
            .Where(p => p.UserId == userId && p.EventGroup == eventGroup && !p.Enabled)
            .Select(p => p.Channel)
            .ToListAsync(ct);

        if (disabledChannelMasters.Count == 0 && disabledGroups.Count == 0)
        {
            var hasRows = await db.UserChannelPreferences.AnyAsync(p => p.UserId == userId, ct)
                       || await db.UserGroupPreferences.AnyAsync(p => p.UserId == userId, ct);
            if (!hasRows) return DefaultChannels;
        }

        var disabled = disabledChannelMasters.Union(disabledGroups).ToHashSet();
        return Enum.GetValues<NotificationChannel>()
                   .Where(ch => !disabled.Contains(ch))
                   .ToList();
    }

    public async Task<IReadOnlyList<UserChannelPreference>> GetAllChannelPrefsAsync(
        Guid userId, CancellationToken ct = default)
        => await db.UserChannelPreferences
                   .Where(p => p.UserId == userId)
                   .ToListAsync(ct);

    public async Task<IReadOnlyList<UserGroupPreference>> GetAllGroupPrefsAsync(
        Guid userId, CancellationToken ct = default)
        => await db.UserGroupPreferences
                   .Where(p => p.UserId == userId)
                   .ToListAsync(ct);

    public async Task UpsertChannelAsync(UserChannelPreference preference, CancellationToken ct = default)
    {
        var existing = await db.UserChannelPreferences.FirstOrDefaultAsync(
            p => p.UserId == preference.UserId && p.Channel == preference.Channel, ct);

        if (existing is null)
            db.UserChannelPreferences.Add(preference);
        else
            existing.SetEnabled(preference.Enabled);

        await db.SaveChangesAsync(ct);
    }

    public async Task UpsertGroupAsync(UserGroupPreference preference, CancellationToken ct = default)
    {
        var existing = await db.UserGroupPreferences.FirstOrDefaultAsync(
            p => p.UserId    == preference.UserId
              && p.Channel    == preference.Channel
              && p.EventGroup == preference.EventGroup, ct);

        if (existing is null)
            db.UserGroupPreferences.Add(preference);
        else
            existing.SetEnabled(preference.Enabled);

        await db.SaveChangesAsync(ct);
    }
}
