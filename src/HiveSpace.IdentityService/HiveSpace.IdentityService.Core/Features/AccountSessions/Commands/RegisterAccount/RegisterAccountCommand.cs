using HiveSpace.Application.Shared.Commands;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.RegisterAccount;

public record RegisterAccountCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string? FullName,
    string App,
    string? ReturnUrl,
    string? Culture) : ICommand<RegisterAccountPendingResponse>;
