namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands;

internal static class AccountSessionValidation
{
    private static readonly HashSet<string> KnownApps = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin",
        "seller",
        "buyer"
    };

    public static bool IsKnownApp(string? app)
        => !string.IsNullOrWhiteSpace(app) && KnownApps.Contains(app);

    public static bool IsSafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return true;

        if (Uri.TryCreate(returnUrl, UriKind.Relative, out _))
            return IsLocalPath(returnUrl);

        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri))
            return false;

        return uri.Scheme is "http" or "https"
            && (uri.IsLoopback || uri.Host.EndsWith(".hivespace.local", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsSafeGoogleReturnUrl(string app, string? returnUrl, string? allowedOrigin)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return true;

        if (!IsBuyerOrSellerApp(app))
            return false;

        if (Uri.TryCreate(returnUrl, UriKind.Relative, out _))
            return IsLocalPath(returnUrl) && !IsLegacyAccountPath(returnUrl);

        return Uri.TryCreate(returnUrl, UriKind.Absolute, out var returnUri)
            && Uri.TryCreate(allowedOrigin, UriKind.Absolute, out var allowedOriginUri)
            && SameOrigin(returnUri, allowedOriginUri);
    }

    public static bool IsBuyerOrSellerApp(string? app)
        => IsKnownApp(app)
            && AccountSessionHandlerBase.NormalizeApp(app!) != "admin";

    private static bool SameOrigin(Uri left, Uri right)
        => string.Equals(left.Scheme, right.Scheme, StringComparison.OrdinalIgnoreCase)
            && string.Equals(left.Host, right.Host, StringComparison.OrdinalIgnoreCase)
            && left.Port == right.Port;

    private static bool IsLegacyAccountPath(string returnUrl)
        => string.Equals(returnUrl, "/Account", StringComparison.OrdinalIgnoreCase)
            || returnUrl.StartsWith("/Account/", StringComparison.OrdinalIgnoreCase);

    private static bool IsLocalPath(string returnUrl)
        => returnUrl.StartsWith('/')
            && (returnUrl.Length == 1 || returnUrl[1] is not '/' and not '\\');
}
