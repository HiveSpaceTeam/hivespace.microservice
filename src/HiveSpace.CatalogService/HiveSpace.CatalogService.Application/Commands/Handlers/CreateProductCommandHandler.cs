using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Helpers;
using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Application.Models.Requests;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Repositories.Domain;
using HiveSpace.Core.Contexts;
using HiveSpace.Infrastructure.Persistence.Transaction;

namespace HiveSpace.CatalogService.Application.Commands.Handlers;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IUserContext _userContext;
    private readonly IProductRepository _productRepository;
    private readonly ITransactionService _transactionService;
    public CreateProductCommandHandler(IProductRepository productRepository,
            ITransactionService transactionService,
            IUserContext userContext)
    {
        _productRepository = productRepository;
        _transactionService = transactionService;
        _userContext = userContext;
    }

    private string GetCurrentUserId() => _userContext.UserId.ToString();
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var currentUserId = GetCurrentUserId();

        var payload = request.Payload;

        // Create product first to get the generated ID (synchronous operation)
        var product = new Product(
            payload.Name,
            payload.Description,
            ProductStatus.Available,
            DateTimeOffset.UtcNow,
           currentUserId

        );

        // Build related entities using shared factory methods (synchronous operations)
        var categories = ProductFactory.CreateProductCategories(product.Id, payload.Category);
        var variants = ProductFactory.CreateProductVariants(payload.Variants);
        var skus = ProductFactory.CreateProductSkus(product.Id, payload.Skus);
        var attributes = ProductFactory.CreateProductAttributes(product.Id, payload.Attributes);

        // Update product with related entities
        product.UpdateCategories(categories);
        product.UpdateVariants(variants);
        product.UpdateSkus(skus);
        product.UpdateAttributes(attributes);

        // Only wrap the actual repository operation in transaction
        await _transactionService.InTransactionScopeAsync(async transaction =>
        {
            await _productRepository.AddAsync(product, cancellationToken);
        }, performIdempotenceCheck: true, actionName: nameof(CreateProductCommandHandler));

        return product.Id;
    }
}

