using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Application.Interfaces.Repositories;
using HiveSpace.CatalogService.Application.Models.Dtos.Crud;
using HiveSpace.CatalogService.Application.Models.Dtos.Request.Product;
using HiveSpace.CatalogService.Application.Models.Requests;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Common;
using HiveSpace.CatalogService.Domain.Common.Enums;

namespace HiveSpace.CatalogService.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Guid> SaveProductAsync(ProductUpsertRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var productId = Guid.NewGuid();

            var categories = new List<ProductCategory>
            {
                new ProductCategory(productId, request.Category)
            };

            var variants = request.Variants?.Select(v =>
                new ProductVariant(v.Name, v.Options?.Select(o => new ProductVariantOption(v.Id, o.OptionId, o.Value ?? string.Empty)).ToList() ?? new List<ProductVariantOption>())
            ).ToList() ?? new List<ProductVariant>();

            var skus = request.Skus?.Select(s =>
            {
                var skuId = Guid.Empty;
                var parsed = decimal.TryParse(s.Price, out var amount) ? amount : 0m;
                var money = new Money(parsed, Currency.VND);
                var skuVariants = s.SkuVariants?.Select(sv => new SkuVariant(skuId, sv.VariantId, sv.OptionId, sv.Value ?? string.Empty)).ToList() ?? new List<SkuVariant>();
                return new Sku(s.SkuNo ?? string.Empty, productId, skuVariants, new List<SkuImage>(), int.TryParse(s.Quantity, out var q) ? q : 0, true, money);

            }).ToList() ?? new List<Sku>();

            var attributes = request.Attributes?.Select(a => new ProductAttribute(a.AttributeId, productId, a.SelectedValueIds, a.FreeTextValue)).ToList() ?? new List<ProductAttribute>();

            var product = new Product(
                productId,
                request.Name ?? string.Empty,
                request.Description ?? string.Empty,
                ProductStatus.Available,
                categories,
                attributes,
                new List<ProductImage>(),
                skus,
                variants,
                DateTimeOffset.UtcNow,
                null,
                "system",
                null
            );

            await _productRepository.AddAsync(product, cancellationToken);
            return productId;
        }

        public async Task<PagingData> GetProductsAsync(ProductSearchRequestDto request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (items, total) = await _productRepository.GetPagedAsync(request.Keyword ?? string.Empty, request.PageIndex, request.PageSize, request.Sort, cancellationToken);
            return new PagingData
            {
                Total = total,
                Data = items
            };
        }

        public async Task<object?> GetProductDetailAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var product = await _productRepository.GetDetailByIdAsync(id, cancellationToken);
            return product;
        }

        public async Task<bool> UpdateProductAsync(Guid id, ProductUpsertRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var existing = await _productRepository.GetDetailByIdAsync(id, cancellationToken);
            if (existing == null) return false;

            existing.UpdateName(request.Name ?? existing.Name);
            existing.UpdateDescription(request.Description ?? existing.Description);

            // For brevity: skip deep merge of categories/attributes/variants/skus
            // depending on your rules you might replace or merge collections here

            await _productRepository.UpdateAsync(existing, cancellationToken);
            return true;
        }
    }
}
