namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;

public record RegisterAccountRequest(
    string Email,
    string Password,
    string ConfirmPassword,
    string? FullName,
    string App,
    string? ReturnUrl,
    string? Culture);
