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
            return returnUrl.StartsWith('/');

        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri))
            return false;

        return uri.Scheme is "http" or "https"
            && (uri.IsLoopback || uri.Host.EndsWith(".hivespace.local", StringComparison.OrdinalIgnoreCase));
    }
}
