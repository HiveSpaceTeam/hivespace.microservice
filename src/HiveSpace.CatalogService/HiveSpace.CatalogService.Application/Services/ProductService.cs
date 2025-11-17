using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Application.Interfaces.Repositories;
using HiveSpace.CatalogService.Application.Models.Dtos.Crud;
using HiveSpace.CatalogService.Application.Models.Dtos.Request.Product;
using HiveSpace.CatalogService.Application.Models.Requests;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Core.Contexts;
using HiveSpace.Infrastructure.Persistence.Transaction;
using HiveSpace.CatalogService.Application.Interfaces.Messaging;

namespace HiveSpace.CatalogService.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ITransactionService _transactionService;
        private readonly IUserContext _userContext;
        private readonly ICatalogEventPublisher _catalogEventPublisher;

        public ProductService(
            IProductRepository productRepository,
            ITransactionService transactionService,
            IUserContext userContext,
            ICatalogEventPublisher catalogEventPublisher)
        {
            _productRepository = productRepository;
            _transactionService = transactionService;
            _userContext = userContext;
            _catalogEventPublisher = catalogEventPublisher;
        }

        public async Task<Guid> SaveProductAsync(ProductUpsertRequestDto request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var currentUserId = GetCurrentUserId();

            // Create product first to get the generated ID (synchronous operation)
            var product = new Product(
                request.Name,
                request.Description,
                ProductStatus.Available,
                DateTimeOffset.UtcNow,
                currentUserId
            );

            // Build related entities using shared factory methods (synchronous operations)
            var categories = CreateProductCategories(product.Id, request.Category);
            var variants = CreateProductVariants(request.Variants);
            var skus = CreateProductSkus(product.Id, request.Skus);
            var attributes = CreateProductAttributes(product.Id, request.Attributes);

            // Update product with related entities
            product.UpdateCategories(categories);
            product.UpdateVariants(variants);
            product.UpdateSkus(skus);
            product.UpdateAttributes(attributes);

            // Only wrap the actual repository operation in transaction
            await _transactionService.InTransactionScopeAsync(async transaction =>
            {
                await _productRepository.AddAsync(product, cancellationToken);
            }, performIdempotenceCheck: true, actionName: nameof(SaveProductAsync));

            await _catalogEventPublisher.PublishProductCreatedAsync(product, cancellationToken);

            return product.Id;
        }

        public async Task<PagingData> GetProductsAsync(ProductSearchRequestDto request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (items, total) = await _productRepository.GetPagedAsync(request.Keyword ?? string.Empty, request.PageIndex, request.PageSize, request.Sort, cancellationToken);
            return new PagingData(total, items);
        }

        public async Task<Product> GetProductDetailAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var product = await _productRepository.GetDetailByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException(CatalogErrorCode.ProductNotFound, nameof(Product));
            return product;
        }

        public async Task<bool> UpdateProductAsync(Guid id, ProductUpsertRequestDto request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var currentUserId = GetCurrentUserId();
            var wasUpdated = false;
            Product? updatedProduct = null;

            await _transactionService.InTransactionScopeAsync(async transaction =>
            {
                // Get the existing product within the transaction
                var product = await _productRepository.GetDetailByIdAsync(id, cancellationToken);
                if (product is null) return;

                var isUpdated = false;

                // Update basic properties (synchronous operations)
                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    product.UpdateName(request.Name);
                    isUpdated = true;
                }

                if (!string.IsNullOrWhiteSpace(request.Description))
                {
                    product.UpdateDescription(request.Description);
                    isUpdated = true;
                }

                // Update categories using shared factory method (synchronous operations)
                if (request.Category > 0)
                {
                    var categories = CreateProductCategories(product.Id, request.Category);
                    product.UpdateCategories(categories);
                    isUpdated = true;
                }

                // Build and update variants, SKUs, and attributes using shared factory methods
                var variants = CreateProductVariants(request.Variants);
                var skus = CreateProductSkus(product.Id, request.Skus);
                var attributes = CreateProductAttributes(product.Id, request.Attributes);

                // Update collections if they have items
                isUpdated |= UpdateIfNotEmpty(variants, () => product.UpdateVariants(variants));
                isUpdated |= UpdateIfNotEmpty(skus, () => product.UpdateSkus(skus));
                isUpdated |= UpdateIfNotEmpty(attributes, () => product.UpdateAttributes(attributes));

                // Update audit information and persist if any changes were made
                if (isUpdated)
                {
                    product.UpdateAuditInfo(currentUserId);
                    await _productRepository.UpdateAsync(product, cancellationToken);
                    wasUpdated = true;
                    updatedProduct = product;
                }
            }, performIdempotenceCheck: true, actionName: nameof(UpdateProductAsync));

            if (updatedProduct is not null)
            {
                await _catalogEventPublisher.PublishProductUpdatedAsync(updatedProduct, cancellationToken);
            }

            return wasUpdated;
        }

        public async Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product is null) return false;

            await _productRepository.DeleteAsync(product, cancellationToken);
            await _catalogEventPublisher.PublishProductDeletedAsync(product.Id, _userContext.UserId.ToString(), cancellationToken);
            return true;
        }

        #region Shared Factory Methods

        private static List<ProductCategory> CreateProductCategories(Guid productId, int categoryId)
        {
            return categoryId > 0 ? [new ProductCategory(productId, categoryId)] : [];
        }

        private static bool UpdateIfNotEmpty<T>(ICollection<T> items, Action updateAction)
        {
            if (items.Count > 0)
            {
                updateAction();
                return true;
            }
            return false;
        }

        private string GetCurrentUserId() => _userContext.UserId.ToString();

        private static List<ProductVariant> CreateProductVariants(ICollection<ProductVariantRequestDto>? variantRequests)
        {
            return variantRequests?.Select(CreateProductVariant).ToList() ?? [];
        }

        private static ProductVariant CreateProductVariant(ProductVariantRequestDto v)
        {
            var variantId = v.Id != Guid.Empty ? v.Id : Guid.NewGuid();
            var options = v.Options?.Select(o => new ProductVariantOption(variantId, o.OptionId, o.Value ?? string.Empty)).ToList() ?? [];
            return new ProductVariant(variantId, v.Name, options);
        }

        private static List<Sku> CreateProductSkus(Guid productId, ICollection<ProductSkuRequestDto>? skuRequests)
        {
            if (skuRequests is null) return [];

            return [.. skuRequests.Select(s =>
            {
                var skuId = s.Id != Guid.Empty ? s.Id : Guid.NewGuid();
                var skuVariants = s.SkuVariants?.Select(sv => new SkuVariant(skuId, sv.VariantId, sv.OptionId, sv.Value ?? string.Empty)).ToList() ?? [];
                return new Sku(skuId, s.SkuNo ?? string.Empty, productId, skuVariants, [], s.Quantity, true, s.Price);
            })];
        }

        private static List<ProductAttribute> CreateProductAttributes(Guid productId, ICollection<ProductAttributeRequestDto>? attributeRequests)
        {
            return attributeRequests?.Select(a => new ProductAttribute(a.AttributeId, productId, a.SelectedValueIds, a.FreeTextValue)).ToList() ?? [];
        }

        #endregion
    }
}
