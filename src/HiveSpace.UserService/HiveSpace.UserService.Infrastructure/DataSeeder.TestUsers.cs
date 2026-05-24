using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.UserService.Infrastructure;

public static partial class DataSeeder
{
    private const string AliceAvatarUrl = "https://cdn.discordapp.com/avatars/474579515188707339/24acc2c6b645216504447360a58c0683.webp";
    private const string BobAvatarUrl   = "https://cdn.discordapp.com/avatars/743061397037908059/edaa33d7f618405017a26cc7ca42379b.webp";

    private static async Task SeedAliceAsync(
        UserDbContext context,
        ILogger logger, CancellationToken ct)
    {
        var exists = await context.Users.AnyAsync(u => u.Id == AliceId, ct);
        if (exists)
        {
            logger.LogDebug("alice profile already exists");
            return;
        }

        var alice = User.CreateProfile(
            id: AliceId,
            email: Email.Create("aliceSmith@gmail.com"),
            userName: "alice",
            fullName: "Alice Smith",
            avatarUrl: AliceAvatarUrl,
            phoneNumber: PhoneNumber.CreateOrDefault("+84901234567"),
            dateOfBirth: DateOfBirth.CreateOrDefault(new DateTime(1990, 1, 15)),
            gender: Gender.Female,
            createdAt: DateTimeOffset.UtcNow
        );
        alice.UpdateTheme(Theme.Light);
        alice.UpdateCulture(Culture.Vi);

        alice.AddAddress(
            fullName: "Alice Smith", phoneNumber: "+84901234567",
            street: "123 Main Street", commune: "Downtown",
            province: "California", country: "USA",
            zipCode: "12345", addressType: AddressType.Home,
            setAsDefault: true);

        context.Users.Add(alice);
        await context.SaveChangesAsync(ct);
        logger.LogDebug("alice profile created with sample address");
    }

    private static async Task SeedBobAsync(
        UserDbContext context,
        ILogger logger, CancellationToken ct)
    {
        var exists = await context.Users.AnyAsync(u => u.Id == BobId, ct);
        if (exists)
        {
            logger.LogDebug("bob profile already exists");
            return;
        }

        var bob = User.CreateProfile(
            id: BobId,
            email: Email.Create("bobSmith@gmail.com"),
            userName: "bob",
            fullName: "Bob Smith",
            avatarUrl: BobAvatarUrl,
            phoneNumber: PhoneNumber.CreateOrDefault("+84987654321"),
            dateOfBirth: DateOfBirth.CreateOrDefault(new DateTime(1985, 6, 20)),
            gender: Gender.Male,
            createdAt: DateTimeOffset.UtcNow
        );
        bob.UpdateTheme(Theme.Light);
        bob.UpdateCulture(Culture.Vi);

        bob.AddAddress(
            fullName: "Bob Smith", phoneNumber: "+84987654321",
            street: "456 Oak Avenue", commune: "Suburb",
            province: "Texas", country: "USA",
            zipCode: "67890", addressType: AddressType.Home,
            setAsDefault: true);

        bob.AddAddress(
            fullName: "Bob Smith", phoneNumber: "+84987654321",
            street: "789 Business Blvd", commune: "Business commune",
            province: "Texas", country: "USA",
            zipCode: "67891", addressType: AddressType.Work);

        context.Users.Add(bob);
        await context.SaveChangesAsync(ct);
        logger.LogDebug("bob profile created with sample addresses");
    }
}
