using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.ProvisionImportedSellerAccount;

public record ProvisionImportedSellerAccountCommand(
    string SourceSystem,
    string ExternalSellerId,
    string DisplayName,
    string? SourceUrl) : ICommand<ProvisionImportedSellerAccountResult>;

public record ProvisionImportedSellerAccountResult(
    Guid? UserId,
    string Status,
    string? ConflictReason);
