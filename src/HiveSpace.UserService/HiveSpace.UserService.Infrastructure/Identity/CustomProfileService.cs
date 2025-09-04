using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Infrastructure.Data;
using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Infrastructure.Identity;

/// <summary>
/// Custom profile service for IdentityServer that provides user claims during token creation.
/// This service enriches tokens with business-specific roles and permissions.
/// </summary>
public class CustomProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStoreRepository _storeRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomProfileService"/> class.
    /// </summary>
    /// <param name="userManager">The user manager instance to query user identity information.</param>
    /// <param name="storeRepository">The store repository to query business-specific information.</param>
    public CustomProfileService(UserManager<ApplicationUser> userManager, IStoreRepository storeRepository)
    {
        _userManager = userManager;
        _storeRepository = storeRepository;
    }

    /// <summary>
    /// This method is called whenever claims about a user are requested (e.g., during token creation).
    /// </summary>
    /// <param name="context">The context for the profile data request.</param>
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        // Get the user based on the subject ID from the authentication request.
        var user = await _userManager.GetUserAsync(context.Subject);
        
        if (user == null)
        {
            // If the user isn't found, no claims can be issued.
            return;
        }

        // Create a list to hold all the claims for the user.
        var claims = new List<Claim>
        {
            // Standard claims that every user gets.
            new Claim("sub", user.Id.ToString()),
            new Claim("email", user.Email ?? string.Empty),
            new Claim("name", user.FullName),
            new Claim("username", user.UserName ?? string.Empty)
        };

        // 1. Get and add standard ASP.NET Core Identity roles.
        // These are the "technical" roles like "Admin" and "SystemAdmin".
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim("role", role));
        }
        
        // 2. Check for the "Store Owner" business role from the domain layer.
        // This links a user's identity to a business-specific role (the Store Owner).
        if (user.StoreId.HasValue)
        {
            var store = await _storeRepository.GetByOwnerIdAsync(user.Id);
            if (store is not null)
            {
                claims.Add(new Claim("role", "store_owner"));
                claims.Add(new Claim("store_id", store.Id.ToString()));
                claims.Add(new Claim("store_name", store.StoreName));
            }
        }
        
        // 3. Add user status information
        claims.Add(new Claim("user_status", user.UserStatus));
        
        // 4. Add additional user information if available
        if (user.DateOfBirth.HasValue)
        {
            claims.Add(new Claim("birthdate", user.DateOfBirth.Value.ToString("yyyy-MM-dd")));
        }
        
        if (!string.IsNullOrEmpty(user.Gender))
        {
            claims.Add(new Claim("gender", user.Gender));
        }

        // Finally, issue the claims to the token.
        context.IssuedClaims = claims;
    }

    /// <summary>
    /// This method is called to check if the user is currently allowed to log in.
    /// It's a key security check to prevent inactive users from getting a token.
    /// </summary>
    /// <param name="context">The context for the active check.</param>
    public async Task IsActiveAsync(IsActiveContext context)
    {
        var user = await _userManager.GetUserAsync(context.Subject);
        
        // A user is considered active if they exist, are not locked out, and have an active status.
        context.IsActive = user is not null 
            && user.UserStatus == UserStatus.Active.ToString();
    }
}
