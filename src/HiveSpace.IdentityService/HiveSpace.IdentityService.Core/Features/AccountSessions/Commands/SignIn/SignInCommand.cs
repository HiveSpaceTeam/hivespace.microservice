using HiveSpace.Application.Shared.Commands;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.SignIn;

public record SignInCommand(
    string Email,
    string Password,
    string App,
    string? ReturnUrl,
    string? Culture) : ICommand<SessionResponse>;
