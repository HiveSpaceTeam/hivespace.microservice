using HiveSpace.Application.Shared.Commands;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.VerifyOtpSignIn;

public record VerifyOtpSignInCommand(
    string ChallengeToken,
    string Code,
    string App,
    string? ReturnUrl) : ICommand<VerifyOtpSignInResponseDto>;
