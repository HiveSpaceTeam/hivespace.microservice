namespace HiveSpace.IdentityService.Api.Configs;

public class GoogleExternalAuthOptions
{
    public const string SectionName = "Authentication:Google";

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string CallbackPath { get; set; } = "/api/v1/accounts/external/google/callback";

    public string? PublicCallbackOrigin { get; set; }

    public int PendingLinkLifetimeMinutes { get; set; } = 10;

    public string ProviderDisplayName { get; set; } = "Google";

    public Dictionary<string, string> AllowedFrontendOrigins { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public bool IsConfigured()
        => !string.IsNullOrWhiteSpace(ClientId)
            && !string.IsNullOrWhiteSpace(ClientSecret)
            && !string.IsNullOrWhiteSpace(CallbackPath)
            && CallbackPath.StartsWith('/')
            && (string.IsNullOrWhiteSpace(PublicCallbackOrigin)
                || Uri.TryCreate(PublicCallbackOrigin, UriKind.Absolute, out _))
            && AllowedFrontendOrigins.ContainsKey("buyer")
            && AllowedFrontendOrigins.ContainsKey("seller")
            && PendingLinkLifetimeMinutes > 0;
}
