using Grpc.Core;
using Grpc.Net.Client;
using HiveSpace.CatalogService.Application.CatalogImports.Ports;
using Grpc.Core.Interceptors;
using HiveSpace.Infrastructure.Messaging.Shared.Grpc;
using HiveSpace.Infrastructure.Messaging.Shared.Grpc.SellerProvisioning;
using Microsoft.Extensions.Configuration;

namespace HiveSpace.CatalogService.Infrastructure.CatalogImports;

public class ImportedSellerAccountGrpcClient(
    IConfiguration configuration,
    ICatalogImportServiceTokenProvider tokenProvider)
    : IImportedSellerAccountClient
{
    public async Task<ImportedSellerAccountProvisioningResult> ProvisionAsync(
        ImportedSellerAccountProvisioningRequest request,
        CancellationToken cancellationToken = default)
    {
        var address = configuration["InternalGrpc:IdentityService:Address"]
            ?? configuration["ServiceUrls:IdentityService:Grpc"]
            ?? "http://localhost:50011";

        using var channel = GrpcChannel.ForAddress(address);
        var client = new IdentityImportedSellerProvisioning.IdentityImportedSellerProvisioningClient(
            channel.Intercept(new CorrelationGrpcInterceptor(request.CorrelationId)));
        var headers = await BuildHeadersAsync(tokenProvider, cancellationToken);

        try
        {
            var response = await client.ProvisionImportedSellerAccountAsync(
                new ProvisionImportedSellerAccountRequest
                {
                    CorrelationId = request.CorrelationId ?? Guid.NewGuid().ToString("N"),
                    BundleId = request.BundleId?.ToString() ?? string.Empty,
                    ImportedSellerId = request.ImportedSellerId?.ToString() ?? string.Empty,
                    ExternalSource = request.SourceSystem,
                    ExternalSellerId = request.ExternalSellerId,
                    SellerName = request.DisplayName,
                    Email = string.Empty
                },
                headers,
                cancellationToken: cancellationToken);

            return new ImportedSellerAccountProvisioningResult(
                Guid.TryParse(response.IdentityUserId, out var userId) ? userId : null,
                Map(response.Outcome),
                NullIfEmpty(response.FailureCode) ?? NullIfEmpty(response.FailureMessage));
        }
        catch (RpcException ex) when (ex.StatusCode is StatusCode.Unauthenticated or StatusCode.PermissionDenied)
        {
            return new ImportedSellerAccountProvisioningResult(
                null,
                ImportedSellerProvisioningOutcome.Failed,
                "IdentityProvisioningUnauthorized");
        }
        catch (RpcException ex)
        {
            return new ImportedSellerAccountProvisioningResult(
                null,
                ImportedSellerProvisioningOutcome.Failed,
                $"IdentityServiceGrpc{ex.StatusCode}");
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
