namespace HiveSpace.IdentityService.Core.Interfaces.Services;

public interface ICsrfTokenService
{
    string Issue(string sessionId, DateTimeOffset expiresAt);

    void Clear();
}
