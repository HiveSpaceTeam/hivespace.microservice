namespace HiveSpace.IdentityService.Core.Features.Roles.Commands.ProvisionSellerIdentity;

public record ProvisionSellerIdentityResult(
    ProvisionSellerIdentityOutcome Outcome,
    Guid? IdentityUserId,
    Guid StoreId,
    string? FailureCode,
    string? FailureMessage);
