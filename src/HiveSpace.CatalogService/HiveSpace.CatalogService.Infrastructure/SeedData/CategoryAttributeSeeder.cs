using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Infrastructure.SeedData;

internal sealed class CategoryAttributeSeeder(CatalogDbContext db, ILogger<CategoryAttributeSeeder> logger) : ISeeder
{
    public int Order => 3;

    private static readonly IReadOnlyDictionary<string, string[]> CategoryAttributeSeeds =
        new Dictionary<string, string[]>
        {
            ["Nhà Sách Tiki"] =
            [
                "Công ty phát hành",
                "Loại bìa",
                "Số trang",
                "Nhà xuất bản",
                "Ngày xuất bản",
            ],
            ["Nhà Cửa - Đời Sống"] =
            [
                "Thương hiệu",
                "Xuất xứ (Made in)",
            ],
            ["Điện Thoại - Máy Tính Bảng"] =
            [
                "Thời gian bảo hành",
                "Hình thức bảo hành",
                "Nơi bảo hành",
                "Thương hiệu",
                "Xuất xứ (Made in)",
                "Có thuế VAT",
                "Hệ điều hành",
                "Kích thước màn hình",
                "Dung lượng pin",
                "Loại màn hình",
                "Camera trước",
                "Camera sau",
                "Chip xử lý (CPU)",
            ],
        };

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var categories = await db.Categories
            .Include(c => c.CategoryAttributes)
            .Where(c => CategoryAttributeSeeds.Keys.Contains(c.Name))
            .ToDictionaryAsync(c => c.Name, ct);

        var attributeNames = CategoryAttributeSeeds.Values
            .SelectMany(names => names)
            .Distinct()
            .ToList();

        var attributes = await db.Attributes
            .Where(a => a.ParentId != null && attributeNames.Contains(a.Name))
            .ToDictionaryAsync(a => a.Name, a => a.Id, ct);

        var addedCount = 0;

        foreach (var (categoryName, seededAttributeNames) in CategoryAttributeSeeds)
        {
            if (!categories.TryGetValue(categoryName, out var category))
            {
                logger.LogWarning("Category '{CategoryName}' not found. Skipping category attribute links.", categoryName);
                continue;
            }

            var existingAttributeIds = category.CategoryAttributes
                .Select(ca => ca.AttributeId)
                .ToHashSet();

            foreach (var attributeName in seededAttributeNames)
            {
                if (!attributes.TryGetValue(attributeName, out var attributeId))
                {
                    logger.LogWarning(
                        "Attribute '{AttributeName}' not found. Skipping link for category '{CategoryName}'.",
                        attributeName,
                        categoryName);
                    continue;
                }

                if (existingAttributeIds.Add(attributeId))
                {
                    category.AddAttribute(attributeId);
                    addedCount++;
                }
            }
        }

        if (addedCount == 0)
        {
            logger.LogDebug("Category attributes already seeded. Skipping.");
            return;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} category attribute links.", addedCount);
    }
}
