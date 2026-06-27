using HiveSpace.Application.Shared.Commands;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.RequestOtpSignIn;

public record RequestOtpSignInCommand(string Email) : ICommand<OtpSignInResponseDto>;
