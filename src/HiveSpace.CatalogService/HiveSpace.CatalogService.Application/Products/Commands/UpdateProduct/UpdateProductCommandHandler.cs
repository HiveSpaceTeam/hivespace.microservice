using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Contracts;
using HiveSpace.CatalogService.Application.Helpers;
using HiveSpace.CatalogService.Application.Interfaces.Messaging;
using HiveSpace.CatalogService.Domain.Repositories;
using HiveSpace.Core.Contexts;
using HiveSpace.Infrastructure.Persistence.Transaction;

namespace HiveSpace.CatalogService.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler(
    IProductRepository productRepository,
    ITransactionService transactionService,
    IUserContext userContext,
    IProductEventPublisher productEventPublisher)
    : ICommandHandler<UpdateProductCommand, bool>
{
    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var currentUserId = userContext.UserId.ToString();
        var wasUpdated = false;

        await transactionService.InTransactionScopeAsync(async _ =>
        {
            var product = await productRepository.GetDetailByIdAsync(request.ProductId, false, cancellationToken);
            if (product is null) return;

            var isUpdated = false;
            ProductUpsertRequestDto payload = request.Payload;

            if (!string.IsNullOrWhiteSpace(payload.Name))
            {
                product.UpdateName(payload.Name);
                isUpdated = true;
            }

            if (!string.IsNullOrWhiteSpace(payload.Description))
            {
                product.UpdateDescription(payload.Description);
                isUpdated = true;
            }

            if (payload.Category > 0)
            {
                product.UpdateCategories(ProductFactory.CreateProductCategories(payload.Category));
                isUpdated = true;
            }

            var variants   = ProductFactory.CreateProductVariants(payload.Variants);
            var skus       = ProductFactory.CreateProductSkus(payload.Skus);
            var attributes = ProductFactory.CreateProductAttributes(payload.Attributes);

            isUpdated |= UpdateIfNotEmpty(variants,   () => product.UpdateVariants(variants));
            isUpdated |= UpdateIfNotEmpty(skus,       () => product.UpdateSkus(skus));
            isUpdated |= UpdateIfNotEmpty(attributes, () => product.UpdateAttributes(attributes));

            if (isUpdated)
            {
                product.UpdateAuditInfo(currentUserId);
                await productRepository.UpdateAsync(product, cancellationToken);
                wasUpdated = true;
            }
        }, performIdempotenceCheck: true, actionName: nameof(UpdateProductCommandHandler));

        if (wasUpdated)
        {
            var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product != null)
            {
                await productEventPublisher.PublishProductUpdatedAsync(product, cancellationToken);
                await productEventPublisher.PublishSkuUpdatedAsync(product, cancellationToken);
            }
        }

        return wasUpdated;
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
}
