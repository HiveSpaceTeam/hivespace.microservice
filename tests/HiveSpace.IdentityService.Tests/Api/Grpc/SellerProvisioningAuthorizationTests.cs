using System.Security.Claims;
using FluentAssertions;
using HiveSpace.IdentityService.Api.Configs;
using HiveSpace.Infrastructure.Authorization.Grpc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Api.Grpc;

public class SellerProvisioningAuthorizationTests
{
    [Fact]
    public void IsAuthorizedUserServiceSellerProvisioningClient_AllowsUserService()
    {
        var context = NewAuthorizationContext("user-service");

        var allowed = ServiceClientAuthorization.IsAuthorizedUserServiceSellerProvisioningClient(context);

        allowed.Should().BeTrue();
    }

    [Fact]
    public void IsAuthorizedUserServiceSellerProvisioningClient_RejectsCatalogService()
    {
        var context = NewAuthorizationContext("catalog-service");

        var allowed = ServiceClientAuthorization.IsAuthorizedUserServiceSellerProvisioningClient(context);

        allowed.Should().BeFalse();
    }

    [Fact]
    public void GetClients_IncludesUserServiceClientCredentialsClient()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Clients:userservice:ClientId"] = "user-service",
                ["Clients:userservice:ClientName"] = "User Service",
                ["Clients:userservice:ClientSecret"] = "secret",
                ["Clients:userservice:AllowedScopes:0"] = "identity.fullaccess",
                ["Clients:userservice:AccessTokenLifetime"] = "1800"
            })
            .Build();

        var client = Config.GetClients(configuration)
            .Single(c => c.ClientId == "user-service");

        client.AllowedGrantTypes.Should().ContainSingle("client_credentials");
        client.AllowedScopes.Should().ContainSingle("identity.fullaccess");
        client.ClientSecrets.Should().HaveCount(1);
    }

    private static AuthorizationHandlerContext NewAuthorizationContext(string clientId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("client_id", clientId)
        ], "Bearer"));

        return new AuthorizationHandlerContext([], user, null);
    }
}
