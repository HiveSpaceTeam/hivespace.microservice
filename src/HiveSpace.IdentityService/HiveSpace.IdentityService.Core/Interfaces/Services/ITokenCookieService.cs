using HiveSpace.IdentityService.Core.DomainModels;

namespace HiveSpace.IdentityService.Core.Interfaces.Services;

public interface ITokenCookieService
{
    Task<TokenCookieIssueResult> IssueAsync(
        ApplicationUser user,
        string app,
        CancellationToken cancellationToken = default);

    Task<TokenCookieIssueResult> RefreshAsync(
        BrowserRefreshSession currentSession,
        ApplicationUser user,
        string app,
        CancellationToken cancellationToken = default);

    Task<BrowserRefreshSession> GetRequiredRefreshSessionAsync(CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}

public record TokenCookieIssueResult(
    string SessionId,
    DateTimeOffset AccessExpiresAt,
    DateTimeOffset RefreshExpiresAt);

public record BrowserRefreshSession(
    string SessionId,
    Guid UserId,
    string RefreshHandle,
    string App,
    DateTimeOffset RefreshExpiresAt,
    string? SecurityStamp,
    DateTimeOffset IssuedAt);
