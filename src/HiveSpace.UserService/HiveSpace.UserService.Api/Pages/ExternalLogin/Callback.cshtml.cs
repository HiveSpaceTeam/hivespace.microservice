using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using HiveSpace.UserService.Infrastructure.Identity;
using HiveSpace.UserService.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace HiveSpace.UserService.Api.Pages.ExternalLogin;
[AllowAnonymous]
[SecurityHeaders]
public class Callback : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly ILogger<Callback> _logger;
    private readonly IEventService _events;

    public Callback(
        IIdentityServerInteractionService interaction,
        IEventService events,
        ILogger<Callback> logger,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _interaction = interaction;
        _logger = logger;
        _events = events;
    }

    public async Task<IActionResult> OnGet()
    {
        // read external identity from the temporary cookie
        var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
        if (result.Succeeded != true)
        {
            throw new InvalidOperationException($"External authentication error: {result.Failure}");
        }

        var externalUser = result.Principal ??
            throw new InvalidOperationException("External authentication produced a null Principal");

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var externalClaims = externalUser.Claims.Select(c => $"{c.Type}: {c.Value}");
            _logger.ExternalClaims(externalClaims);
        }

        // lookup our user and external provider info
        // try to determine the unique id of the external user (issued by the provider)
        // the most common claim type for that are the sub claim and the NameIdentifier
        // depending on the external provider, some other claim type might be used
        var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                          externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                          throw new InvalidOperationException("Unknown userid");

        var provider = result.Properties.Items["scheme"] ?? throw new InvalidOperationException("Null scheme in authentication properties");
        var providerUserId = userIdClaim.Value;

        // find external user
        var user = await _userManager.FindByLoginAsync(provider, providerUserId);
        if (user == null)
        {
            // this might be where you might initiate a custom workflow for user registration
            // in this sample we don't show how that would be done, as our sample implementation
            // simply auto-provisions new external user
            user = await AutoProvisionUserAsync(provider, providerUserId, externalUser.Claims);
        }

        // this allows us to collect any additional claims or properties
        // for the specific protocols used and store them in the local auth cookie.
        // this is typically used to store data needed for signout from those protocols.
        var additionalLocalClaims = new List<Claim>();
        var localSignInProps = new AuthenticationProperties();
        CaptureExternalLoginContext(result, additionalLocalClaims, localSignInProps);

        // issue authentication cookie for user
        await _signInManager.SignInWithClaimsAsync(user, localSignInProps, additionalLocalClaims);

        // delete temporary cookie used during external authentication
        await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

        // retrieve return URL
        var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

        // check if external login is in the context of an OIDC request
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.Id.ToString(), user.UserName, true, context?.Client.ClientId));
        Telemetry.Metrics.UserLogin(context?.Client.ClientId, provider!);

        if (context != null)
        {
            if (context.IsNativeClient())
            {
                // The client is native, so this change in how to
                // return the response is for better UX for the end user.
                return this.LoadingPage(returnUrl);
            }
        }

        return Redirect(returnUrl);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1851:Possible multiple enumerations of 'IEnumerable' collection", Justification = "<Pending>")]
    private async Task<ApplicationUser> AutoProvisionUserAsync(string provider, string providerUserId, IEnumerable<Claim> claims)
    {
        // Extract values from claims
        var email = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value
            ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

        var fullName = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)?.Value
            ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

        var firstName = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value
            ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;

        var lastName = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value
            ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;

        if (string.IsNullOrWhiteSpace(fullName))
        {
            if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
                fullName = $"{firstName} {lastName}";
            else if (!string.IsNullOrWhiteSpace(firstName))
                fullName = firstName;
            else if (!string.IsNullOrWhiteSpace(lastName))
                fullName = lastName;
            else
                fullName = "External User";
        }

        var userName = email ?? Guid.NewGuid().ToString();
        var genderClaim = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Gender)?.Value
            ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.Gender)?.Value;
        Gender? gender = null;
        if (Enum.TryParse<Gender>(genderClaim, true, out var parsedGender))
            gender = parsedGender;

        DateTime? dob = null;
        var dobClaim = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.BirthDate)?.Value
            ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.DateOfBirth)?.Value;
        if (DateTime.TryParse(dobClaim, out var parsedDob))
            dob = parsedDob;

        var user = new ApplicationUser
        {
            Email = email ?? $"{Guid.NewGuid()}@external.local",
            UserName = userName,
            FullName = fullName,
            PhoneNumber = null,
            Gender = gender?.ToString(),
            DateOfBirth = dob
        };

        var identityResult = await _userManager.CreateAsync(user);
        if (!identityResult.Succeeded)
        {
            _logger.LogError("Failed to create external user: {Error}", identityResult.Errors.First().Description);
            throw new InvalidOperationException(identityResult.Errors.First().Description);
        }

        // Add claims
        var filtered = new List<Claim>();
        if (!string.IsNullOrWhiteSpace(fullName))
            filtered.Add(new Claim(JwtClaimTypes.Name, fullName));
        if (!string.IsNullOrWhiteSpace(email))
            filtered.Add(new Claim(JwtClaimTypes.Email, email));
        if (!string.IsNullOrWhiteSpace(genderClaim))
            filtered.Add(new Claim(JwtClaimTypes.Gender, genderClaim));
        if (!string.IsNullOrWhiteSpace(dobClaim))
            filtered.Add(new Claim(JwtClaimTypes.BirthDate, dobClaim));

        if (filtered.Count != 0)
        {
            identityResult = await _userManager.AddClaimsAsync(user, filtered);
            if (!identityResult.Succeeded)
            {
                _logger.LogError("Failed to add claims to external user: {Error}", identityResult.Errors.First().Description);
                throw new InvalidOperationException(identityResult.Errors.First().Description);
            }
        }

        identityResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));
        if (!identityResult.Succeeded)
        {
            _logger.LogError("Failed to add external login: {Error}", identityResult.Errors.First().Description);
            throw new InvalidOperationException(identityResult.Errors.First().Description);
        }

        return user;
    }
    // if the external login is OIDC-based, there are certain things we need to preserve to make logout work
    // this will be different for WS-Fed, SAML2p or other protocols
    private static void CaptureExternalLoginContext(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
    {
        ArgumentNullException.ThrowIfNull(externalResult.Principal, nameof(externalResult.Principal));

        // capture the idp used to login, so the session knows where the user came from
        localClaims.Add(new Claim(JwtClaimTypes.IdentityProvider, externalResult.Properties?.Items["scheme"] ?? "unknown identity provider"));

        // if the external system sent a session id claim, copy it over
        // so we can use it for single sign-out
        var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
        if (sid != null)
        {
            localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
        }

        // if the external provider issued an id_token, we'll keep it for signout
        var idToken = externalResult.Properties?.GetTokenValue("id_token");
        if (idToken != null)
        {
            localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = idToken } });
        }
    }
}
