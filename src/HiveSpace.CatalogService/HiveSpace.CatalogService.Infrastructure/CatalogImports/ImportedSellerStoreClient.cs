using HiveSpace.CatalogService.Application.CatalogImports.Ports;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace HiveSpace.CatalogService.Infrastructure.CatalogImports;

public class ImportedSellerStoreClient(HttpClient httpClient, ICatalogImportServiceTokenProvider tokenProvider) : IImportedSellerStoreClient
{
    public async Task<ImportedSellerStoreProvisioningResult> ProvisionAsync(
        ImportedSellerStoreProvisioningRequest request,
        CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admins/imported-seller-stores")
        {
            Content = JsonContent.Create(request)
        };
        await AddServiceTokenAsync(message, cancellationToken);

        var response = await httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new ImportedSellerStoreProvisioningResult(
                null,
                ImportedSellerProvisioningOutcome.Failed,
                $"UserServiceStatus{(int)response.StatusCode}");
        }

        var payload = await response.Content.ReadFromJsonAsync<ImportedSellerStoreResponse>(cancellationToken);
        if (payload is null)
            return new ImportedSellerStoreProvisioningResult(null, ImportedSellerProvisioningOutcome.Failed, "EmptyUserServiceResponse");

        return new ImportedSellerStoreProvisioningResult(
            payload.StoreId,
            MapOutcome(payload.Status),
            payload.ConflictReason);
    }

    private static ImportedSellerProvisioningOutcome MapOutcome(string status)
        => status switch
        {
            "Created" => ImportedSellerProvisioningOutcome.Created,
            "Matched" => ImportedSellerProvisioningOutcome.Matched,
            "Conflict" => ImportedSellerProvisioningOutcome.Conflict,
            _ => ImportedSellerProvisioningOutcome.Failed
        };

    private async Task AddServiceTokenAsync(HttpRequestMessage message, CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetAccessTokenAsync(cancellationToken);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private sealed record ImportedSellerStoreResponse(Guid? StoreId, string Status, string? ConflictReason);
}
