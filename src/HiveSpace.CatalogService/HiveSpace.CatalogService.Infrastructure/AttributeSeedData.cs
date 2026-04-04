using HiveSpace.CatalogService.Domain.Aggregates.AttributeAggregate;
using HiveSpace.CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HiveSpace.CatalogService.Infrastructure;

public static class AttributeSeedData
{
    public static async Task SeedAsync(CatalogDbContext context, CancellationToken cancellationToken = default)
    {
        var anyExists = await context.Attributes.AnyAsync(cancellationToken);
        if (anyExists)
        {
            Log.Debug("Attributes already seeded. Skipping.");
            return;
        }

        // Each AttributeDefinition needs its own AttributeType instance (EF owned-type requirement)
        static AttributeType FreeText() => new(AttributeValueType.String, InputType.Textbox);

        // Groups first so we can use their IDs for leaf ParentId
        var warrantyGroup = new AttributeDefinition("Bảo hành",           FreeText());
        var specsGroup    = new AttributeDefinition("Thông tin chi tiết",  FreeText());

        await context.Attributes.AddRangeAsync([warrantyGroup, specsGroup], cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // Leaf attributes — Bảo hành
        var warrantyAttrs = new List<AttributeDefinition>
        {
            new("Thời gian bảo hành", FreeText(), parentId: warrantyGroup.Id),
            new("Hình thức bảo hành", FreeText(), parentId: warrantyGroup.Id),
            new("Nơi bảo hành",       FreeText(), parentId: warrantyGroup.Id),
        };

        // Leaf attributes — Thông tin chi tiết
        var specsAttrs = new List<AttributeDefinition>
        {
            new("Thương hiệu",         FreeText(), parentId: specsGroup.Id),
            new("Xuất xứ (Made in)",   FreeText(), parentId: specsGroup.Id),
            new("Có thuế VAT",         FreeText(), parentId: specsGroup.Id),
            new("Hệ điều hành",        FreeText(), parentId: specsGroup.Id),
            new("Kích thước màn hình", FreeText(), parentId: specsGroup.Id),
            new("Dung lượng pin",      FreeText(), parentId: specsGroup.Id),
            new("Loại màn hình",       FreeText(), parentId: specsGroup.Id),
            new("Camera trước",        FreeText(), parentId: specsGroup.Id),
            new("Camera sau",          FreeText(), parentId: specsGroup.Id),
            new("Chip xử lý (CPU)",    FreeText(), parentId: specsGroup.Id),
            // Book-specific attributes
            new("Công ty phát hành",   FreeText(), parentId: specsGroup.Id),
            new("Loại bìa",            FreeText(), parentId: specsGroup.Id),
            new("Số trang",            FreeText(), parentId: specsGroup.Id),
            new("Nhà xuất bản",        FreeText(), parentId: specsGroup.Id),
            new("Ngày xuất bản",       FreeText(), parentId: specsGroup.Id),
        };

        await context.Attributes.AddRangeAsync([..warrantyAttrs, ..specsAttrs], cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        Log.Debug("Seeded {Count} attribute definitions.", 2 + warrantyAttrs.Count + specsAttrs.Count);
        // Note: specsAttrs now includes 10 mobile + 5 book attributes = 15 total
    }
}
