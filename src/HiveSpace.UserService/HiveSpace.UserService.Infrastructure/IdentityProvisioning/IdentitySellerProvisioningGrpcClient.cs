using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Core.Interceptors;
using HiveSpace.Infrastructure.Messaging.Shared.Grpc;
using HiveSpace.Infrastructure.Messaging.Shared.Grpc.SellerProvisioning;
using HiveSpace.UserService.Application.Stores.IdentityProvisioning;
using Microsoft.Extensions.Configuration;

namespace HiveSpace.UserService.Infrastructure.IdentityProvisioning;

public class IdentitySellerProvisioningGrpcClient(
    IConfiguration configuration,
    IIdentityServiceClientTokenProvider tokenProvider)
    : IIdentitySellerProvisioningClient
{
    private const int DefaultDeadlineSeconds = 10;

    public async Task<IdentitySellerProvisioningResult> ProvisionSellerIdentityAsync(
        Guid userId,
        Guid storeId,
        string storeName,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        var address = configuration["InternalGrpc:IdentityService:Address"]
            ?? configuration["Services:IdentityService:Grpc"]
            ?? "http://localhost:5001";

        var deadline = TimeSpan.FromSeconds(
            int.TryParse(configuration["InternalGrpc:IdentityService:DeadlineSeconds"], out var seconds) && seconds > 0
                ? seconds
                : DefaultDeadlineSeconds);

        using var channel = GrpcChannel.ForAddress(address);
        var client = new IdentitySellerProvisioning.IdentitySellerProvisioningClient(
            channel.Intercept(new CorrelationGrpcInterceptor(correlationId)));
        var headers = await BuildHeadersAsync(tokenProvider, cancellationToken);

        try
        {
            var response = await client.ProvisionSellerIdentityAsync(
                new ProvisionSellerIdentityRequest
                {
                    CorrelationId = correlationId,
                    UserId = userId.ToString(),
                    StoreId = storeId.ToString(),
                    StoreName = storeName
                },
                headers,
                deadline: DateTime.UtcNow.Add(deadline),
                cancellationToken: cancellationToken);

            return Map(response);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
        {
            return new IdentitySellerProvisioningResult(
                IdentitySellerProvisioningOutcome.Timeout,
                null,
                storeId,
                "IdentityProvisioningTimeout",
                "Seller identity provisioning timed out.");
        }
        catch (RpcException ex) when (ex.StatusCode is StatusCode.Unauthenticated or StatusCode.PermissionDenied)
        {
            return new IdentitySellerProvisioningResult(
                IdentitySellerProvisioningOutcome.Failed,
                null,
                storeId,
                "IdentityProvisioningUnauthorized",
                "Seller identity provisioning was not authorized.");
        }
        catch (RpcException ex)
        {
            return new IdentitySellerProvisioningResult(
                IdentitySellerProvisioningOutcome.Failed,
                null,
                storeId,
                ex.StatusCode.ToString(),
                "Seller identity provisioning failed.");
        }
    }

    private static async Task<Metadata> BuildHeadersAsync(
        IIdentityServiceClientTokenProvider tokenProvider,
        CancellationToken cancellationToken)
    {
        var headers = new Metadata();
        var accessToken = await tokenProvider.GetAccessTokenAsync(cancellationToken);
        headers.Add("authorization", $"Bearer {accessToken}");
        return headers;
    }

    private static IdentitySellerProvisioningResult Map(ProvisionSellerIdentityResponse response)
    {
        var identityUserId = Guid.TryParse(response.IdentityUserId, out var parsedIdentityUserId)
            ? parsedIdentityUserId
            : (Guid?)null;
        var storeId = Guid.TryParse(response.StoreId, out var parsedStoreId)
            ? parsedStoreId
            : (Guid?)null;

        var outcome = response.Outcome switch
        {
            ProvisioningOutcome.Created => IdentitySellerProvisioningOutcome.Created,
            ProvisioningOutcome.Matched => IdentitySellerProvisioningOutcome.Matched,
            ProvisioningOutcome.Conflict => IdentitySellerProvisioningOutcome.Conflict,
            _ => IdentitySellerProvisioningOutcome.Failed
        };

        return new IdentitySellerProvisioningResult(
            outcome,
            identityUserId,
            storeId,
            string.IsNullOrWhiteSpace(response.FailureCode) ? null : response.FailureCode,
            string.IsNullOrWhiteSpace(response.FailureMessage) ? null : response.FailureMessage);
    }
}
