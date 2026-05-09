using Duende.IdentityModel;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Infrastructure.Data;
using HiveSpace.UserService.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace HiveSpace.UserService.Infrastructure;

public static partial class DataSeeder
{
    private const string AliceAvatarUrl = "https://cdn.discordapp.com/avatars/474579515188707339/24acc2c6b645216504447360a58c0683.webp";
    private const string BobAvatarUrl   = "https://cdn.discordapp.com/avatars/743061397037908059/edaa33d7f618405017a26cc7ca42379b.webp";

    private static async Task SeedAliceAsync(
        UserManager<ApplicationUser> userMgr, UserDbContext context,
        ILogger logger, CancellationToken ct)
    {
        var alice = await userMgr.FindByNameAsync("alice");
        if (alice != null)
        {
            if (await EnsureAvatarUrlAsync(userMgr, alice, AliceAvatarUrl, logger))
                await context.SaveChangesAsync(ct);

            logger.LogDebug("alice already exists");
            return;
        }

        alice = new ApplicationUser
        {
            Id             = AliceId,
            UserName       = "alice",
            Email          = "aliceSmith@gmail.com",
            EmailConfirmed = true,
            FullName       = "Alice Smith",
            AvatarFileId   = Guid.NewGuid().ToString(),
            AvatarUrl      = AliceAvatarUrl,
            PhoneNumber    = "+84901234567",
            DateOfBirth    = new DateTime(1990, 1, 15),
            Gender         = (int)Gender.Female,
            Status         = (int)UserStatus.Active,
            CreatedAt      = DateTimeOffset.UtcNow,
            Theme          = Theme.Light,
            Culture        = Culture.Vi
        };

        var result = await userMgr.CreateAsync(alice, "AliceSmith123$");
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create alice: {Error}", result.Errors.First().Description);
            throw new InvalidFieldException(UserDomainErrorCode.UserCreationFailed, nameof(ApplicationUser));
        }

        var aliceHomeAddress = new Address(
            fullName: "Alice Smith", phoneNumber: "+84901234567",
            street: "123 Main Street", commune: "Downtown",
            province: "California", country: "USA",
            zipCode: "12345", addressType: AddressType.Home);

        aliceHomeAddress.SetAsDefault();
        context.Entry(aliceHomeAddress).Property("UserId").CurrentValue = alice.Id;
        context.Addresses.Add(aliceHomeAddress);

        result = await userMgr.AddClaimsAsync(alice,
        [
            new Claim(JwtClaimTypes.Name,       "Alice Smith"),
            new Claim(JwtClaimTypes.GivenName,  "Alice"),
            new Claim(JwtClaimTypes.FamilyName, "Smith"),
            new Claim(JwtClaimTypes.WebSite,    "http://alice.example.com"),
            new Claim(JwtClaimTypes.Picture,    AliceAvatarUrl),
        ]);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to add claims to alice: {Error}", result.Errors.First().Description);
            throw new InvalidFieldException(UserDomainErrorCode.UserCreationFailed, nameof(ApplicationUser));
        }

        await context.SaveChangesAsync(ct);
        logger.LogDebug("alice created with sample address");
    }

    private static async Task SeedBobAsync(
        UserManager<ApplicationUser> userMgr, UserDbContext context,
        ILogger logger, CancellationToken ct)
    {
        var bob = await userMgr.FindByNameAsync("bob");
        if (bob != null)
        {
            if (await EnsureAvatarUrlAsync(userMgr, bob, BobAvatarUrl, logger))
                await context.SaveChangesAsync(ct);

            logger.LogDebug("bob already exists");
            return;
        }

        bob = new ApplicationUser
        {
            Id             = BobId,
            UserName       = "bob",
            Email          = "bobSmith@gmail.com",
            EmailConfirmed = false,
            FullName       = "Bob Smith",
            AvatarFileId   = Guid.NewGuid().ToString(),
            AvatarUrl      = BobAvatarUrl,
            PhoneNumber    = "+84987654321",
            DateOfBirth    = new DateTime(1985, 6, 20),
            Gender         = (int)Gender.Male,
            Status         = (int)UserStatus.Active,
            CreatedAt      = DateTimeOffset.UtcNow,
            Theme          = Theme.Light,
            Culture        = Culture.Vi
        };

        var result = await userMgr.CreateAsync(bob, "BobSmith123$");
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create bob: {Error}", result.Errors.First().Description);
            throw new InvalidFieldException(UserDomainErrorCode.UserCreationFailed, nameof(ApplicationUser));
        }

        var bobHomeAddress = new Address(
            fullName: "Bob Smith", phoneNumber: "+84987654321",
            street: "456 Oak Avenue", commune: "Suburb",
            province: "Texas", country: "USA",
            zipCode: "67890", addressType: AddressType.Home);

        var bobWorkAddress = new Address(
            fullName: "Bob Smith", phoneNumber: "+84987654321",
            street: "789 Business Blvd", commune: "Business commune",
            province: "Texas", country: "USA",
            zipCode: "67891", addressType: AddressType.Work);

        bobHomeAddress.SetAsDefault();
        context.Entry(bobHomeAddress).Property("UserId").CurrentValue = bob.Id;
        context.Entry(bobWorkAddress).Property("UserId").CurrentValue = bob.Id;
        context.Addresses.AddRange(bobHomeAddress, bobWorkAddress);

        result = await userMgr.AddClaimsAsync(bob,
        [
            new Claim(JwtClaimTypes.Name,       "Bob Smith"),
            new Claim(JwtClaimTypes.GivenName,  "Bob"),
            new Claim(JwtClaimTypes.FamilyName, "Smith"),
            new Claim(JwtClaimTypes.WebSite,    "http://bob.example.com"),
            new Claim("location",               "somewhere"),
            new Claim(JwtClaimTypes.Picture,    BobAvatarUrl),
        ]);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create bob: {Error}", result.Errors.First().Description);
            throw new InvalidFieldException(UserDomainErrorCode.UserCreationFailed, nameof(ApplicationUser));
        }

        await context.SaveChangesAsync(ct);
        logger.LogDebug("bob created with sample addresses");
    }
}
