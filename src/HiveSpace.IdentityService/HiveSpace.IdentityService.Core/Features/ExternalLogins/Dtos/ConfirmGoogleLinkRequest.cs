namespace HiveSpace.IdentityService.Core.Features.ExternalLogins.Dtos;

public record ConfirmGoogleLinkRequest(
    bool ConsentAccepted,
    string Password,
    string App,
    string? ReturnUrl,
    string? Culture);
