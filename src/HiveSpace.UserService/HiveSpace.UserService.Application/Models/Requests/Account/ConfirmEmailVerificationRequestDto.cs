namespace HiveSpace.UserService.Application.Models.Requests.Account;

public record ConfirmEmailVerificationRequestDto(
    string UserId,
    string Token,
    string? ReturnUrl = null
);