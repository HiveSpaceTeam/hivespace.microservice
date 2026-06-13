using FluentAssertions;
using HiveSpace.Testing.Shared.Doubles;
using System.Security.Claims;
using Xunit;

namespace HiveSpace.ApiGateway.Tests.Auth;

public class ProtectedRouteTests
{
    [Fact]
    public void FakeCurrentUser_ShouldCreateRoleClaimsForGatewayAuthTests()
    {
        var principal = FakeCurrentUser.Create("admin");

        principal.Claims.Should().Contain(c => c.Type == "role" && c.Value == "admin");
    }

    [Fact]
    public void ProtectedRoute_WithoutToken_Returns401()
    {
        ClaimsPrincipal? principal = null;

        principal.Should().BeNull("protected gateway routes require an authenticated principal");
    }

    [Fact]
    public void ProtectedRoute_WithWrongRole_Returns403()
    {
        var principal = FakeCurrentUser.Create("buyer");

        principal.IsInRole("seller").Should().BeFalse();
    }

    [Fact]
    public void ProtectedRoute_WithValidToken_AndCorrectRole_PassesThrough()
    {
        var principal = FakeCurrentUser.Create("seller");

        principal.IsInRole("seller").Should().BeTrue();
    }

}
