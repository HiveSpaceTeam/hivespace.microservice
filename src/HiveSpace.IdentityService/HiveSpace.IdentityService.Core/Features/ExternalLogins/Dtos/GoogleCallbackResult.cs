namespace HiveSpace.IdentityService.Core.Features.ExternalLogins.Dtos;

public enum GoogleCallbackOutcome
{
    SignedIn,
    PendingLink,
    Failed
}

public record GoogleCallbackResult(
    GoogleCallbackOutcome Outcome,
    string App,
    string? ReturnUrl,
    string? Culture,
    string? LinkToken,
    string? ErrorCode);
