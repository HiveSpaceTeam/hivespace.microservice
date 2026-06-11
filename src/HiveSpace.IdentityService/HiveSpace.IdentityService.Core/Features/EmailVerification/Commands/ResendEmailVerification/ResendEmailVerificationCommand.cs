using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.IdentityService.Core.Features.EmailVerification.Commands.ResendEmailVerification;

public record ResendEmailVerificationCommand(
    string Email,
    string App,
    string? ReturnUrl,
    string? Culture) : ICommand;
