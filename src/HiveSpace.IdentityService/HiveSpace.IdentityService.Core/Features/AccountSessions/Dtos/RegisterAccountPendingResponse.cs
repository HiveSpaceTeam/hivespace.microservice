namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;

public record RegisterAccountPendingResponse(
    string Email,
    string App,
    DateTimeOffset CanResendAt);
