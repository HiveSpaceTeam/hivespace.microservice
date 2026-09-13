using FluentValidation;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.ValidateCatalogImportBundle;

public class ValidateCatalogImportBundleValidator : AbstractValidator<ValidateCatalogImportBundleCommand>
{
    public ValidateCatalogImportBundleValidator()
    {
        RuleFor(x => x.BundleId)
            .NotEmpty()
            .WithState(_ => new Error(CatalogDomainErrorCode.InvalidCatalogImportBundle, nameof(ValidateCatalogImportBundleCommand.BundleId)));
    }
}
