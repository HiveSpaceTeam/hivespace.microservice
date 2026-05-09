using FluentValidation;

namespace HiveSpace.CatalogService.Application.Products.Queries.GetProducts;

public class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.Payload.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Payload.Page)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Payload.Keyword)
            .MaximumLength(200)
            .When(x => x.Payload.Keyword is not null);
    }
}
