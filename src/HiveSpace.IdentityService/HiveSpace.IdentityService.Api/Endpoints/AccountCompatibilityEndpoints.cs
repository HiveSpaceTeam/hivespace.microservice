using Microsoft.AspNetCore.Mvc;

namespace HiveSpace.IdentityService.Api.Endpoints;

internal static class AccountCompatibilityEndpoints
{
    public static IEndpointRouteBuilder MapAccountCompatibilityEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/Account/Login", RedirectToLogin).AllowAnonymous();
        app.MapGet("/Account/Register", RedirectToRegister).AllowAnonymous();
        app.MapGet("/Account/Logout", RedirectToLogout).AllowAnonymous();

        return app;
    }

    private static IResult RedirectToLogin(
        [FromQuery] string? returnUrl,
        [FromQuery] string? app,
        [FromQuery] string? clientId,
        IConfiguration configuration)
        => Results.Redirect(BuildRedirectUrl(configuration, ResolveApp(returnUrl, app, clientId), "login", returnUrl));

    private static IResult RedirectToRegister(
        [FromQuery] string? returnUrl,
        [FromQuery] string? app,
        [FromQuery] string? clientId,
        IConfiguration configuration)
        => Results.Redirect(BuildRedirectUrl(configuration, ResolveApp(returnUrl, app, clientId), "register", returnUrl));

    private static IResult RedirectToLogout(
        [FromQuery] string? returnUrl,
        [FromQuery] string? app,
        [FromQuery] string? clientId,
        IConfiguration configuration)
        => Results.Redirect(BuildRedirectUrl(configuration, ResolveApp(returnUrl, app, clientId), "logout", returnUrl));

    private static string ResolveApp(string? returnUrl, string? app, string? clientId)
    {
        var hint = $"{app} {clientId} {returnUrl}".ToLowerInvariant();
        if (hint.Contains("adminportal") || hint.Contains("admin"))
            return "admin";
        if (hint.Contains("sellercenter") || hint.Contains("seller"))
            return "seller";

        return "buyer";
    }

    private static string BuildRedirectUrl(IConfiguration configuration, string app, string route, string? returnUrl)
    {
        var origin = configuration[$"FrontendRedirects:{app}:Origin"]
            ?? configuration["DefaultRedirectUrl"]
            ?? "http://localhost:5175";
        var path = configuration[$"FrontendRedirects:{app}:{route}Path"]
            ?? (route == "register" ? "/register" : route == "logout" ? "/logout" : "/login");

        var builder = new UriBuilder(new Uri(new Uri(origin.TrimEnd('/')), path.TrimStart('/')));
        if (!string.IsNullOrWhiteSpace(returnUrl) && IsSafeReturnUrl(returnUrl))
            builder.Query = $"returnUrl={Uri.EscapeDataString(returnUrl)}";

        return builder.Uri.ToString();
    }

    private static bool IsSafeReturnUrl(string returnUrl)
    {
        if (Uri.TryCreate(returnUrl, UriKind.Relative, out _))
            return returnUrl.StartsWith('/');

        return Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri)
            && uri.Scheme is "http" or "https"
            && uri.IsLoopback;
    }
}
