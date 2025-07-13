using Duende.IdentityModel;
using HiveSpace.IdentityService.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using HiveSpace.IdentityService.Domain.Aggregates;

namespace HiveSpace.IdentityService.Infrastructure;
public class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            context.Database.Migrate();

            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Seed Alice
            var alice = userMgr.FindByNameAsync("alice").Result;
            if (alice == null)
            {
                alice = new ApplicationUser(
                    email: "AliceSmith@example.com",
                    userName: "alice",
                    fullName: "Alice Smith",
                    phoneNumber: null,
                    gender: null,
                    dob: null
                )
                {
                    EmailConfirmed = true
                };

                var result = userMgr.CreateAsync(alice, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    Log.Error("Failed to create Alice: {Error}", result.Errors.First().Description);
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(alice, new[]
                {
                    new Claim(JwtClaimTypes.Name, "Alice Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Alice"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.WebSite, "http://alice.example.com"),
                }).Result;
                if (!result.Succeeded)
                {
                    Log.Error("Failed to add claims to Alice: {Error}", result.Errors.First().Description);
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Debug("alice created");
            }
            else
            {
                Log.Debug("alice already exists");
            }

            // Seed Bob
            var bob = userMgr.FindByNameAsync("bob").Result;
            if (bob == null)
            {
                bob = new ApplicationUser(
                    email: "BobSmith@example.com",
                    userName: "bob",
                    fullName: "Bob Smith",
                    phoneNumber: null,
                    gender: null,
                    dob: null
                )
                {
                    EmailConfirmed = true
                };

                var result = userMgr.CreateAsync(bob, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    Log.Error("Failed to create Bob: {Error}", result.Errors.First().Description);
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(bob, new[]
                {
                    new Claim(JwtClaimTypes.Name, "Bob Smith"),
                    new Claim(JwtClaimTypes.GivenName, "Bob"),
                    new Claim(JwtClaimTypes.FamilyName, "Smith"),
                    new Claim(JwtClaimTypes.WebSite, "http://bob.example.com"),
                    new Claim("location", "somewhere")
                }).Result;
                if (!result.Succeeded)
                {
                    Log.Error("Failed to add claims to Bob: {Error}", result.Errors.First().Description);
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Debug("bob created");
            }
            else
            {
                Log.Debug("bob already exists");
            }
        }
    }
}