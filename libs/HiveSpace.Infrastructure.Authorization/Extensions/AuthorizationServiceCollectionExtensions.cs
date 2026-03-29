using Duende.IdentityServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.Infrastructure.Authorization.Extensions;

public static class AuthorizationServiceCollectionExtensions
{
    /// <summary>
    /// Adds HiveSpace authorization policies for services that use JWT tokens from IdentityServer.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="serviceScope">The expected scope for this service (e.g., "user.fullaccess", "order.fullaccess")</param>
    /// <param name="useLocalApi">Whether to include LocalApi authentication scheme (for services that host IdentityServer)</param>
    /// <param name="useJwtBearer">Whether to include JWT Bearer authentication scheme (for services that consume JWT tokens)</param>
    public static void AddHiveSpaceAuthorization(this IServiceCollection services, string serviceScope, bool useLocalApi = false, bool useJwtBearer = true)
    {
        services.AddAuthorization(options =>
        {
            var authSchemes = new List<string>();

            if (useJwtBearer)
                authSchemes.Add("Bearer");

            if (useLocalApi)
                authSchemes.Add(IdentityServerConstants.LocalApi.AuthenticationScheme);

            if (authSchemes.Count == 0)
                authSchemes.Add("Bearer");

            // Base builder: scope + auth + schemes defined once, reused by all policies.
            AuthorizationPolicyBuilder ScopedPolicy() =>
                new AuthorizationPolicyBuilder(authSchemes.ToArray())
                    .RequireAuthenticatedUser()
                    .RequireClaim("scope", serviceScope);

            // Default policy — used by bare [Authorize] attributes.
            options.DefaultPolicy = ScopedPolicy().Build();

            // Role policies — scope is enforced via ScopedPolicy(), each policy only adds role logic.
            options.AddPolicy("RequireSystemAdmin", ScopedPolicy()
                .RequireAssertion(ctx => ctx.User.FindFirst("role")?.Value == "SystemAdmin")
                .Build());

            options.AddPolicy("RequireAdmin", ScopedPolicy()
                .RequireAssertion(ctx =>
                {
                    var role = ctx.User.FindFirst("role")?.Value;
                    return role == "Admin" || role == "SystemAdmin";
                })
                .Build());

            options.AddPolicy("RequireSeller", ScopedPolicy()
                .RequireAssertion(ctx => ctx.User.FindFirst("role")?.Value == "Seller")
                .Build());

            options.AddPolicy("RequireUser", ScopedPolicy()
                .RequireAssertion(ctx =>
                {
                    var role = ctx.User.FindFirst("role")?.Value;
                    return role == "Seller" || role == "Customer";
                })
                .Build());

            options.AddPolicy("RequireCustomer", ScopedPolicy()
                .RequireAssertion(ctx => ctx.User.FindFirst("role")?.Value == "Customer")
                .Build());

            options.AddPolicy("RequireAdminOrUser", ScopedPolicy()
                .RequireAssertion(ctx =>
                {
                    var role = ctx.User.FindFirst("role")?.Value;
                    return role == "Admin" || role == "SystemAdmin" || role == "Seller" || role == "Customer";
                })
                .Build());
        });
    }

    /// <summary>
    /// Adds HiveSpace authorization policies for services that only use LocalApi authentication
    /// (like User Service that hosts IdentityServer).
    /// </summary>
    public static void AddHiveSpaceAuthorizationForLocalApi(this IServiceCollection services, string serviceScope)
    {
        services.AddHiveSpaceAuthorization(serviceScope, useLocalApi: true, useJwtBearer: false);
    }
}
