using FluentValidation;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.ProvisionImportedSellers;

public class ProvisionImportedSellersValidator : AbstractValidator<ProvisionImportedSellersCommand>
{
    public ProvisionImportedSellersValidator()
    {
        RuleFor(x => x.BundleId)
            .NotEmpty()
            .WithState(_ => new Error(CatalogDomainErrorCode.InvalidCatalogImportBundle, nameof(ProvisionImportedSellersCommand.BundleId)));
    }
}
