using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.UserService.Application.Stores.Commands.ProvisionImportedSellerStore;

public record ProvisionImportedSellerStoreCommand(
    string SourceSystem,
    string ExternalSellerId,
    Guid UserId,
    string StoreName,
    string? SourceUrl,
    string? LogoUrl = null) : ICommand<ProvisionImportedSellerStoreResult>;

public record ProvisionImportedSellerStoreResult(
    Guid? StoreId,
    string Status,
    string? ConflictReason);
