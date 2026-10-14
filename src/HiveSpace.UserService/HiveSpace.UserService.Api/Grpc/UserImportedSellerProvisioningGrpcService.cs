using Grpc.Core;
using HiveSpace.Infrastructure.Authorization.Grpc;
using HiveSpace.Infrastructure.Messaging.Shared.Grpc.SellerProvisioning;
using HiveSpace.UserService.Application.Stores.Commands.ProvisionImportedSellerStore;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace HiveSpace.UserService.Api.Grpc;

[Authorize(Policy = ServiceClientAuthorization.CatalogServiceSellerProvisioningPolicy)]
public class UserImportedSellerProvisioningGrpcService(
    IMediator mediator,
    ILogger<UserImportedSellerProvisioningGrpcService> logger)
    : UserImportedSellerProvisioning.UserImportedSellerProvisioningBase
{
    public override async Task<ProvisionImportedSellerStoreResponse> ProvisionImportedSellerStore(
        ProvisionImportedSellerStoreRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.IdentityUserId, out var identityUserId)
            || string.IsNullOrWhiteSpace(request.ExternalSource)
            || string.IsNullOrWhiteSpace(request.ExternalSellerId)
            || string.IsNullOrWhiteSpace(request.StoreName))
        {
            return Failed("InvalidProvisioningRequest", "Imported seller store provisioning request was invalid.");
        }

        var result = await mediator.Send(
            new ProvisionImportedSellerStoreCommand(
                request.ExternalSource,
                request.ExternalSellerId,
                identityUserId,
                request.StoreName,
                null),
            context.CancellationToken);

        logger.LogInformation(
            "Imported seller store provisioning completed. CorrelationId: {CorrelationId}, BundleId: {BundleId}, ImportedSellerId: {ImportedSellerId}, ExternalSellerId: {ExternalSellerId}, Outcome: {Outcome}, FailureCode: {FailureCode}",
            request.CorrelationId,
            request.BundleId,
            request.ImportedSellerId,
            request.ExternalSellerId,
            result.Status,
            result.ConflictReason);

        return new ProvisionImportedSellerStoreResponse
        {
            Outcome = Map(result.Status),
            StoreId = result.StoreId?.ToString() ?? string.Empty,
            FailureCode = result.ConflictReason ?? string.Empty,
            FailureMessage = result.ConflictReason ?? string.Empty
        };
    }

    private static ProvisionImportedSellerStoreResponse Failed(string failureCode, string failureMessage)
        => new()
        {
            Outcome = ProvisioningOutcome.Failed,
            FailureCode = failureCode,
            FailureMessage = failureMessage
        };

    private static ProvisioningOutcome Map(string status)
        => status switch
        {
            "Created" => ProvisioningOutcome.Created,
            "Matched" => ProvisioningOutcome.Matched,
            "Conflict" => ProvisioningOutcome.Conflict,
            _ => ProvisioningOutcome.Failed
        };
}
