using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Infrastructure.SeedData;

internal sealed class AttributeSeeder(CatalogDbContext db, ILogger<AttributeSeeder> logger) : ISeeder
{
    public int Order => 2;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var anyExists = await db.Attributes.AnyAsync(ct);
        if (anyExists)
        {
            logger.LogDebug("Attributes already seeded. Skipping.");
            return;
        }

        static AttributeType FreeText() => new(AttributeValueType.String, InputType.Textbox);

        var warrantyGroup    = new AttributeDefinition("Bảo hành",            FreeText());
        var bookSpecsGroup   = new AttributeDefinition("Thông tin sách",      FreeText());
        var deviceSpecsGroup = new AttributeDefinition("Thông tin thiết bị",  FreeText());

        const int seededCount = 21;

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            await db.Attributes.AddRangeAsync([warrantyGroup, bookSpecsGroup, deviceSpecsGroup], ct);
            await db.SaveChangesAsync(ct);

        var warrantyAttrs = new List<AttributeDefinition>
        {
            new("Thời gian bảo hành", FreeText(), parentId: warrantyGroup.Id),
            new("Hình thức bảo hành", FreeText(), parentId: warrantyGroup.Id),
            new("Nơi bảo hành",       FreeText(), parentId: warrantyGroup.Id),
        };

        var bookAttrs = new List<AttributeDefinition>
        {
            new("Công ty phát hành", FreeText(), parentId: bookSpecsGroup.Id),
            new("Loại bìa",          FreeText(), parentId: bookSpecsGroup.Id),
            new("Số trang",          FreeText(), parentId: bookSpecsGroup.Id),
            new("Nhà xuất bản",      FreeText(), parentId: bookSpecsGroup.Id),
            new("Ngày xuất bản",     FreeText(), parentId: bookSpecsGroup.Id),
        };

        var deviceAttrs = new List<AttributeDefinition>
        {
            new("Thương hiệu",         FreeText(), parentId: deviceSpecsGroup.Id),
            new("Xuất xứ (Made in)",   FreeText(), parentId: deviceSpecsGroup.Id),
            new("Có thuế VAT",         FreeText(), parentId: deviceSpecsGroup.Id),
            new("Hệ điều hành",        FreeText(), parentId: deviceSpecsGroup.Id),
            new("Kích thước màn hình", FreeText(), parentId: deviceSpecsGroup.Id),
            new("Dung lượng pin",      FreeText(), parentId: deviceSpecsGroup.Id),
            new("Loại màn hình",       FreeText(), parentId: deviceSpecsGroup.Id),
            new("Camera trước",        FreeText(), parentId: deviceSpecsGroup.Id),
            new("Camera sau",          FreeText(), parentId: deviceSpecsGroup.Id),
            new("Chip xử lý (CPU)",    FreeText(), parentId: deviceSpecsGroup.Id),
        };

            await db.Attributes.AddRangeAsync([..warrantyAttrs, ..bookAttrs, ..deviceAttrs], ct);
            await db.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);
        });
        logger.LogInformation("Seeded {Count} attribute definitions.", seededCount);
    }
}
