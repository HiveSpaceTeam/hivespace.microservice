namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;

public record SessionResponse(
    SessionUser User,
    DateTimeOffset ExpiresAt,
    DateTimeOffset RefreshExpiresAt,
    string CsrfToken,
    string? RedirectTo);
