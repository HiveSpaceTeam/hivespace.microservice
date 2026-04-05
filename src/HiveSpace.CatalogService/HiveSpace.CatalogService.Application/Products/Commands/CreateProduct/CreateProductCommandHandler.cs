using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Contracts;
using HiveSpace.CatalogService.Application.Helpers;
using HiveSpace.CatalogService.Application.Interfaces.Messaging;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Domain.Repositories;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Persistence.Transaction;

namespace HiveSpace.CatalogService.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    ITransactionService transactionService,
    IUserContext userContext,
    IProductEventPublisher productEventPublisher)
    : ICommandHandler<CreateProductCommand, int>
{
    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var payload = request.Payload;
        var currentUserId = userContext.UserId.ToString();

        var product = Product.CreateProduct(
            name:             payload.Name,
            slug:             $"{payload.Name}-{Guid.NewGuid().ToString("N")[..6]}",
            description:      payload.Description,
            shortDescription: null,
            status:           ProductStatus.Available,
            sellerId:         userContext.StoreId ?? Guid.Empty,
            condition:        ProductCondition.New,
            featured:         false,
            categories:       ProductFactory.CreateProductCategories(payload.Category),
            attributes:       ProductFactory.CreateProductAttributes(payload.Attributes),
            images:           [],
            skus:             ProductFactory.CreateProductSkus(payload.Skus),
            variants:         ProductFactory.CreateProductVariants(payload.Variants),
            createdAt:        DateTimeOffset.UtcNow,
            createdBy:        currentUserId
        );

        await transactionService.InTransactionScopeAsync(async _ =>
        {
            await productRepository.AddAsync(product, cancellationToken);
        }, performIdempotenceCheck: true, actionName: nameof(CreateProductCommandHandler));

        await productEventPublisher.PublishProductCreatedAsync(product, cancellationToken);

        return product.Id;
    }
}
