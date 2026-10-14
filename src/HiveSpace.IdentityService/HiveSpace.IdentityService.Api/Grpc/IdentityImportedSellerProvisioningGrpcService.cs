using Grpc.Core;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.ProvisionImportedSellerAccount;
using HiveSpace.Infrastructure.Authorization.Grpc;
using HiveSpace.Infrastructure.Messaging.Shared.Grpc.SellerProvisioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace HiveSpace.IdentityService.Api.Grpc;

[Authorize(Policy = ServiceClientAuthorization.CatalogServiceSellerProvisioningPolicy)]
public class IdentityImportedSellerProvisioningGrpcService(
    IMediator mediator,
    ILogger<IdentityImportedSellerProvisioningGrpcService> logger)
    : IdentityImportedSellerProvisioning.IdentityImportedSellerProvisioningBase
{
    public override async Task<ProvisionImportedSellerAccountResponse> ProvisionImportedSellerAccount(
        ProvisionImportedSellerAccountRequest request,
        ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.ExternalSource)
            || string.IsNullOrWhiteSpace(request.ExternalSellerId)
            || string.IsNullOrWhiteSpace(request.SellerName))
        {
            return Failed("InvalidProvisioningRequest", "Imported seller account provisioning request was invalid.");
        }

        var result = await mediator.Send(
            new ProvisionImportedSellerAccountCommand(
                request.ExternalSource,
                request.ExternalSellerId,
                request.SellerName,
                null),
            context.CancellationToken);

        logger.LogInformation(
            "Imported seller account provisioning completed. CorrelationId: {CorrelationId}, BundleId: {BundleId}, ImportedSellerId: {ImportedSellerId}, ExternalSellerId: {ExternalSellerId}, Outcome: {Outcome}, FailureCode: {FailureCode}",
            request.CorrelationId,
            request.BundleId,
            request.ImportedSellerId,
            request.ExternalSellerId,
            result.Status,
            result.ConflictReason);

        return new ProvisionImportedSellerAccountResponse
        {
            Outcome = Map(result.Status),
            IdentityUserId = result.UserId?.ToString() ?? string.Empty,
            FailureCode = result.ConflictReason ?? string.Empty,
            FailureMessage = result.ConflictReason ?? string.Empty
        };
    }

    private static ProvisionImportedSellerAccountResponse Failed(string failureCode, string failureMessage)
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
