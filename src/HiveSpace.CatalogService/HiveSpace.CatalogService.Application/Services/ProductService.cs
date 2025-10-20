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
				new ProductVariant(v.Id != Guid.Empty ? v.Id : Guid.NewGuid(), v.Name, v.Options?.Select(o => new ProductVariantOption(v.Id, o.OptionId, o.Value ?? string.Empty)).ToList() ?? new List<ProductVariantOption>())
			).ToList() ?? new List<ProductVariant>();

            var skus = request.Skus?.Select(s =>
            {
                var skuId = s.Id != Guid.Empty ? s.Id : Guid.NewGuid();
                var money = s.Price;
                var skuVariants = s.SkuVariants?.Select(sv => new SkuVariant(skuId, sv.VariantId, sv.OptionId, sv.Value ?? string.Empty)).ToList() ?? new List<SkuVariant>();
                return new Sku(skuId, s.SkuNo ?? string.Empty, productId, skuVariants, new List<SkuImage>(), s.Quantity, true, money);

            }).ToList() ?? new List<Sku>();

            var attributes = request.Attributes?.Select(a => new ProductAttribute(a.AttributeId, productId, a.SelectedValueIds, a.FreeTextValue)).ToList() ?? new List<ProductAttribute>();

            var product = new Product(
                productId,
                request.Name,
                request.Description,
                ProductStatus.Available,
                categories,
                attributes,
                new List<ProductImage>(),
                skus,
                variants,
                DateTimeOffset.UtcNow,
                null,
                "",
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

            // Update basic properties
            if (!string.IsNullOrEmpty(request.Name))
            {
                existing.UpdateName(request.Name);
            }

            if (!string.IsNullOrEmpty(request.Description))
            {
                existing.UpdateDescription(request.Description);
            }

            // Update categories
            if (request.Category != Guid.Empty)
            {
                var categories = new List<ProductCategory>
                {
                    new ProductCategory(id, request.Category)
                };
                existing.UpdateCategories(categories);
            }

            // Update variants
            if (request.Variants != null && request.Variants.Any())
            {
				var variants = request.Variants.Select(v =>
					new ProductVariant(v.Id != Guid.Empty ? v.Id : Guid.NewGuid(), v.Name, v.Options?.Select(o => new ProductVariantOption(v.Id, o.OptionId, o.Value ?? string.Empty)).ToList() ?? new List<ProductVariantOption>())
				).ToList();
                existing.UpdateVariants(variants);
            }

            // Update SKUs
            if (request.Skus != null && request.Skus.Any())
            {
                var skus = request.Skus.Select(s =>
                {
                    var skuId = s.Id != Guid.Empty ? s.Id : Guid.NewGuid();
                    var money = s.Price;
                    var skuVariants = s.SkuVariants?.Select(sv => new SkuVariant(skuId, sv.VariantId, sv.OptionId, sv.Value ?? string.Empty)).ToList() ?? new List<SkuVariant>();
                    return new Sku(skuId, s.SkuNo ?? string.Empty, id, skuVariants, new List<SkuImage>(), s.Quantity, true, money);
                }).ToList();
                existing.UpdateSkus(skus);
            }

            // Update attributes
            if (request.Attributes != null && request.Attributes.Any())
            {
                var attributes = request.Attributes.Select(a => new ProductAttribute(a.AttributeId, id, a.SelectedValueIds, a.FreeTextValue)).ToList();
                existing.UpdateAttributes(attributes);
            }

            // Update audit information
            existing.UpdateAuditInfo("system");

            await _productRepository.UpdateAsync(existing, cancellationToken);
            return true;
        }

        public async Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null) return false;

            await _productRepository.DeleteAsync(product, cancellationToken);
            return true;
        }
    }
}
