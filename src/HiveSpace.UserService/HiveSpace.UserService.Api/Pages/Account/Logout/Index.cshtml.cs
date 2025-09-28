using Duende.IdentityModel;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;
using HiveSpace.UserService.Api.Pages;
using HiveSpace.UserService.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HiveSpace.UserService.Api.Pages.Account.Logout;
[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IEventService _events;
    private readonly ILogger<Index> _logger;
    private readonly IConfiguration _configuration;

    [BindProperty]
    public string? LogoutId { get; set; }

    // Expose logout context values to the Razor page so it can render client-specific links
    public string? ClientId { get; private set; }
    public string? ClientName { get; private set; }
    public string? PostLogoutRedirectUri { get; private set; }
    public bool HasClient => !string.IsNullOrEmpty(ClientId) || !string.IsNullOrEmpty(ClientName);

    public Index(SignInManager<ApplicationUser> signInManager, IIdentityServerInteractionService interaction, IEventService events, ILogger<Index> logger, IConfiguration configuration)
    {
        _signInManager = signInManager;
        _interaction = interaction;
        _events = events;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<IActionResult> OnGet(string? logoutId)
    {
        LogoutId = logoutId;

        // Do not redirect to the login page here. Instead build the logout view context
        try
        {
            // Read (or create) the logout context so we know which client initiated the logout
            var logoutRequest = await _interaction.GetLogoutContextAsync(LogoutId);
            if (logoutRequest == null && string.IsNullOrEmpty(LogoutId))
            {
                LogoutId = await _interaction.CreateLogoutContextAsync();
                logoutRequest = await _interaction.GetLogoutContextAsync(LogoutId);
            }

            ClientId = logoutRequest?.ClientId;
            ClientName = logoutRequest?.ClientName;
            PostLogoutRedirectUri = logoutRequest?.PostLogoutRedirectUri;

            _logger.LogDebug("Logout context for logoutId={LogoutId}: clientId={ClientId}, postLogoutRedirectUri={PostLogoutRedirectUri}", LogoutId, ClientId, PostLogoutRedirectUri);

            // If the user is authenticated, sign them out now (preserve logout context for the view)
            if (User.Identity?.IsAuthenticated == true)
            {
                await _signInManager.SignOutAsync();
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // Determine final redirect - prefer the logout context's PostLogoutRedirectUri
            var finalRedirect = PostLogoutRedirectUri;
            if (string.IsNullOrEmpty(finalRedirect) && !string.IsNullOrEmpty(ClientId))
            {
                // Try to read the client's configured PostLogoutRedirectUris or ClientUri from appsettings as a fallback
                try
                {
                    var clientSection = _configuration.GetSection("Clients").GetSection(ClientId);
                    var postLogoutUris = clientSection.GetSection("PostLogoutRedirectUris").Get<string[]>();
                    if (postLogoutUris != null && postLogoutUris.Length > 0)
                    {
                        finalRedirect = postLogoutUris[0];
                    }
                    else
                    {
                        finalRedirect = clientSection.GetValue<string>("ClientUri");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read fallback post-logout redirect for client {ClientId}", ClientId);
                }
            }

            finalRedirect ??= _configuration["DefaultRedirectUrl"];

            if (!string.IsNullOrEmpty(finalRedirect))
            {
                _logger.LogDebug("Redirecting user after logout to {FinalRedirect}", finalRedirect);
                return Redirect(finalRedirect);
            }

            // Otherwise return the logout page so the view can render client-specific links or instructions
            return Page();
        }
        catch (Exception)
        {
            // Log the error in production; show the logout page as a fallback
            return Page();
        }
    }

    public async Task<IActionResult> OnPost()
    {
        try
        {
            // Build/ensure logout context so view can know the client
            var logoutRequest = await _interaction.GetLogoutContextAsync(LogoutId);
            if (logoutRequest == null && string.IsNullOrEmpty(LogoutId))
            {
                LogoutId = await _interaction.CreateLogoutContextAsync();
                logoutRequest = await _interaction.GetLogoutContextAsync(LogoutId);
            }

            ClientId = logoutRequest?.ClientId;
            ClientName = logoutRequest?.ClientName;
            PostLogoutRedirectUri = logoutRequest?.PostLogoutRedirectUri;

            // If the user is authenticated, sign them out
            if (User.Identity?.IsAuthenticated == true)
            {
                await _signInManager.SignOutAsync();
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // If the client provided a post-logout redirect URI, redirect the user there.
            if (!string.IsNullOrEmpty(PostLogoutRedirectUri))
            {
                return Redirect(PostLogoutRedirectUri);
            }

            // Otherwise render the logout page so the UI can show client-specific sign-in links.
            return Page();
        }
        catch (Exception)
        {
            // If anything goes wrong during logout, show the logout page as fallback
            return Page();
        }
    }
}
