using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Interfaces;

namespace HiveSpace.CatalogService.Application.Commands.Handlers;

public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, bool>
{
    private readonly IProductService _productService;

    public UpdateProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        return _productService.UpdateProductAsync(request.ProductId, request.Payload, cancellationToken);
    }
}

