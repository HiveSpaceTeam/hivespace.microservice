using System.Security.Claims;

namespace HiveSpace.Testing.Shared.Doubles;

public static class FakeCurrentUser
{
    public static ClaimsPrincipal Create(
        string role = "buyer",
        string? userId = null,
        string? email = null,
        string? storeId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId ?? Guid.NewGuid().ToString()),
            new("sub", userId ?? Guid.NewGuid().ToString()),
            new(ClaimTypes.Email, email ?? "test.user@hivespace.local"),
            new(ClaimTypes.Role, role),
            new("role", role)
        };

        if (!string.IsNullOrWhiteSpace(storeId))
        {
            claims.Add(new Claim("store_id", storeId));
            claims.Add(new Claim("StoreId", storeId));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }
}
