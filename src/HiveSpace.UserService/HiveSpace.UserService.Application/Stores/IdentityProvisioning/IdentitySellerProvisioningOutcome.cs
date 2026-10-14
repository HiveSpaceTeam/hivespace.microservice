namespace HiveSpace.UserService.Application.Stores.IdentityProvisioning;

public enum IdentitySellerProvisioningOutcome
{
    Created,
    Matched,
    Conflict,
    Failed,
    Timeout
}
