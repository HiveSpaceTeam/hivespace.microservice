using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.CatalogService.Application.Categories.Queries.GetAttributesByCategoryId;

public class GetAttributesByCategoryIdValidator : AbstractValidator<GetAttributesByCategoryIdQuery>
{
    public GetAttributesByCategoryIdValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(GetAttributesByCategoryIdQuery.CategoryId)));
    }
}
