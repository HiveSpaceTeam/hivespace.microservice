namespace HiveSpace.UserService.Application.Stores.IdentityProvisioning;

public interface IIdentitySellerProvisioningClient
{
    Task<IdentitySellerProvisioningResult> ProvisionSellerIdentityAsync(
        Guid userId,
        Guid storeId,
        string storeName,
        string correlationId,
        CancellationToken cancellationToken = default);
}
