using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace HiveSpace.UserService.Infrastructure.IdentityProvisioning;

public class IdentityServiceClientTokenProvider(
    HttpClient httpClient,
    IConfiguration configuration)
    : IIdentityServiceClientTokenProvider
{
    private static readonly TimeSpan RefreshSkew = TimeSpan.FromMinutes(1);
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private string? _accessToken;
    private DateTimeOffset _expiresAt;

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (HasUsableToken())
            return _accessToken!;

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (HasUsableToken())
                return _accessToken!;

            var tokenEndpoint = GetRequired("InternalGrpc:IdentityService:TokenEndpoint");
            using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = GetRequired("InternalGrpc:IdentityService:ClientId"),
                    ["client_secret"] = GetRequired("InternalGrpc:IdentityService:ClientSecret"),
                    ["scope"] = GetRequired("InternalGrpc:IdentityService:Scope")
                })
            };

            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadFromJsonAsync<ClientCredentialsTokenResponse>(
                cancellationToken: cancellationToken);
            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
                throw new InvalidOperationException("IdentityService token response did not include an access token.");

            _accessToken = token.AccessToken;
            _expiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(token.ExpiresIn, 60));
            return _accessToken;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private bool HasUsableToken()
        => !string.IsNullOrWhiteSpace(_accessToken)
            && _expiresAt - RefreshSkew > DateTimeOffset.UtcNow;

    private string GetRequired(string key)
        => configuration[key] ?? throw new InvalidOperationException($"{key} is required.");

    private sealed record ClientCredentialsTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn,
        [property: JsonPropertyName("token_type")] string TokenType);
}
