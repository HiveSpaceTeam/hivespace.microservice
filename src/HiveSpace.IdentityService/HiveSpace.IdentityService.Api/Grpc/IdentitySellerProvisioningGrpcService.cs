using Grpc.Core;
using HiveSpace.IdentityService.Core.Features.Roles.Commands.ProvisionSellerIdentity;
using HiveSpace.Infrastructure.Authorization.Grpc;
using HiveSpace.Infrastructure.Messaging.Shared.Grpc.SellerProvisioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace HiveSpace.IdentityService.Api.Grpc;

[Authorize(Policy = ServiceClientAuthorization.UserServiceSellerProvisioningPolicy)]
public class IdentitySellerProvisioningGrpcService(
    IMediator mediator,
    ILogger<IdentitySellerProvisioningGrpcService> logger)
    : IdentitySellerProvisioning.IdentitySellerProvisioningBase
{
    public override async Task<ProvisionSellerIdentityResponse> ProvisionSellerIdentity(
        ProvisionSellerIdentityRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId)
            || !Guid.TryParse(request.StoreId, out var storeId))
        {
            return Failed(request, "InvalidProvisioningRequest", "Seller provisioning request was invalid.");
        }

        var result = await mediator.Send(
            new ProvisionSellerIdentityCommand(
                userId,
                storeId,
                request.StoreName,
                request.CorrelationId),
            context.CancellationToken);

        logger.LogInformation(
            "Seller identity provisioning completed. CorrelationId: {CorrelationId}, UserId: {UserId}, StoreId: {StoreId}, Outcome: {Outcome}, FailureCode: {FailureCode}",
            request.CorrelationId,
            userId,
            storeId,
            result.Outcome,
            result.FailureCode);

        return new ProvisionSellerIdentityResponse
        {
            Outcome = Map(result.Outcome),
            IdentityUserId = result.IdentityUserId?.ToString() ?? string.Empty,
            StoreId = result.StoreId.ToString(),
            FailureCode = result.FailureCode ?? string.Empty,
            FailureMessage = result.FailureMessage ?? string.Empty
        };
    }

    private static ProvisionSellerIdentityResponse Failed(
        ProvisionSellerIdentityRequest request,
        string failureCode,
        string failureMessage)
        => new()
        {
            Outcome = ProvisioningOutcome.Failed,
            StoreId = request.StoreId,
            FailureCode = failureCode,
            FailureMessage = failureMessage
        };

    private static ProvisioningOutcome Map(ProvisionSellerIdentityOutcome outcome)
        => outcome switch
        {
            ProvisionSellerIdentityOutcome.Created => ProvisioningOutcome.Created,
            ProvisionSellerIdentityOutcome.Matched => ProvisioningOutcome.Matched,
            ProvisionSellerIdentityOutcome.Conflict => ProvisioningOutcome.Conflict,
            _ => ProvisioningOutcome.Failed
        };
}
