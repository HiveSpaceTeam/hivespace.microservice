namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;

public record SessionUser(
    Guid UserId,
    string Email,
    string? DisplayName,
    IReadOnlyCollection<string> Roles,
    bool EmailVerified,
    string AccountStatus);
