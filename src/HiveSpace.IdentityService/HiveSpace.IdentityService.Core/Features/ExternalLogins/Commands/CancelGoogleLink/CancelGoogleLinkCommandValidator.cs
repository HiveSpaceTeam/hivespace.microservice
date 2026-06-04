using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.CancelGoogleLink;

public class CancelGoogleLinkCommandValidator : AbstractValidator<CancelGoogleLinkCommand>
{
    public CancelGoogleLinkCommandValidator()
    {
        RuleFor(c => c.LinkToken)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CancelGoogleLinkCommand.LinkToken)));
    }
}
