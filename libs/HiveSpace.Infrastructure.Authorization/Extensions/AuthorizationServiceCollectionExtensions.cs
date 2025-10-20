using Duende.IdentityServer;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.Infrastructure.Authorization.Extensions;

/// <summary>
/// Service collection extensions for HiveSpace authorization.
/// Provides reusable authorization policies for all microservices.
/// </summary>
public static class AuthorizationServiceCollectionExtensions
{
    /// <summary>
    /// Adds HiveSpace authorization policies for services that use JWT tokens from IdentityServer.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="serviceScope">The expected scope for this service (e.g., "user.fullaccess", "order.fullaccess")</param>
    /// <param name="useLocalApi">Whether to include LocalApi authentication scheme (for services that host IdentityServer)</param>
    public static void AddHiveSpaceAuthorization(this IServiceCollection services, string serviceScope, bool useLocalApi = false)
    {
        services.AddAuthorization(options =>
        {
            var authSchemes = new List<string> { "Bearer" };
            if (useLocalApi)
            {
                authSchemes.Add(IdentityServerConstants.LocalApi.AuthenticationScheme);
            }

            // Base policy - requires valid service scope
            var basePolicyName = GetBasePolicyName(serviceScope);
            options.AddPolicy(basePolicyName, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", serviceScope);
                foreach (var scheme in authSchemes)
                {
                    policy.AuthenticationSchemes.Add(scheme);
                }
            });

            // System Admin only - highest level access
            options.AddPolicy("RequireSystemAdmin", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", serviceScope);
                policy.RequireRole("SystemAdmin");
                foreach (var scheme in authSchemes)
                {
                    policy.AuthenticationSchemes.Add(scheme);
                }
            });

            // Admin or SystemAdmin - administrative access
            options.AddPolicy("RequireAdmin", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", serviceScope);
                policy.RequireRole("Admin", "SystemAdmin");
                foreach (var scheme in authSchemes)
                {
                    policy.AuthenticationSchemes.Add(scheme);
                }
            });

            // Seller only - store management access
            options.AddPolicy("RequireSeller", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", serviceScope);
                policy.RequireRole("Seller");
                foreach (var scheme in authSchemes)
                {
                    policy.AuthenticationSchemes.Add(scheme);
                }
            });

            // Users (Seller + Customer) - general user access
            options.AddPolicy("RequireUser", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", serviceScope);
                // Allow users with Seller role or no role (Customer)
                policy.RequireAssertion(context =>
                {
                    var roleClaim = context.User.FindFirst("role")?.Value;
                    return roleClaim == "Seller" || string.IsNullOrEmpty(roleClaim);
                });
                foreach (var scheme in authSchemes)
                {
                    policy.AuthenticationSchemes.Add(scheme);
                }
            });

            // Admin + User - both administrative and user access
            options.AddPolicy("RequireAdminOrUser", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", serviceScope);
                policy.RequireAssertion(context =>
                {
                    var roleClaim = context.User.FindFirst("role")?.Value;
                    return roleClaim == "Admin" || roleClaim == "SystemAdmin" || 
                           roleClaim == "Seller" || string.IsNullOrEmpty(roleClaim);
                });
                foreach (var scheme in authSchemes)
                {
                    policy.AuthenticationSchemes.Add(scheme);
                }
            });

            // Customer only - regular customer access
            options.AddPolicy("RequireCustomer", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", serviceScope);
                policy.RequireAssertion(context =>
                {
                    var roleClaim = context.User.FindFirst("role")?.Value;
                    return string.IsNullOrEmpty(roleClaim); // Customer has no explicit role
                });
                foreach (var scheme in authSchemes)
                {
                    policy.AuthenticationSchemes.Add(scheme);
                }
            });
        });
    }

    /// <summary>
    /// Gets the base policy name for a given service scope.
    /// </summary>
    private static string GetBasePolicyName(string serviceScope)
    {
        return serviceScope switch
        {
            "user.fullaccess" => "RequireUserFullAccessScope",
            "order.fullaccess" => "RequireOrderFullAccessScope",
            "basket.fullaccess" => "RequireBasketFullAccessScope",
            "catalog.fullaccess" => "RequireCatalogFullAccessScope",
            _ => $"Require{serviceScope.Replace(".", "").Replace("fullaccess", "FullAccessScope")}"
        };
    }
}