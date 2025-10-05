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

namespace HiveSpace.UserService.Api.Pages.Account.Register;

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
    private readonly IConfiguration _configuration;
    private readonly ILogger<Index> _logger;

    public ViewModel View { get; set; } = default!;

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public Index(
        IIdentityServerInteractionService interaction,
        IAuthenticationSchemeProvider schemeProvider,
        IIdentityProviderStore identityProviderStore,
        IEventService events,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        ILogger<Index> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _interaction = interaction;
        _schemeProvider = schemeProvider;
        _identityProviderStore = identityProviderStore;
        _events = events;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IActionResult> OnGet(string? returnUrl)
    {
        // Clear all error messages on page refresh/load
        ClearAllErrors();

        // If the OAuth middleware redirected here with an error query param (e.g. ?error=...)
        // move that value to TempData and perform a redirect (PRG) so the error does not
        // persist on browser refresh. The redirected request will read from TempData.
        var qError = Request.Query["error"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(qError))
        {
            TempData["ExternalError"] = qError;
            // Preserve returnUrl when redirecting
            return RedirectToPage(new { returnUrl });
        }

        await BuildModelAsync(returnUrl);

        if (View.IsExternalRegistrationOnly)
        {
            // we only have one option for registration and it's an external provider
            return RedirectToPage("/ExternalLogin/Challenge", new { scheme = View.ExternalRegistrationScheme, returnUrl });
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        // Check if we are in the context of an authorization request
        var context = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        // Treat null or empty button as register attempt, only cancel if explicitly set to something else
        if (!string.IsNullOrEmpty(Input.Button) && Input.Button != "register")
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
            var user = await _userManager.FindByEmailAsync(Input.Email!);
            if (user is not null)
            {
                const string error = "invalid credentials";
                await _events.RaiseAsync(new UserLoginFailureEvent(Input.Email, error, clientId: context?.Client.ClientId));
                Telemetry.Metrics.UserLoginFailure(context?.Client.ClientId, IdentityServerConstants.LocalIdentityProvider, error);
                AddApiError("Email is already registered, please use a different email");
                await BuildModelAsync(Input.ReturnUrl);
                return Page();
            }
            // Create the user
            var newUser = new ApplicationUser 
            { 
                UserName = Input.Email, 
                Email = Input.Email,
                FullName = Input.FullName ?? string.Empty
            };

            var result = await _userManager.CreateAsync(newUser, Input.Password!);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                // Automatically sign in the user after registration
                await _signInManager.SignInAsync(newUser, isPersistent: false);

                // Raise successful registration event
                await _events.RaiseAsync(new UserLoginSuccessEvent(newUser.UserName, newUser.Id.ToString(), newUser.UserName, clientId: context?.Client?.ClientId));

                if (context != null)
                {
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
                    return Redirect("~/");
                }
                else
                {
                    // user might have clicked on a malicious link - should be logged
                    // user might have clicked on a malicious link - should be logged
                    _logger.LogWarning("Invalid return URL detected during registration: {ReturnUrl}", Input.ReturnUrl);
                    throw new ArgumentException("invalid return URL");
                }
            }

            // If we got this far, something failed, add errors to ModelState
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // something went wrong, show form with error
        TransferModelStateErrorsToApiErrors();
        await BuildModelAsync(Input.ReturnUrl);
        return Page();
    }
    
    /// <summary>
    /// Adds a single API error message for display in _ApiErrors partial
    /// </summary>
    private void AddApiError(string message)
    {
        var errors = ViewData["ApiErrors"] as List<string> ?? new List<string>();
        errors.Add(message);
        ViewData["ApiErrors"] = errors;
    }
    
    /// <summary>
    /// Transfers ModelState errors to ViewData["ApiErrors"] for display in _ApiErrors partial
    /// </summary>
    private void TransferModelStateErrorsToApiErrors()
    {
        if (!ModelState.IsValid)
        {
            var errors = new List<string>();
            
            // Add all ModelState errors
            foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
            {
                if (!string.IsNullOrWhiteSpace(modelError.ErrorMessage))
                {
                    errors.Add(modelError.ErrorMessage);
                }
            }
            
            if (errors.Count > 0)
            {
                ViewData["ApiErrors"] = errors;
            }
        }
    }

    /// <summary>
    /// Clears all error messages from ViewData and ModelState on page refresh
    /// </summary>
    private void ClearAllErrors()
    {
        // Clear ViewData errors
        ViewData.Remove("ApiErrors");
        ViewData.Remove("ErrorMessage");
        
        // Clear ModelState errors
        ModelState.Clear();
        
        // Clear TempData errors (except for external errors which are handled separately)
        if (TempData.ContainsKey("ApiErrors"))
            TempData.Remove("ApiErrors");
        if (TempData.ContainsKey("ErrorMessage"))
            TempData.Remove("ErrorMessage");
    }

    private async Task BuildModelAsync(string? returnUrl)
    {
        Input ??= new InputModel();
        Input.ReturnUrl = returnUrl;

        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

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
            if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
            {
                providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
            }
        }

        View = new ViewModel
        {
            EnableLocalRegistration = allowLocal,
            ExternalProviders = providers
        };

        // Set external error message from TempData if it exists
        if (TempData.ContainsKey("ExternalError"))
        {
            View.ExternalErrorMessage = TempData["ExternalError"] as string;
        }
    }
}