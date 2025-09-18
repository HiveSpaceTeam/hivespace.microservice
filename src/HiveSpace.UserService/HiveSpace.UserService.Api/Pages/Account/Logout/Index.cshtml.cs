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

namespace HiveSpace.UserService.Api.Pages.Account.Logout;
[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IEventService _events;

    [BindProperty]
    public string? LogoutId { get; set; }

    public Index(SignInManager<ApplicationUser> signInManager, IIdentityServerInteractionService interaction, IEventService events)
    {
        _signInManager = signInManager;
        _interaction = interaction;
        _events = events;
    }

    public async Task<IActionResult> OnGet(string? logoutId)
    {
        LogoutId = logoutId;

        try
        {
            var showLogoutPrompt = LogoutOptions.ShowLogoutPrompt;

            if (User.Identity?.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                showLogoutPrompt = false;
            }
            else
            {
                var context = await _interaction.GetLogoutContextAsync(LogoutId);
                if (context?.ShowSignoutPrompt == false)
                {
                    // it's safe to automatically sign-out
                    showLogoutPrompt = false;
                }
            }

            if (showLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await OnPost();
            }

            return Page();
        }
        catch (Exception)
        {
            // Log the error and redirect to login
            // In a production environment, you would want to log this properly
            return RedirectToPage("/Account/Login/Index");
        }
    }

    public async Task<IActionResult> OnPost()
    {
        try
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                // Get the logout context using the LogoutId from the request
                var logoutRequest = await _interaction.GetLogoutContextAsync(LogoutId);
                
                // if there's no current logout context, we need to create one
                // this captures necessary info from the current logged in user
                // this can still return null if there is no context needed
                if (logoutRequest == null && string.IsNullOrEmpty(LogoutId))
                {
                    LogoutId = await _interaction.CreateLogoutContextAsync();
                    logoutRequest = await _interaction.GetLogoutContextAsync(LogoutId);
                }

                // Perform the actual sign out
                await _signInManager.SignOutAsync();
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));

                // if there's a valid post logout redirect URI, use it
                if (!string.IsNullOrEmpty(logoutRequest?.PostLogoutRedirectUri))
                {
                    return Redirect(logoutRequest.PostLogoutRedirectUri);
                }

                // Otherwise, redirect to login page
                return RedirectToPage("/Account/Login/Index");
            }

            return RedirectToPage("/Account/Login/Index");
        }
        catch (Exception)
        {
            // If anything goes wrong during logout, still redirect to login
            return RedirectToPage("/Account/Login/Index");
        }
    }
}
