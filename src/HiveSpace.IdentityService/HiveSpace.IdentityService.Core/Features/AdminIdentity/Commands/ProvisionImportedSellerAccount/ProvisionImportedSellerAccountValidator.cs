using FluentValidation;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Core.Exceptions;

namespace HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.ProvisionImportedSellerAccount;

public class ProvisionImportedSellerAccountValidator : AbstractValidator<ProvisionImportedSellerAccountCommand>
{
    public ProvisionImportedSellerAccountValidator()
    {
        RuleFor(x => x.SourceSystem)
            .NotEmpty()
            .WithState(_ => new Error(IdentityDomainErrorCode.InvalidConfiguration, nameof(ProvisionImportedSellerAccountCommand.SourceSystem)))
            .MaximumLength(64)
            .WithState(_ => new Error(IdentityDomainErrorCode.InvalidConfiguration, nameof(ProvisionImportedSellerAccountCommand.SourceSystem)));

        RuleFor(x => x.ExternalSellerId)
            .NotEmpty()
            .WithState(_ => new Error(IdentityDomainErrorCode.InvalidConfiguration, nameof(ProvisionImportedSellerAccountCommand.ExternalSellerId)))
            .MaximumLength(128)
            .WithState(_ => new Error(IdentityDomainErrorCode.InvalidConfiguration, nameof(ProvisionImportedSellerAccountCommand.ExternalSellerId)));

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithState(_ => new Error(IdentityDomainErrorCode.InvalidConfiguration, nameof(ProvisionImportedSellerAccountCommand.DisplayName)))
            .MaximumLength(100)
            .WithState(_ => new Error(IdentityDomainErrorCode.InvalidConfiguration, nameof(ProvisionImportedSellerAccountCommand.DisplayName)));

        RuleFor(x => x.SourceUrl)
            .MaximumLength(1000)
            .When(x => x.SourceUrl is not null)
            .WithState(_ => new Error(IdentityDomainErrorCode.InvalidConfiguration, nameof(ProvisionImportedSellerAccountCommand.SourceUrl)));
    }
}
