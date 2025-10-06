using Duende.IdentityServer.Services;
using HiveSpace.UserService.Api.Pages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HiveSpace.UserService.Api.Pages.ExternalLogin;
[AllowAnonymous]
[SecurityHeaders]
public class Challenge : PageModel
{
    private readonly IIdentityServerInteractionService _interactionService;
    private readonly IConfiguration _configuration;

    public Challenge(IIdentityServerInteractionService interactionService, IConfiguration configuration)
    {
        _interactionService = interactionService;
        _configuration = configuration;
    }

    // Added optional clientId parameter. If returnUrl is empty, we try to resolve it from the configured client's RedirectUris
    public IActionResult OnGet(string scheme, string? returnUrl, string? clientId)
    {
        if (string.IsNullOrEmpty(returnUrl))
        {
            // If a clientId was provided, try to read the first RedirectUris entry from configuration
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                try
                {
                    var redirectUris = _configuration.GetSection($"Clients:{clientId}:RedirectUris").Get<string[]>();
                    if (redirectUris != null && redirectUris.Length > 0)
                    {
                        returnUrl = redirectUris[0];
                    }
                }
                catch
                {
                    // ignore and fall back to default below
                }
            }

            // fallback to DefaultRedirectUrl or root
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = _configuration["DefaultRedirectUrl"] ?? "~/";
            }
        }

        // Abort on incorrect returnUrl - it is neither a local url nor a valid OIDC url.
        if (Url.IsLocalUrl(returnUrl) == false && _interactionService.IsValidReturnUrl(returnUrl) == false)
        {
            // user might have clicked on a malicious link - should be logged
            throw new ArgumentException("invalid return URL");
        }

        // start challenge and roundtrip the return URL and scheme 
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Page("/externallogin/callback"),

            Items =
            {
                { "returnUrl", returnUrl },
                { "scheme", scheme },
            }
        };

        return Challenge(props, scheme);
    }
}
