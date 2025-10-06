using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using HiveSpace.UserService.Infrastructure.Identity;
using HiveSpace.UserService.Domain.Enums;
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
    private readonly IConfiguration _configuration;

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
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _interaction = interaction;
        _schemeProvider = schemeProvider;
        _identityProviderStore = identityProviderStore;
        _events = events;
        _configuration = configuration;
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
            try
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

                // Check if user is active before attempting login
                if (user.Status != (int)UserStatus.Active)
                {
                    const string error = "user inactive";
                    await _events.RaiseAsync(new UserLoginFailureEvent(Input.Email, error, clientId: context?.Client.ClientId));
                    Telemetry.Metrics.UserLoginFailure(context?.Client.ClientId, IdentityServerConstants.LocalIdentityProvider, error);
                    AddApiError("This account is inactive. Please contact admin to activate it.");
                    await BuildModelAsync(Input.ReturnUrl);
                    return Page();
                }

                // Role-based client restriction: deny sign-in if user not allowed for requesting client
                try
                {
                    var clientId = context?.Client?.ClientId?.ToLowerInvariant();
                    if (!string.IsNullOrEmpty(clientId))
                    {
                        // Admin portal: only SystemAdmin or Admin
                        if (string.Equals(clientId, "adminportal", StringComparison.OrdinalIgnoreCase))
                        {
                            var isSystemAdmin = await _userManager.IsInRoleAsync(user, "SystemAdmin");
                            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                            if (!isSystemAdmin && !isAdmin)
                            {
                                const string error = "not authorized for admin portal";
                                await _events.RaiseAsync(new UserLoginFailureEvent(Input.Email, error, clientId: context?.Client.ClientId));
                                Telemetry.Metrics.UserLoginFailure(context?.Client.ClientId, IdentityServerConstants.LocalIdentityProvider, error);
                                AddApiError("You are not authorized to sign in to the Admin Portal.");
                                await BuildModelAsync(Input.ReturnUrl);
                                return Page();
                            }
                        }

                        // Seller center: only Seller role (or users with StoreId set)
                        if (string.Equals(clientId, "sellercenter", StringComparison.OrdinalIgnoreCase))
                        {
                            var isSeller = await _userManager.IsInRoleAsync(user, "Seller");
                            // Fallback: if your domain marks sellers with StoreId, check that too (safe reflection)
                            var hasStore = false;
                            var storeProp = user.GetType().GetProperty("StoreId");
                            if (storeProp != null)
                            {
                                var val = storeProp.GetValue(user);
                                hasStore = val != null;
                            }
                            if (!isSeller && !hasStore)
                            {
                                const string error = "not authorized for seller center";
                                await _events.RaiseAsync(new UserLoginFailureEvent(Input.Email, error, clientId: context?.Client.ClientId));
                                Telemetry.Metrics.UserLoginFailure(context?.Client.ClientId, IdentityServerConstants.LocalIdentityProvider, error);
                                AddApiError("You are not authorized to sign in to the Seller Center.");
                                await BuildModelAsync(Input.ReturnUrl);
                                return Page();
                            }
                        }
                    }
                }
                catch
                {
                    // If role checks fail unexpectedly, don't reveal details; fall through to normal auth failure path
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
                        user.UserName // Use display name or full name if available
                    ));
                    Telemetry.Metrics.UserLogin(
                        context?.Client.ClientId,
                        IdentityServerConstants.LocalIdentityProvider
                    );

                    if (context != null)
                    {
                        // This "can't happen", because if the ReturnUrl was null, then the context would be null
                        if (Input.ReturnUrl == null)
                        {
                            AddApiError("Invalid return URL provided.");
                            await BuildModelAsync(Input.ReturnUrl);
                            return Page();
                        }

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
                             // Use a configurable default redirect URL from configuration
                        var defaultRedirectUrl = _configuration["DefaultRedirectUrl"];
                        if (string.IsNullOrWhiteSpace(defaultRedirectUrl))
                        {
                            defaultRedirectUrl = "/"; // fallback to root if not configured
                        }
                        return Redirect(defaultRedirectUrl);
                    }
                    else
                    {
                        // user might have clicked on a malicious link - handle gracefully
                        AddApiError("Invalid return URL provided.");
                        await BuildModelAsync(Input.ReturnUrl);
                        return Page();
                    }
                }
                
                // If we get here, login failed for some other reason
                const string invalidCredentials = "invalid credentials";
                await _events.RaiseAsync(new UserLoginFailureEvent(Input.Email, invalidCredentials, clientId: context?.Client.ClientId));
                Telemetry.Metrics.UserLoginFailure(context?.Client.ClientId, IdentityServerConstants.LocalIdentityProvider, invalidCredentials);
                AddApiError("Invalid username or password, please try again");
            }
            catch (Exception)
            {
                // Log the exception but don't expose internal details to user
                // You can add logging here if needed
                AddApiError("An error occurred during login. Please try again.");
            }
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
        
        // DO NOT clear TempData["ApiErrors"] here - it might contain errors from external auth flows
        // that need to be displayed. The BuildModelAsync method will handle transferring them to ViewData.
        // Only clear TempData errors that are not from external auth flows
        if (TempData.ContainsKey("ErrorMessage"))
            TempData.Remove("ErrorMessage");
    }

    private async Task BuildModelAsync(string? returnUrl)
    {
        Input = new InputModel
        {
            ReturnUrl = returnUrl
        };

        // If an external provider previously reported an error, surface it in the ViewModel.
        // Prefer TempData (set by PRG) then query param as a fallback.
        var externalError = TempData["ExternalError"] as string;
        if (string.IsNullOrWhiteSpace(externalError))
        {
            externalError = Request.Query["error"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(externalError))
            {
                // Ensure we don't keep showing this on refresh — move it into TempData and
                // let the OnGet PRG flow clear the query on the next request.
                TempData["ExternalError"] = externalError;
            }
        }

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
                    ClientId = context?.Client?.ClientId,
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
            // Hide external providers for sensitive clients such as the admin portal
            // so administrators must login using local credentials only.
            // You can adjust the client id or make this configurable if needed.
            if (string.Equals(client.ClientId, "adminportal", StringComparison.OrdinalIgnoreCase))
            {
                providers = new List<ViewModel.ExternalProvider>();
            }
        }

        View = new ViewModel
        {
            AllowRememberLogin = LoginOptions.AllowRememberLogin,
            EnableLocalLogin = allowLocal && LoginOptions.AllowLocalLogin,
            ExternalProviders = providers.ToArray(),
            ClientId = context?.Client?.ClientId,
            // ExternalErrorMessage intentionally omitted; use ApiErrors via ViewData instead
        };

        // If ApiErrors were set by the external auth flow, deserialize them and
        // put into ViewData for the shared _ApiErrors partial to render.
        if (TempData.TryGetValue("ApiErrors", out var apiErrorsObj))
        {
            if (apiErrorsObj is string apiErrorsJson)
            {
                try
                {
                    var messages = System.Text.Json.JsonSerializer.Deserialize<List<string>>(apiErrorsJson) ?? new List<string>();
                    ViewData["ApiErrors"] = messages;
                }
                catch
                {
                    // If deserialization fails, fall back to the ExternalError single message
                    if (!string.IsNullOrWhiteSpace(externalError))
                    {
                        ViewData["ApiErrors"] = new List<string> { externalError };
                    }
                }
            }
        }
        else
        {
            // no ApiErrors in TempData
        }
    }
}
