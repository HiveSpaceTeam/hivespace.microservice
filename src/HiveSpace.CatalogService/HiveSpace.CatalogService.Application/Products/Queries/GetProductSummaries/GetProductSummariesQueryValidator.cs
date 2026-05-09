using FluentValidation;

namespace HiveSpace.CatalogService.Application.Products.Queries.GetProductSummaries;

public class GetProductSummariesQueryValidator : AbstractValidator<GetProductSummariesQuery>
{
    public GetProductSummariesQueryValidator()
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
