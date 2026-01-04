using FluentValidation;

namespace HiveSpace.CatalogService.Application.Commands.Validators;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Payload.Name)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.Payload.Name));
    }
}

