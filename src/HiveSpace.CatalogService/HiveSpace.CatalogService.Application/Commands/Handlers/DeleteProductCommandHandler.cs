using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Interfaces;

namespace HiveSpace.CatalogService.Application.Commands.Handlers;

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, bool>
{
    private readonly IProductService _productService;

    public DeleteProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        return _productService.DeleteProductAsync(request.ProductId, cancellationToken);
    }
}

