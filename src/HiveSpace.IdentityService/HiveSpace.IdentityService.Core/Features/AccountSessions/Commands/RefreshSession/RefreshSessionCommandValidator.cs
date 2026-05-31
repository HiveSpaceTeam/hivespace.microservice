using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Commands;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.RefreshSession;

public class RefreshSessionCommandValidator : AbstractValidator<RefreshSessionCommand>
{
    public RefreshSessionCommandValidator()
    {
        RuleFor(c => c.App)
            .Must(AccountSessionValidation.IsKnownApp)
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(RefreshSessionCommand.App)));
    }
}
