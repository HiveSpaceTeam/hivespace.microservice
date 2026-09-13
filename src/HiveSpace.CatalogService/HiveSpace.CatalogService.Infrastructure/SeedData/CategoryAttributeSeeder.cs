using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.CatalogService.Infrastructure.SeedData;

internal sealed class CategoryAttributeSeeder(CatalogDbContext db, ILogger<CategoryAttributeSeeder> logger) : ISeeder
{
    public int Order => 3;
    public SeedKind Kind => SeedKind.SampleData;

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
        var categoryRows = await db.Categories
            .Include(c => c.CategoryAttributes)
            .Where(c => CategoryAttributeSeeds.Keys.Contains(c.Name))
            .OrderBy(c => c.Id)
            .ToListAsync(ct);

        var duplicateCategoryNames = categoryRows
            .GroupBy(c => c.Name)
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var duplicateCategory in duplicateCategoryNames)
        {
            logger.LogWarning(
                "Found {Count} category rows named '{CategoryName}' while seeding category attributes. Using the lowest category id.",
                duplicateCategory.Count(),
                duplicateCategory.Key);
        }

        var categories = categoryRows
            .GroupBy(c => c.Name)
            .ToDictionary(g => g.Key, g => g.First());

        var attributeNames = CategoryAttributeSeeds.Values
            .SelectMany(names => names)
            .Distinct()
            .ToList();

        var attributeRows = await db.Attributes
            .Where(a => a.ParentId != null && attributeNames.Contains(a.Name))
            .OrderBy(a => a.Id)
            .ToListAsync(ct);

        var duplicateAttributeNames = attributeRows
            .GroupBy(a => a.Name)
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var duplicateAttribute in duplicateAttributeNames)
        {
            logger.LogWarning(
                "Found {Count} attribute rows named '{AttributeName}' while seeding category attributes. Using the lowest attribute id.",
                duplicateAttribute.Count(),
                duplicateAttribute.Key);
        }

        var attributes = attributeRows
            .GroupBy(a => a.Name)
            .ToDictionary(g => g.Key, g => g.First().Id);

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
