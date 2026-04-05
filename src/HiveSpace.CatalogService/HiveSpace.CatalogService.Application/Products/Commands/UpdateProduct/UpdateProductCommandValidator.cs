using FluentValidation;

namespace HiveSpace.CatalogService.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0);

        RuleFor(x => x.Payload.Name)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Payload.Name));
    }
}
