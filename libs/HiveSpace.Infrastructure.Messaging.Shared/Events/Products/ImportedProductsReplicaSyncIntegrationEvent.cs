using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Shared.Events.Products;

public record ImportedProductsReplicaSyncIntegrationEvent(
    Guid BundleId,
    string SourceSystem,
    string SourceFingerprint,
    DateTimeOffset ImportedAt,
    IReadOnlyCollection<ImportedProductReplicaSyncItem> Products
) : IntegrationEvent;

public record ImportedProductReplicaSyncItem(
    long ProductId,
    Guid StoreId,
    string Name,
    string? ThumbnailUrl,
    ProductStatus Status,
    IReadOnlyCollection<ImportedSkuReplicaSyncItem> Skus
);

public record ImportedSkuReplicaSyncItem(
    long SkuId,
    long ProductId,
    string SkuNo,
    string SkuName,
    long Price,
    string Currency,
    string? ImageUrl
);
