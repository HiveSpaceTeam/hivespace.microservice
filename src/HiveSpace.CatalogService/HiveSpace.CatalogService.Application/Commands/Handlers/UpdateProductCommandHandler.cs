using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Helpers;
using HiveSpace.CatalogService.Application.Models.Requests;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Repositories;
using HiveSpace.Core.Contexts;
using HiveSpace.Infrastructure.Persistence.Transaction;

namespace HiveSpace.CatalogService.Application.Commands.Handlers;

public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, bool>
{
    private readonly IUserContext _userContext;
    private readonly IProductRepository _productRepository;
    private readonly ITransactionService _transactionService;

    public UpdateProductCommandHandler(IProductRepository productRepository,
            ITransactionService transactionService,
            IUserContext userContext)
    {
        _productRepository = productRepository;
        _transactionService = transactionService;
        _userContext = userContext;
    }
    private string GetCurrentUserId() => _userContext.UserId.ToString();

    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var currentUserId = GetCurrentUserId();
        var wasUpdated = false;

        await _transactionService.InTransactionScopeAsync(async transaction =>
        {
            // Get the existing product within the transaction
            var product = await _productRepository.GetDetailByIdAsync(request.ProductId, cancellationToken);
            if (product is null) return;

            var isUpdated = false;

            ProductUpsertRequestDto payload = request.Payload;

            // Update basic properties (synchronous operations)
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

            // Update categories using shared factory method (synchronous operations)
            if (payload.Category > 0)
            {
                var categories = ProductFactory.CreateProductCategories(product.Id, payload.Category);
                product.UpdateCategories(categories);
                isUpdated = true;
            }

            // Build and update variants, SKUs, and attributes using shared factory methods
            var variants = ProductFactory.CreateProductVariants(payload.Variants);
            var skus = ProductFactory.CreateProductSkus(product.Id, payload.Skus);
            var attributes = ProductFactory.CreateProductAttributes(product.Id, payload.Attributes);

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
            }
        }, performIdempotenceCheck: true, actionName: nameof(UpdateProductCommandHandler));

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

