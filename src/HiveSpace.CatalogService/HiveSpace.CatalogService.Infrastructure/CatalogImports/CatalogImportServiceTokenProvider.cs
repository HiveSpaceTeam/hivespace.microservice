using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace HiveSpace.CatalogService.Infrastructure.CatalogImports;

public class CatalogImportServiceTokenProvider(HttpClient httpClient, IConfiguration configuration) : ICatalogImportServiceTokenProvider
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private string? _accessToken;
    private DateTimeOffset _expiresAt;

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (HasUsableToken())
            return _accessToken!;

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (HasUsableToken())
                return _accessToken!;

            var clientId = Required("ServiceClients:CatalogImportProvisioning:ClientId");
            var clientSecret = Required("ServiceClients:CatalogImportProvisioning:ClientSecret");
            var scope = string.Join(' ', configuration
                .GetSection("ServiceClients:CatalogImportProvisioning:Scopes")
                .Get<string[]>() ?? ["identity.fullaccess", "user.fullaccess"]);

            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["scope"] = scope
            });

            using var response = await httpClient.PostAsync("/connect/token", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
            if (payload is null || string.IsNullOrWhiteSpace(payload.AccessToken))
                throw new InvalidOperationException("IdentityService returned an empty catalog import service token.");

            _accessToken = payload.AccessToken;
            _expiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(60, payload.ExpiresIn - 60));
            return _accessToken;
        }
        finally
        {
            _gate.Release();
        }
    }

    private bool HasUsableToken()
        => !string.IsNullOrWhiteSpace(_accessToken) && _expiresAt > DateTimeOffset.UtcNow;

    private string Required(string key)
        => configuration[key] is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"Missing configuration value '{key}'.");

    private sealed record TokenResponse(
        [property: System.Text.Json.Serialization.JsonPropertyName("access_token")] string AccessToken,
        [property: System.Text.Json.Serialization.JsonPropertyName("expires_in")] int ExpiresIn);
}
