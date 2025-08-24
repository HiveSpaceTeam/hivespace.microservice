using Duende.IdentityModel;
using HiveSpace.UserService.Infrastructure.Data;
using HiveSpace.UserService.Infrastructure.Identity;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.UserService.Infrastructure;

public class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            context.Database.Migrate();

            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Seed Alice
            var alice = userMgr.FindByNameAsync("alice").Result;
            if (alice == null)
            {
                alice = new ApplicationUser
                {
                    UserName = "alice",
                    Email = "AliceSmith@example.com",
                    EmailConfirmed = true,
                    FullName = "Alice Smith",
                    PhoneNumber = "+1234567890",
                    DateOfBirth = new DateTime(1990, 1, 15),
                    Gender = "Female",
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var result = userMgr.CreateAsync(alice, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    Log.Error("Failed to create Alice: {Error}", result.Errors.First().Description);
                    throw new Exception(result.Errors.First().Description);
                }

                // Add some sample addresses for Alice
                var aliceHomeAddress = new Address(
                    fullName: "Alice Smith",
                    phoneNumber: "+1234567890",
                    street: "123 Main Street",
                    district: "Downtown",
                    province: "California",
                    country: "USA",
                    zipCode: "12345",
                    addressType: AddressType.Home
                );
                
                // Set the foreign key manually
                context.Entry(aliceHomeAddress).Property("UserId").CurrentValue = alice.Id;
                context.Addresses.Add(aliceHomeAddress);

                // Since this is the first address, it will be set as default by the service layer
                // For seeding purposes, we'll set it manually using reflection
                var isDefaultProperty = typeof(Address).GetProperty("IsDefault");
                isDefaultProperty?.SetValue(aliceHomeAddress, true);

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
                
                context.SaveChanges();
                Log.Debug("alice created with sample address");
            }
            else
            {
                Log.Debug("alice already exists");
            }

            // Seed Bob
            var bob = userMgr.FindByNameAsync("bob").Result;
            if (bob == null)
            {
                bob = new ApplicationUser
                {
                    UserName = "bob",
                    Email = "BobSmith@example.com",
                    EmailConfirmed = true,
                    FullName = "Bob Smith",
                    PhoneNumber = "+0987654321",
                    DateOfBirth = new DateTime(1985, 6, 20),
                    Gender = "Male",
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var result = userMgr.CreateAsync(bob, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    Log.Error("Failed to create Bob: {Error}", result.Errors.First().Description);
                    throw new Exception(result.Errors.First().Description);
                }

                // Add sample addresses for Bob
                var bobHomeAddress = new Address(
                    fullName: "Bob Smith",
                    phoneNumber: "+0987654321",
                    street: "456 Oak Avenue",
                    district: "Suburb",
                    province: "Texas",
                    country: "USA",
                    zipCode: "67890",
                    addressType: AddressType.Home
                );
                
                var bobWorkAddress = new Address(
                    fullName: "Bob Smith",
                    phoneNumber: "+0987654321",
                    street: "789 Business Blvd",
                    district: "Business District",
                    province: "Texas",
                    country: "USA",
                    zipCode: "67891",
                    addressType: AddressType.Work
                );

                // Set foreign keys manually
                context.Entry(bobHomeAddress).Property("UserId").CurrentValue = bob.Id;
                context.Entry(bobWorkAddress).Property("UserId").CurrentValue = bob.Id;
                context.Addresses.AddRange(bobHomeAddress, bobWorkAddress);

                // Set home address as default using reflection for seeding purposes
                var isDefaultProperty = typeof(Address).GetProperty("IsDefault");
                isDefaultProperty?.SetValue(bobHomeAddress, true);

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
                
                context.SaveChanges();
                Log.Debug("bob created with sample addresses");
            }
            else
            {
                Log.Debug("bob already exists");
            }
        }
    }
}
