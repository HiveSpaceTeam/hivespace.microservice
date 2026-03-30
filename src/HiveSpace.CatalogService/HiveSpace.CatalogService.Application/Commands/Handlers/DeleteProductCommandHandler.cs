using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Interfaces.Messaging;
using HiveSpace.CatalogService.Domain.Repositories;

namespace HiveSpace.CatalogService.Application.Commands.Handlers;

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductEventPublisher _productEventPublisher;

    public DeleteProductCommandHandler(IProductRepository productRepo, IProductEventPublisher productEventPublisher)
    {
        _productRepository = productRepo;
        _productEventPublisher = productEventPublisher;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null) return false;

        await _productRepository.DeleteAsync(product, cancellationToken);

        await _productEventPublisher.PublishProductDeletedAsync(product, cancellationToken);
        return true;
    }
}

