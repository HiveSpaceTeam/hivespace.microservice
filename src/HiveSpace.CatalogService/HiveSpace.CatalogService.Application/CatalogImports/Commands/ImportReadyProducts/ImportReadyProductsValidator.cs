using FluentValidation;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Enumerations;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.ImportReadyProducts;

public class ImportReadyProductsValidator : AbstractValidator<ImportReadyProductsCommand>
{
    public ImportReadyProductsValidator()
    {
        RuleFor(x => x.BundleId)
            .NotEmpty()
            .WithState(_ => new Error(CatalogDomainErrorCode.InvalidCatalogImportBundle, nameof(ImportReadyProductsCommand.BundleId)));

        RuleFor(x => x.PublicationState)
            .Must(BeSupportedPublicationState)
            .WithState(_ => new Error(CatalogDomainErrorCode.InvalidProductStatus, nameof(ImportReadyProductsCommand.PublicationState)));
    }

    internal static bool BeSupportedPublicationState(string? publicationState)
    {
        if (string.IsNullOrWhiteSpace(publicationState))
            return false;

        return publicationState.Trim() switch
        {
            "Draft" => true,
            nameof(ProductStatus.Available) => true,
            nameof(ProductStatus.Unpublish) => true,
            _ => false
        };
    }
}
