namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;

public record SignInRequest(
    string Email,
    string Password,
    string App,
    string? ReturnUrl,
    string? Culture);
