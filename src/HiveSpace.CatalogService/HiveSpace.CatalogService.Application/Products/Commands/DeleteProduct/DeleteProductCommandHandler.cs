using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Interfaces.Messaging;
using HiveSpace.CatalogService.Domain.Repositories;

namespace HiveSpace.CatalogService.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IProductEventPublisher productEventPublisher)
    : ICommandHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null) return false;

        await productRepository.DeleteAsync(product, cancellationToken);
        await productEventPublisher.PublishProductDeletedAsync(product, cancellationToken);

        return true;
    }
}
