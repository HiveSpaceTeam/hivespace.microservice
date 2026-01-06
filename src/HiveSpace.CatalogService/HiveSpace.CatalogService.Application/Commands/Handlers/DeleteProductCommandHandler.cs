using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Domain.Repositories;

namespace HiveSpace.CatalogService.Application.Commands.Handlers;

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _productRepository;

    public DeleteProductCommandHandler(IProductRepository productRepo)
    {
        _productRepository = productRepo;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null) return false;

        await _productRepository.DeleteAsync(product, cancellationToken);
        return true;
    }
}

