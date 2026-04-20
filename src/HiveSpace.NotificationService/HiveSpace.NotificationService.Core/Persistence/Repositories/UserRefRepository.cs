using Microsoft.EntityFrameworkCore;
using HiveSpace.NotificationService.Core.DomainModels.External;
using HiveSpace.NotificationService.Core.Interfaces;

namespace HiveSpace.NotificationService.Core.Persistence.Repositories;

public class UserRefRepository(NotificationDbContext db) : IUserRefRepository
{
    public Task<UserRef?> GetByIdAsync(Guid userId, CancellationToken ct = default)
        => db.UserRefs.FirstOrDefaultAsync(u => u.Id == userId, ct);

    public Task<UserRef?> GetByStoreIdAsync(Guid storeId, CancellationToken ct = default)
        => db.UserRefs.FirstOrDefaultAsync(u => u.StoreId == storeId, ct);

    public async Task UpsertAsync(UserRef userRef, CancellationToken ct = default)
    {
        var existing = await db.UserRefs.FirstOrDefaultAsync(u => u.Id == userRef.Id, ct);
        if (existing is null)
            db.UserRefs.Add(userRef);
        else
            db.Entry(existing).CurrentValues.SetValues(userRef);

        await db.SaveChangesAsync(ct);
    }
}
