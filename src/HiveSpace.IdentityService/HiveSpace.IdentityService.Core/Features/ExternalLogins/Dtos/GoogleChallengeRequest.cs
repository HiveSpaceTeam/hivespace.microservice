namespace HiveSpace.IdentityService.Core.Features.ExternalLogins.Dtos;

public record GoogleChallengeRequest(
    string App,
    string? ReturnUrl,
    string? Culture);
