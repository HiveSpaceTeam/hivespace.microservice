using HiveSpace.CatalogService.Application.CatalogImports.Dtos;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports;

internal static class SubmitCatalogImportBundleCommandHandlerTestsHelper
{
    public static CatalogImportBundleRequestDto CreateRequest(string sourceFingerprint)
        => new(
            "2026-07-24",
            new CatalogImportSourceDto("tiki", "category", "1846", "https://tiki.vn/1846"),
            new CatalogImportCrawlDto(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow, sourceFingerprint, null),
            [new ImportedSellerRequestDto("seller-1", "Tiki Trading", "tiki-trading", "https://tiki.vn/cua-hang/tiki-trading", null, "https://cdn.example.com/sellers/tiki-trading.png")],
            [],
            [
                new ImportedProductRequestDto(
                    "product-1",
                    "seller-1",
                    ["1846"],
                    "https://tiki.vn/product-1",
                    "Imported Book",
                    null,
                    "https://cdn.tiki.vn/book.jpg",
                    [new ImportedAttributeRequestDto("Brand", "Tiki", null, null)],
                    [new ImportedImageRequestDto("https://cdn.tiki.vn/book.jpg", "Thumbnail", null)],
                    [],
                    [new ImportedSkuRequestDto("sku-1", "TIKI-SKU-1", new Dictionary<string, string>(), new ImportedPriceRequestDto(125000, "VND", "125000"), 12, [])])
            ],
            [new ImportValidationHintRequestDto("Product", "product-1", "description", "Warning", "MissingOptionalDescription", "Description was not available.")]);
}
