using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.PaymentService.Domain.Aggregates.External;
using HiveSpace.PaymentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.PaymentService.Infrastructure.SeedData;

internal sealed class PlatformCurrencyPolicyRefSeeder(
    PaymentDbContext db,
    ILogger<PlatformCurrencyPolicyRefSeeder> logger) : ISeeder
{
    public int Order => 0;

    private static readonly Guid FallbackPolicyId = new("00000000-0000-0000-0000-000000000001");

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await db.PlatformCurrencyPolicyRefs.AnyAsync(ct))
        {
            logger.LogDebug("Platform currency policy ref already seeded. Skipping.");
            return;
        }

        db.PlatformCurrencyPolicyRefs.Add(new PlatformCurrencyPolicyRef(
            FallbackPolicyId,
            "VND",
            0,
            DateTimeOffset.UtcNow,
            ["VND"]));

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded fallback platform currency policy ref.");
    }
}
