# Media Asset Patterns

Every image-bearing entity stores two fields: a stable `*FileId` (the MediaAsset GUID written at creation) and a nullable `*Url` (the CDN URL resolved after processing). Never store just a URL, and never expose a FileId as a URL.

## Dual-field rule

| Field | Type | Nullable | Set by | Cleared by |
|-------|------|----------|--------|------------|
| `*FileId` | `string` | Required entities: non-null. Optional: nullable | Upload request | Never |
| `*Url` | `string?` | Always nullable | `MediaAssetProcessedConsumer` | Domain method when FileId changes |

When a new FileId is stored (e.g. `store.UpdateLogo(newFileId)`), clear the old Url (`LogoUrl = null`) so the stale CDN link is never served.

## Value objects with images (CatalogService)

`ProductImage` and `SkuImage` are value objects — they cannot mutate in place. Use the `WithImageUrl(url)` factory method to produce a new instance, then call the aggregate method that replaces the item in the EF `OwnsMany` collection:

```csharp
product.UpdateProductImageUrl(fileId, publicUrl);
sku.UpdateSkuImageUrl(fileId, publicUrl);
```

Equality is defined on `FileId` only (not `ImageUrl`), so replacing the URL does not affect deduplication.

## Domain methods reference

| Entity | FileId field | Url field | Set-url method |
|--------|-------------|-----------|----------------|
| `Product` | `ThumbnailFileId` | `ThumbnailUrl` | `SetThumbnailUrl(url)` |
| `ProductImage` (VO) | `FileId` | `ImageUrl` | `WithImageUrl(url)` via `Product.UpdateProductImageUrl` |
| `SkuImage` (VO) | `FileId` | `ImageUrl` | `WithImageUrl(url)` via `Sku.UpdateSkuImageUrl` |
| `Category` | `ImageFileId` | `ImageUrl` | `SetImageUrl(url)` |
| `User` | `AvatarFileId` | `AvatarUrl` | `SetAvatarUrl(url)` |
| `Store` | `LogoFileId` | `LogoUrl` | `SetLogoUrl(url)` |

## MediaAssetProcessedIntegrationEvent

Published by `ImageProcessingFunction` (MediaService.Func) immediately after `SaveChanges`. Contract:

```csharp
public record MediaAssetProcessedIntegrationEvent(
    Guid FileId,
    string EntityType,   // see table below
    string? EntityId,
    string PublicUrl,
    string? ThumbnailUrl
) : IntegrationEvent;
```

Location: `libs/HiveSpace.Infrastructure.Messaging.Shared/Events/Media/MediaAssetProcessedIntegrationEvent.cs`

### EntityType values

| EntityType | Consumer service | Entity updated |
|------------|-----------------|----------------|
| `product_thumbnail` | CatalogService | `Product.ThumbnailUrl` |
| `product_image` | CatalogService | `ProductImage.ImageUrl` |
| `sku_image` | CatalogService | `SkuImage.ImageUrl` + re-publishes `ProductSkuUpdatedIntegrationEvent` |
| `category` | CatalogService | `Category.ImageUrl` |
| `user_avatar` | UserService | `ApplicationUser.AvatarUrl` |
| `store_logo` | UserService | `Store.LogoUrl` |

## Consumer pattern

Both `MediaAssetProcessedConsumer` classes (one per consuming service) follow the same structure:

```csharp
public async Task Consume(ConsumeContext<MediaAssetProcessedIntegrationEvent> context)
{
    var msg = context.Message;
    var fileIdStr = msg.FileId.ToString();

    switch (msg.EntityType)
    {
        case "store_logo":
            var store = await db.Stores.FirstOrDefaultAsync(s => s.LogoFileId == fileIdStr, ct);
            if (store is null) { logger.LogWarning(...); return; }  // warn, do not throw
            store.SetLogoUrl(msg.PublicUrl);
            await db.SaveChangesAsync(ct);
            break;
        // ...
    }
}
```

Consumer locations:
- `src/HiveSpace.CatalogService/HiveSpace.CatalogService.Api/Consumers/Sync/MediaAssetProcessedConsumer.cs`
- `src/HiveSpace.UserService/HiveSpace.UserService.Api/Consumers/Sync/MediaAssetProcessedConsumer.cs`

Key rules:
- **Warn, never throw** when the entity is not found — the event may arrive out of order or reference a deleted entity.
- **No transaction needed** — each `SaveChanges` is a single-row URL update.
- After updating a `sku_image`, **re-publish `ProductSkuUpdatedIntegrationEvent`** so OrderService syncs `SkuRef.ImageUrl`.

## EF configuration

```csharp
// Scalar entity (Product, Category, Store, User)
builder.Property(x => x.ThumbnailFileId).HasColumnName("thumbnail_file_id").HasMaxLength(100).IsRequired(false);
builder.Property(x => x.ThumbnailUrl).HasMaxLength(500).IsRequired(false);

// OwnsMany value object (ProductImage, SkuImage)
ownsManyBuilder.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired(false);
```

Column naming convention: FileId fields use `snake_case` (`image_file_id`, `logo_file_id`, `thumbnail_file_id`). Url fields follow the EF default (PascalCase → `ThumbnailUrl`, `ImageUrl`) unless an explicit `HasColumnName` is applied.
