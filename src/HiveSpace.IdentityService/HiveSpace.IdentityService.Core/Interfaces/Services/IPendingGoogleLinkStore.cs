namespace HiveSpace.IdentityService.Core.Interfaces.Services;

public interface IPendingGoogleLinkStore
{
    Task<PendingGoogleLinkState> CreateAsync(PendingGoogleLinkCreateRequest request, CancellationToken cancellationToken = default);

    Task<PendingGoogleLinkState> GetRequiredAsync(string linkToken, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}

public record PendingGoogleLinkCreateRequest(
    string Provider,
    string ProviderKey,
    string ProviderDisplayName,
    string VerifiedEmail,
    Guid TargetAccountId,
    string App,
    string? ReturnUrl,
    string? Culture,
    DateTimeOffset ExpiresAt);

public record PendingGoogleLinkState(
    string Provider,
    string ProviderKey,
    string ProviderDisplayName,
    string VerifiedEmail,
    Guid TargetAccountId,
    string App,
    string? ReturnUrl,
    string? Culture,
    DateTimeOffset ExpiresAt,
    string LinkToken);
