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

    public Guid UserId =>
        Guid.TryParse(User.FindFirstValue("sub"), out Guid userId)
        ? userId
        : throw new UnauthorizedException([]);

    public string PhoneNumber => User.FindFirstValue(JwtRegisteredClaimNames.PhoneNumber) ?? "";
    public string Email => User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? throw new UnauthorizedException([]);

    public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? throw new UnauthorizedException([]);

}