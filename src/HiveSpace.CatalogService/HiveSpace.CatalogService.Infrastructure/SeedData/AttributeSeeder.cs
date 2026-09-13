using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Infrastructure.SeedData;

internal sealed class AttributeSeeder(CatalogDbContext db, ILogger<AttributeSeeder> logger) : ISeeder
{
    public int Order => 2;
    public SeedKind Kind => SeedKind.SampleData;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var anyExists = await db.Attributes.AnyAsync(ct);
        if (anyExists)
        {
            logger.LogDebug("Attributes already seeded. Skipping.");
            return;
        }

        static AttributeType FreeText() => new(AttributeValueType.String, InputType.Textbox);

        var warrantyGroup    = new AttributeDefinition("Báº£o hÃ nh",            FreeText());
        var bookSpecsGroup   = new AttributeDefinition("ThÃ´ng tin sÃ¡ch",      FreeText());
        var deviceSpecsGroup = new AttributeDefinition("ThÃ´ng tin thiáº¿t bá»‹",  FreeText());

        const int seededCount = 21;

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            await db.Attributes.AddRangeAsync([warrantyGroup, bookSpecsGroup, deviceSpecsGroup], ct);
            await db.SaveChangesAsync(ct);

        var warrantyAttrs = new List<AttributeDefinition>
        {
            new("Thá»i gian báº£o hÃ nh", FreeText(), parentId: warrantyGroup.Id),
            new("HÃ¬nh thá»©c báº£o hÃ nh", FreeText(), parentId: warrantyGroup.Id),
            new("NÆ¡i báº£o hÃ nh",       FreeText(), parentId: warrantyGroup.Id),
        };

        var bookAttrs = new List<AttributeDefinition>
        {
            new("CÃ´ng ty phÃ¡t hÃ nh", FreeText(), parentId: bookSpecsGroup.Id),
            new("Loáº¡i bÃ¬a",          FreeText(), parentId: bookSpecsGroup.Id),
            new("Sá»‘ trang",          FreeText(), parentId: bookSpecsGroup.Id),
            new("NhÃ  xuáº¥t báº£n",      FreeText(), parentId: bookSpecsGroup.Id),
            new("NgÃ y xuáº¥t báº£n",     FreeText(), parentId: bookSpecsGroup.Id),
        };

        var deviceAttrs = new List<AttributeDefinition>
        {
            new("ThÆ°Æ¡ng hiá»‡u",         FreeText(), parentId: deviceSpecsGroup.Id),
            new("Xuáº¥t xá»© (Made in)",   FreeText(), parentId: deviceSpecsGroup.Id),
            new("CÃ³ thuáº¿ VAT",         FreeText(), parentId: deviceSpecsGroup.Id),
            new("Há»‡ Ä‘iá»u hÃ nh",        FreeText(), parentId: deviceSpecsGroup.Id),
            new("KÃ­ch thÆ°á»›c mÃ n hÃ¬nh", FreeText(), parentId: deviceSpecsGroup.Id),
            new("Dung lÆ°á»£ng pin",      FreeText(), parentId: deviceSpecsGroup.Id),
            new("Loáº¡i mÃ n hÃ¬nh",       FreeText(), parentId: deviceSpecsGroup.Id),
            new("Camera trÆ°á»›c",        FreeText(), parentId: deviceSpecsGroup.Id),
            new("Camera sau",          FreeText(), parentId: deviceSpecsGroup.Id),
            new("Chip xá»­ lÃ½ (CPU)",    FreeText(), parentId: deviceSpecsGroup.Id),
        };

            await db.Attributes.AddRangeAsync([..warrantyAttrs, ..bookAttrs, ..deviceAttrs], ct);
            await db.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);
        });
        logger.LogInformation("Seeded {Count} attribute definitions.", seededCount);
    }
}
