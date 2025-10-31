using Duende.IdentityModel;
using HiveSpace.UserService.Infrastructure.Data;
using HiveSpace.UserService.Infrastructure.Identity;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Services;
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
            
            // Check for pending migrations before applying them
            var pendingMigrations = context.Database.GetPendingMigrations();
            if (pendingMigrations.Any())
            {
                Log.Information("Found {Count} pending migrations: {Migrations}", 
                    pendingMigrations.Count(), 
                    string.Join(", ", pendingMigrations));
                
                Log.Information("Applying pending migrations...");
                context.Database.Migrate();
                Log.Information("Migrations applied successfully");
            }
            else
            {
                Log.Information("No pending migrations found. Database is up to date.");
            }

            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var storeManager = scope.ServiceProvider.GetRequiredService<StoreManager>();

            // Seed Alice
            var alice = userMgr.FindByNameAsync("alice").Result;
            if (alice == null)
            {
                alice = new ApplicationUser
                {
                    UserName = "alice",
                    Email = "AliceSmith@example.com",
                    EmailConfirmed = false,
                    FullName = "Alice Smith",
                    PhoneNumber = "+1234567890",
                    DateOfBirth = new DateTime(1990, 1, 15),
                    Gender = (int)Gender.Female,
                    Status = (int)UserStatus.Active,
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
                    EmailConfirmed = false,
                    FullName = "Bob Smith",
                    PhoneNumber = "+0987654321",
                    DateOfBirth = new DateTime(1985, 6, 20),
                    Gender = (int)Gender.Male,
                    Status = (int)UserStatus.Active,
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

            // Seed System Admin
            var systemAdmin = userMgr.FindByNameAsync("sysadmin").Result;
            if (systemAdmin == null)
            {
                systemAdmin = new ApplicationUser
                {
                    UserName = "sysadmin",
                    Email = "sysadmin@hivespace.com",
                    EmailConfirmed = true,
                    FullName = "System Administrator",
                    PhoneNumber = "+1111111111",
                    DateOfBirth = new DateTime(1980, 3, 10),
                    Gender = (int)Gender.Male,
                    Status = (int)UserStatus.Active,
                    RoleName = "SystemAdmin", // Set role directly
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var result = userMgr.CreateAsync(systemAdmin, "SysAdmin123$").Result;
                if (!result.Succeeded)
                {
                    Log.Error("Failed to create System Admin: {Error}", result.Errors.First().Description);
                    throw new Exception(result.Errors.First().Description);
                }

                // Add claims
                result = userMgr.AddClaimsAsync(systemAdmin, new[]
                {
                    new Claim(JwtClaimTypes.Name, "System Administrator"),
                    new Claim(JwtClaimTypes.GivenName, "System"),
                    new Claim(JwtClaimTypes.FamilyName, "Administrator"),
                    new Claim(JwtClaimTypes.Role, "SystemAdmin"),
                    new Claim("permissions", "system.manage"),
                    new Claim("permissions", "user.manage"),
                    new Claim("permissions", "store.manage")
                }).Result;

                Log.Debug("sysadmin created");
            }
            else
            {
                Log.Debug("sysadmin already exists");
            }

            // Seed Admin
            var admin = userMgr.FindByNameAsync("admin").Result;
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@hivespace.com",
                    EmailConfirmed = true,
                    FullName = "Admin User",
                    PhoneNumber = "+2222222222",
                    DateOfBirth = new DateTime(1985, 8, 22),
                    Gender = (int)Gender.Female,
                    Status = (int)UserStatus.Active,
                    RoleName = "Admin", // Set role directly
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var result = userMgr.CreateAsync(admin, "Admin123$").Result;
                if (!result.Succeeded)
                {
                    Log.Error("Failed to create Admin: {Error}", result.Errors.First().Description);
                    throw new Exception(result.Errors.First().Description);
                }

                // Add sample address
                var adminAddress = new Address(
                    fullName: "Admin User",
                    phoneNumber: "+2222222222",
                    street: "100 Admin Plaza",
                    district: "Central",
                    province: "New York",
                    country: "USA",
                    zipCode: "10001",
                    addressType: AddressType.Work
                );
                
                context.Entry(adminAddress).Property("UserId").CurrentValue = admin.Id;
                context.Addresses.Add(adminAddress);

                var isDefaultProperty = typeof(Address).GetProperty("IsDefault");
                isDefaultProperty?.SetValue(adminAddress, true);

                // Add claims
                result = userMgr.AddClaimsAsync(admin, new[]
                {
                    new Claim(JwtClaimTypes.Name, "Admin User"),
                    new Claim(JwtClaimTypes.GivenName, "Admin"),
                    new Claim(JwtClaimTypes.FamilyName, "User"),
                    new Claim(JwtClaimTypes.Role, "Admin"),
                    new Claim("permissions", "user.manage"),
                    new Claim("permissions", "store.view")
                }).Result;

                context.SaveChanges();
                Log.Debug("admin created with sample address");
            }
            else
            {
                Log.Debug("admin already exists");
            }

            // Seed Seller
            var seller = userMgr.FindByNameAsync("seller").Result;
            if (seller == null)
            {
                // Create seller user first to get the ID
                seller = new ApplicationUser
                {
                    UserName = "seller",
                    Email = "seller@example.com",
                    EmailConfirmed = false,
                    FullName = "John Seller",
                    PhoneNumber = "+3333333333",
                    DateOfBirth = new DateTime(1988, 12, 5),
                    Gender = (int)Gender.Male,
                    Status = (int)UserStatus.Active,
                    RoleName = "Seller", // Set role directly
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var sellerResult = userMgr.CreateAsync(seller, "Seller123$").Result;
                if (!sellerResult.Succeeded)
                {
                    Log.Error("Failed to create Seller: {Error}", sellerResult.Errors.First().Description);
                    throw new Exception(sellerResult.Errors.First().Description);
                }

                try
                {
                    var sampleStore = storeManager.RegisterStoreAsync(
                        name: "John's Electronics Store",
                        description: "Quality electronics and gadgets",
                        logoUrl: "http://example.com/logos/johns-electronics.png",
                        storeAddress: "555 Commerce Street, Market District, Florida, USA, 33101",
                        ownerId: seller.Id
                    ).Result;
                    
                    // Set the store ID on the seller
                    seller.StoreId = sampleStore.Id;
                    var updateResult = userMgr.UpdateAsync(seller).Result;
                    if (!updateResult.Succeeded)
                    {
                        Log.Error("Failed to update Seller with StoreId: {Error}", updateResult.Errors.First().Description);
                    }
                    
                    context.Stores.Add(sampleStore);
                    context.SaveChanges();
                    Log.Debug("Store created successfully for seller");
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to create store for seller: {Error}", ex.Message);
                }

                // Role is now set directly in the ApplicationUser creation above

                // Add sample addresses
                var sellerHomeAddress = new Address(
                    fullName: "John Seller",
                    phoneNumber: "+3333333333",
                    street: "555 Commerce Street",
                    district: "Market District",
                    province: "Florida",
                    country: "USA",
                    zipCode: "33101",
                    addressType: AddressType.Home
                );

                var sellerBusinessAddress = new Address(
                    fullName: "John Seller",
                    phoneNumber: "+3333333333",
                    street: "777 Business Center",
                    district: "Commercial Zone",
                    province: "Florida",
                    country: "USA",
                    zipCode: "33102",
                    addressType: AddressType.Work
                );

                context.Entry(sellerHomeAddress).Property("UserId").CurrentValue = seller.Id;
                context.Entry(sellerBusinessAddress).Property("UserId").CurrentValue = seller.Id;
                context.Addresses.AddRange(sellerHomeAddress, sellerBusinessAddress);

                var isDefaultProperty = typeof(Address).GetProperty("IsDefault");
                isDefaultProperty?.SetValue(sellerHomeAddress, true);

                // Add claims
                sellerResult = userMgr.AddClaimsAsync(seller, new[]
                {
                    new Claim(JwtClaimTypes.Name, "John Seller"),
                    new Claim(JwtClaimTypes.GivenName, "John"),
                    new Claim(JwtClaimTypes.FamilyName, "Seller"),
                    new Claim(JwtClaimTypes.Role, "Seller"),
                    new Claim("permissions", "store.manage"),
                    new Claim("permissions", "product.manage"),
                    new Claim("permissions", "order.view")
                }).Result;

                context.SaveChanges();
                Log.Debug("seller created with sample addresses");
            }
            else
            {
                Log.Debug("seller already exists");
            }
        }
    }
}
