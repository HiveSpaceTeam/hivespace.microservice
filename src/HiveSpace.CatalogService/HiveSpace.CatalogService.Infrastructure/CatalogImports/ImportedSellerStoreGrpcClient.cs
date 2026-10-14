using Grpc.Core;
using Grpc.Net.Client;
using HiveSpace.CatalogService.Application.CatalogImports.Ports;
using Grpc.Core.Interceptors;
using HiveSpace.Infrastructure.Messaging.Shared.Grpc;
using HiveSpace.Infrastructure.Messaging.Shared.Grpc.SellerProvisioning;
using Microsoft.Extensions.Configuration;

namespace HiveSpace.CatalogService.Infrastructure.CatalogImports;

public class ImportedSellerStoreGrpcClient(
    IConfiguration configuration,
    ICatalogImportServiceTokenProvider tokenProvider)
    : IImportedSellerStoreClient
{
    public async Task<ImportedSellerStoreProvisioningResult> ProvisionAsync(
        ImportedSellerStoreProvisioningRequest request,
        CancellationToken cancellationToken = default)
    {
        var address = configuration["InternalGrpc:UserService:Address"]
            ?? configuration["ServiceUrls:UserService:Grpc"]
            ?? "http://localhost:50017";

        using var channel = GrpcChannel.ForAddress(address);
        var client = new UserImportedSellerProvisioning.UserImportedSellerProvisioningClient(
            channel.Intercept(new CorrelationGrpcInterceptor(request.CorrelationId)));
        var headers = await BuildHeadersAsync(tokenProvider, cancellationToken);

        try
        {
            var response = await client.ProvisionImportedSellerStoreAsync(
                new ProvisionImportedSellerStoreRequest
                {
                    CorrelationId = request.CorrelationId ?? Guid.NewGuid().ToString("N"),
                    BundleId = request.BundleId?.ToString() ?? string.Empty,
                    ImportedSellerId = request.ImportedSellerId?.ToString() ?? string.Empty,
                    IdentityUserId = request.UserId.ToString(),
                    ExternalSource = request.SourceSystem,
                    ExternalSellerId = request.ExternalSellerId,
                    StoreName = request.StoreName
                },
                headers,
                cancellationToken: cancellationToken);

            return new ImportedSellerStoreProvisioningResult(
                Guid.TryParse(response.StoreId, out var storeId) ? storeId : null,
                Map(response.Outcome),
                NullIfEmpty(response.FailureCode) ?? NullIfEmpty(response.FailureMessage));
        }
        catch (RpcException ex) when (ex.StatusCode is StatusCode.Unauthenticated or StatusCode.PermissionDenied)
        {
            return new ImportedSellerStoreProvisioningResult(
                null,
                ImportedSellerProvisioningOutcome.Failed,
                "UserProvisioningUnauthorized");
        }
        catch (RpcException ex)
        {
            return new ImportedSellerStoreProvisioningResult(
                null,
                ImportedSellerProvisioningOutcome.Failed,
                $"UserServiceGrpc{ex.StatusCode}");
        }
    }

    private static async Task<Metadata> BuildHeadersAsync(
        ICatalogImportServiceTokenProvider tokenProvider,
        CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetAccessTokenAsync(cancellationToken);
        return new Metadata { { "authorization", $"Bearer {token}" } };
    }

    private static ImportedSellerProvisioningOutcome Map(ProvisioningOutcome outcome)
        => outcome switch
        {
            ProvisioningOutcome.Created => ImportedSellerProvisioningOutcome.Created,
            ProvisioningOutcome.Matched => ImportedSellerProvisioningOutcome.Matched,
            ProvisioningOutcome.Conflict => ImportedSellerProvisioningOutcome.Conflict,
            _ => ImportedSellerProvisioningOutcome.Failed
        };

    private static string? NullIfEmpty(string value)
        => string.IsNullOrWhiteSpace(value) ? null : value;
}
