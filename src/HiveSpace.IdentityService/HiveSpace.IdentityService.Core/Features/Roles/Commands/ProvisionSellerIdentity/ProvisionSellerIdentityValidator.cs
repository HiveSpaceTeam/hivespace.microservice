using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.IdentityService.Core.Features.Roles.Commands.ProvisionSellerIdentity;

public class ProvisionSellerIdentityValidator : AbstractValidator<ProvisionSellerIdentityCommand>
{
    public ProvisionSellerIdentityValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ProvisionSellerIdentityCommand.UserId)));

        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ProvisionSellerIdentityCommand.StoreId)));

        RuleFor(x => x.StoreName)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ProvisionSellerIdentityCommand.StoreName)));

        RuleFor(x => x.CorrelationId)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ProvisionSellerIdentityCommand.CorrelationId)));
    }
}
