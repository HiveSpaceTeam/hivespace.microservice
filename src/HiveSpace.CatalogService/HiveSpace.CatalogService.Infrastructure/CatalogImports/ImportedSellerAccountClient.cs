using HiveSpace.CatalogService.Application.CatalogImports.Ports;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace HiveSpace.CatalogService.Infrastructure.CatalogImports;

public class ImportedSellerAccountClient(HttpClient httpClient, ICatalogImportServiceTokenProvider tokenProvider) : IImportedSellerAccountClient
{
    public async Task<ImportedSellerAccountProvisioningResult> ProvisionAsync(
        ImportedSellerAccountProvisioningRequest request,
        CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admins/imported-seller-accounts")
        {
            Content = JsonContent.Create(request)
        };
        await AddServiceTokenAsync(message, cancellationToken);

        var response = await httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new ImportedSellerAccountProvisioningResult(
                null,
                ImportedSellerProvisioningOutcome.Failed,
                $"IdentityServiceStatus{(int)response.StatusCode}");
        }

        var payload = await response.Content.ReadFromJsonAsync<ImportedSellerAccountResponse>(cancellationToken);
        if (payload is null)
            return new ImportedSellerAccountProvisioningResult(null, ImportedSellerProvisioningOutcome.Failed, "EmptyIdentityServiceResponse");

        return new ImportedSellerAccountProvisioningResult(
            payload.UserId,
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

    private sealed record ImportedSellerAccountResponse(Guid? UserId, string Status, string? ConflictReason);
}
