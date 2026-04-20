using HiveSpace.NotificationService.Core.DomainModels.External;

namespace HiveSpace.NotificationService.Core.Interfaces;

public interface IUserRefRepository
{
    Task<UserRef?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task<UserRef?> GetByStoreIdAsync(Guid storeId, CancellationToken ct = default);
    Task UpsertAsync(UserRef userRef, CancellationToken ct = default);
}
