namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Services;

public interface ICsrfTokenService
{
    string Issue(string sessionId, DateTimeOffset expiresAt);

    void Clear();
}
