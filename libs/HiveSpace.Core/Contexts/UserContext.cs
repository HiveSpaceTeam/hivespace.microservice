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
                throw new UnauthorizedException([new(CommonErrorCode.SubClaimMissing, "UserId")]);
            }
            
            if (!Guid.TryParse(subClaim, out Guid userId))
            {
                throw new UnauthorizedException([new(CommonErrorCode.SubClaimInvalid, "UserId")]);
            }
            
            return userId;
        }
    }

    public string PhoneNumber => User.FindFirstValue(JwtRegisteredClaimNames.PhoneNumber) ?? "";
    public string Email => User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? throw new UnauthorizedException([]);

    public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? throw new UnauthorizedException([]);

}