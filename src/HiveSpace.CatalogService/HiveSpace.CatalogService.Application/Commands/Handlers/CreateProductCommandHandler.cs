using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Interfaces;

namespace HiveSpace.CatalogService.Application.Commands.Handlers;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IProductService _productService;

    public CreateProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        return _productService.SaveProductAsync(request.Payload, cancellationToken);
    }
}

