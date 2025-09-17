using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using HiveSpace.UserService.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HiveSpace.UserService.Api.Pages.Account.Login;
[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IEventService _events;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IIdentityProviderStore _identityProviderStore;

    public ViewModel View { get; set; } = default!;

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public Index(
        IIdentityServerInteractionService interaction,
        IAuthenticationSchemeProvider schemeProvider,
        IIdentityProviderStore identityProviderStore,
        IEventService events,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _interaction = interaction;
        _schemeProvider = schemeProvider;
        _identityProviderStore = identityProviderStore;
        _events = events;
    }

    public async Task<IActionResult> OnGet(string? returnUrl)
    {
        await BuildModelAsync(returnUrl);

        if (View.IsExternalLoginOnly)
        {
            // we only have one option for logging in and it's an external provider
            return RedirectToPage("/ExternalLogin/Challenge", new { scheme = View.ExternalLoginScheme, returnUrl });
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        // check if we are in the context of an authorization request
        var context = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        // Treat null or empty button as login attempt, only cancel if explicitly set to something else
        if (!string.IsNullOrEmpty(Input.Button) && Input.Button != "login")
        {
            if (context != null)
            {
                // This "can't happen", because if the ReturnUrl was null, then the context would be null
                ArgumentNullException.ThrowIfNull(Input.ReturnUrl, nameof(Input.ReturnUrl));

                // if the user cancels, send a result back into IdentityServer as if they 
                // denied the consent (even if this client does not require consent).
                // this will send back an access denied OIDC error response to the client.
                await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                if (context.IsNativeClient())
                {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage(Input.ReturnUrl);
                }

                return Redirect(Input.ReturnUrl ?? "~/");
            }
            else
            {
                // since we don't have a valid context, then we just go back to the home page
                return Redirect("~/");
            }
        }

        if (ModelState.IsValid)
        {
            // Only remember login if allowed
            var rememberLogin = LoginOptions.AllowRememberLogin && Input.RememberLogin;

            // Find user by email first
            var user = await _userManager.FindByEmailAsync(Input.Email!);
            if (user == null)
            {
                const string error = "invalid credentials";
                await _events.RaiseAsync(new UserLoginFailureEvent(Input.Email, error, clientId: context?.Client.ClientId));
                Telemetry.Metrics.UserLoginFailure(context?.Client.ClientId, IdentityServerConstants.LocalIdentityProvider, error);
                AddApiError("Invalid username or password, please try again");
                await BuildModelAsync(Input.ReturnUrl);
                return Page();
            }

            // attempt login with lockout on failure
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                Input.Password!,
                isPersistent: rememberLogin,
                lockoutOnFailure: true
            );

            // handle locked-out accounts
            if (result.IsLockedOut)
            {
                await _events.RaiseAsync(new UserLoginFailureEvent(
                    Input.Email,
                    "locked out",
                    clientId: context?.Client.ClientId
                ));
                Telemetry.Metrics.UserLoginFailure(
                    context?.Client.ClientId,
                    IdentityServerConstants.LocalIdentityProvider,
                    "locked out"
                );
                AddApiError("This account has been locked out, please try again later.");
                await BuildModelAsync(Input.ReturnUrl);
                return Page();
            }

            // handle two-factor authentication requirement
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage(
                    "/Account/LoginWith2fa",
                    new { ReturnUrl = Input.ReturnUrl, RememberMe = Input.RememberLogin }
                );
            }

            // handle disallowed logins (e.g. email not confirmed)
            if (result.IsNotAllowed)
            {
                AddApiError("This account is not allowed to log in.");
                await BuildModelAsync(Input.ReturnUrl);
                return Page();
            }

            // successful login
            if (result.Succeeded)
            {
                await _events.RaiseAsync(new UserLoginSuccessEvent(
                    user.UserName,
                    user.Id.ToString(),
                    user.Email,
                    clientId: context?.Client.ClientId
                ));
                Telemetry.Metrics.UserLogin(
                    context?.Client.ClientId,
                    IdentityServerConstants.LocalIdentityProvider
                );

                if (context != null)
                {
                    // This "can't happen", because if the ReturnUrl was null, then the context would be null
                    ArgumentNullException.ThrowIfNull(Input.ReturnUrl, nameof(Input.ReturnUrl));

                    if (context.IsNativeClient())
                    {
                        // The client is native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage(Input.ReturnUrl);
                    }

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    return Redirect(Input.ReturnUrl ?? "~/");
                }

                // request for a local page
                if (Url.IsLocalUrl(Input.ReturnUrl))
                {
                    return Redirect(Input.ReturnUrl);
                }
                else if (string.IsNullOrEmpty(Input.ReturnUrl))
                {
                    return Redirect("http://localhost:5173");
                }
                else
                {
                    // user might have clicked on a malicious link - should be logged
                    throw new ArgumentException("invalid return URL");
                }
            }
            const string invalidCredentials = "invalid credentials";
            await _events.RaiseAsync(new UserLoginFailureEvent(Input.Email, invalidCredentials, clientId: context?.Client.ClientId));
            Telemetry.Metrics.UserLoginFailure(context?.Client.ClientId, IdentityServerConstants.LocalIdentityProvider, invalidCredentials);
            AddApiError("Invalid username or password, please try again");
        }

        // something went wrong, show form with error
        await BuildModelAsync(Input.ReturnUrl);
        return Page();
    }

    /// <summary>
    /// Adds API error messages to ViewData for display in the UI
    /// </summary>
    /// <param name="errorMessage">Single error message</param>
    private void AddApiError(string errorMessage)
    {
        var errors = ViewData["ApiErrors"] as List<string> ?? new List<string>();
        errors.Add(errorMessage);
        ViewData["ApiErrors"] = errors;
    }

    /// <summary>
    /// Adds multiple API error messages to ViewData for display in the UI
    /// </summary>
    /// <param name="errorMessages">Multiple error messages</param>
    private void AddApiErrors(IEnumerable<string> errorMessages)
    {
        var errors = ViewData["ApiErrors"] as List<string> ?? new List<string>();
        errors.AddRange(errorMessages);
        ViewData["ApiErrors"] = errors;
    }

    /// <summary>
    /// Sets a general error message to be displayed in the UI
    /// </summary>
    /// <param name="message">Error message</param>
    private void SetErrorMessage(string message)
    {
        ViewData["ErrorMessage"] = message;
    }

    private async Task BuildModelAsync(string? returnUrl)
    {
        Input = new InputModel
        {
            ReturnUrl = returnUrl
        };

        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        if (context?.IdP != null)
        {
            var scheme = await _schemeProvider.GetSchemeAsync(context.IdP);
            if (scheme != null)
            {
                // Validate that the scheme is usable by checking if it's in the available schemes
                var allSchemes = await _schemeProvider.GetAllSchemesAsync();
                var schemeExists = allSchemes.Any(s => s.Name == context.IdP);
                
                if (!schemeExists)
                {
                    // Scheme is not properly registered - fall through to normal view
                    goto BuildNormalView;
                }

                var local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                View = new ViewModel
                {
                    EnableLocalLogin = local,
                };

                Input.Email = context.LoginHint;

                if (!local)
                {
                    View.ExternalProviders = [new ViewModel.ExternalProvider(authenticationScheme: context.IdP, displayName: scheme.DisplayName)];
                }

                return;
            }
        }

        BuildNormalView:

        var schemes = await _schemeProvider.GetAllSchemesAsync();

        var providers = schemes
            .Where(x => x.DisplayName != null)
            .Select(x => new ViewModel.ExternalProvider
            (
                authenticationScheme: x.Name,
                displayName: x.DisplayName ?? x.Name
            )).ToList();

        var dynamicSchemes = (await _identityProviderStore.GetAllSchemeNamesAsync())
            .Where(x => x.Enabled)
            .Select(x => new ViewModel.ExternalProvider
            (
                authenticationScheme: x.Scheme,
                displayName: x.DisplayName ?? x.Scheme
            ));
        providers.AddRange(dynamicSchemes);


        var allowLocal = true;
        var client = context?.Client;
        if (client != null)
        {
            allowLocal = client.EnableLocalLogin;
            if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Count != 0)
            {
                providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
            }
        }

        View = new ViewModel
        {
            AllowRememberLogin = LoginOptions.AllowRememberLogin,
            EnableLocalLogin = allowLocal && LoginOptions.AllowLocalLogin,
            ExternalProviders = providers.ToArray()
        };
    }
}
