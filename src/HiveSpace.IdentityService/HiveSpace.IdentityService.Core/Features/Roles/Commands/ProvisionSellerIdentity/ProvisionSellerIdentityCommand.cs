using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.IdentityService.Core.Features.Roles.Commands.ProvisionSellerIdentity;

public record ProvisionSellerIdentityCommand(
    Guid UserId,
    Guid StoreId,
    string StoreName,
    string CorrelationId) : ICommand<ProvisionSellerIdentityResult>;
