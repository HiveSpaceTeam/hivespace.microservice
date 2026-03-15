using HiveSpace.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HiveSpace.Core.Contexts;

public sealed class UserContext(IHttpContextAccessor httpContextAccessor)
        : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new UnauthorizedException([]);

    private ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User
        ?? throw new UnauthorizedException([]);

    public Guid UserId
    {
        get
        {
            // ASP.NET Core maps the JWT 'sub' claim to ClaimTypes.NameIdentifier
            var subClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
                        
            if (string.IsNullOrEmpty(subClaim))
            {
                throw new UnauthorizedException([new(CommonErrorCode.SubClaimMissing, nameof(UserId))]);
            }
            
            if (!Guid.TryParse(subClaim, out Guid userId))
            {
                throw new UnauthorizedException([new(CommonErrorCode.SubClaimInvalid, nameof(UserId))]);
            }
            
            return userId;
        }
    }

    public string PhoneNumber => User.FindFirstValue(JwtRegisteredClaimNames.PhoneNumber) ?? "";
    public string Email => User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? throw new UnauthorizedException([]);
    
    public IReadOnlyList<string> Roles => User.Claims
        .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
        .Select(c => c.Value)
        .ToList();

    public bool IsSystemAdmin => Roles.Contains("SystemAdmin", StringComparer.OrdinalIgnoreCase);

    public bool IsAdmin => Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase) || 
                           Roles.Contains("SystemAdmin", StringComparer.OrdinalIgnoreCase);

    public bool IsSeller => Roles.Contains("Seller", StringComparer.OrdinalIgnoreCase);

    public bool IsCustomer => Roles.Contains("Customer", StringComparer.OrdinalIgnoreCase);

    public Guid? StoreId
    {
        get
        {
            var storeIdClaim = User.FindFirstValue("store_id");
            if (string.IsNullOrEmpty(storeIdClaim))
                return IsSeller ? throw new UnauthorizedException([new(CommonErrorCode.SubClaimInvalid, nameof(StoreId))]) : null;
            return Guid.TryParse(storeIdClaim, out var storeId) 
                ? storeId 
                : (IsSeller ? throw new UnauthorizedException([new(CommonErrorCode.SubClaimInvalid, nameof(StoreId))]) : null);
        }
    }

    public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? throw new UnauthorizedException([]);

}