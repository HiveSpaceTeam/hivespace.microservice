using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.IdentityService.Core.Features.EmailVerification.Commands.ConfirmEmailVerification;

public record ConfirmEmailVerificationCommand(string UserId, string Token) : ICommand;
