using Microsoft.EntityFrameworkCore;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Core.Persistence.Repositories;

public class UserPreferenceRepository(NotificationDbContext db) : IUserPreferenceRepository
{
    private static readonly IReadOnlyList<NotificationChannel> DefaultChannels =
        [NotificationChannel.InApp, NotificationChannel.Email];

    public async Task<IReadOnlyList<NotificationChannel>> GetEnabledChannelsAsync(
        Guid userId, string eventType, CancellationToken ct = default)
    {
        var rows = await db.UserPreferences
                           .Where(p => p.UserId == userId && p.EventType == eventType)
                           .Select(p => new { p.Channel, p.Enabled })
                           .ToListAsync(ct);

        if (rows.Count == 0)
            return DefaultChannels;

        return DefaultChannels
            .Where(ch =>
            {
                var row = rows.FirstOrDefault(r => r.Channel == ch);
                return row is null || row.Enabled;
            })
            .ToList();
    }

    public async Task<IReadOnlyList<UserPreference>> GetAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await db.UserPreferences
                       .Where(p => p.UserId == userId)
                       .ToListAsync(ct);
    }

    public async Task UpsertAsync(UserPreference preference, CancellationToken ct = default)
    {
        var existing = await db.UserPreferences.FirstOrDefaultAsync(
            p => p.UserId == preference.UserId
              && p.Channel == preference.Channel
              && p.EventType == preference.EventType,
            ct);

        if (existing is null)
            db.UserPreferences.Add(preference);
        else
            db.Entry(existing).CurrentValues.SetValues(preference);

        await db.SaveChangesAsync(ct);
    }
}
