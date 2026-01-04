using FluentValidation;

namespace HiveSpace.CatalogService.Application.Commands.Validators;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Payload.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Payload.Category)
            .GreaterThan(0);
    }
}

