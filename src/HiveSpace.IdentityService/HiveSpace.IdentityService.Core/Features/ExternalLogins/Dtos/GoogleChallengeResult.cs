namespace HiveSpace.IdentityService.Core.Features.ExternalLogins.Dtos;

public record GoogleChallengeResult(
    string App,
    string? ReturnUrl,
    string? Culture);
