using FluentValidation;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Application.Stores.Commands.ProvisionImportedSellerStore;

public class ProvisionImportedSellerStoreValidator : AbstractValidator<ProvisionImportedSellerStoreCommand>
{
    public ProvisionImportedSellerStoreValidator()
    {
        RuleFor(x => x.SourceSystem)
            .NotEmpty()
            .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(ProvisionImportedSellerStoreCommand.SourceSystem)))
            .MaximumLength(64)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(ProvisionImportedSellerStoreCommand.SourceSystem)));

        RuleFor(x => x.ExternalSellerId)
            .NotEmpty()
            .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(ProvisionImportedSellerStoreCommand.ExternalSellerId)))
            .MaximumLength(128)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(ProvisionImportedSellerStoreCommand.ExternalSellerId)));

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(ProvisionImportedSellerStoreCommand.UserId)));

        RuleFor(x => x.StoreName)
            .NotEmpty()
            .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(ProvisionImportedSellerStoreCommand.StoreName)))
            .MaximumLength(100)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(ProvisionImportedSellerStoreCommand.StoreName)));

        RuleFor(x => x.SourceUrl)
            .MaximumLength(1000)
            .When(x => x.SourceUrl is not null)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(ProvisionImportedSellerStoreCommand.SourceUrl)));

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500)
            .When(x => x.LogoUrl is not null)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(ProvisionImportedSellerStoreCommand.LogoUrl)));
    }
}
