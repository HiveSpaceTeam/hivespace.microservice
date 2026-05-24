using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.IdentityService.Core.Features.EmailVerification.Commands.SendEmailVerification;

public record SendEmailVerificationCommand(
    Guid UserId,
    string CallbackUrl,
    string? ReturnUrl) : ICommand;
