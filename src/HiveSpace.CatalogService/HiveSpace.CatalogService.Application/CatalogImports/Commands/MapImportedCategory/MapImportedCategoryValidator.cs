using FluentValidation;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.MapImportedCategory;

public class MapImportedCategoryValidator : AbstractValidator<MapImportedCategoryCommand>
{
    public MapImportedCategoryValidator()
    {
        RuleFor(x => x.ExternalCategoryId)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(MapImportedCategoryCommand.ExternalCategoryId)));

        RuleFor(x => x.BundleId)
            .NotEmpty()
            .WithState(_ => new Error(CatalogDomainErrorCode.InvalidCatalogImportBundle, nameof(MapImportedCategoryCommand.BundleId)));

        RuleFor(x => x.HiveSpaceCategoryId)
            .GreaterThan(0)
            .WithState(_ => new Error(CatalogDomainErrorCode.InvalidImportedCategory, nameof(MapImportedCategoryCommand.HiveSpaceCategoryId)));
    }
}
