namespace HiveSpace.UserService.Application.Stores.IdentityProvisioning;

public record IdentitySellerProvisioningResult(
    IdentitySellerProvisioningOutcome Outcome,
    Guid? IdentityUserId,
    Guid? StoreId,
    string? FailureCode,
    string? FailureMessage)
{
    public bool Succeeded =>
        Outcome is IdentitySellerProvisioningOutcome.Created or IdentitySellerProvisioningOutcome.Matched;
}
