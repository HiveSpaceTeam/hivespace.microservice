namespace HiveSpace.UserService.Application.Models.Requests.Account;

public record ConfirmEmailVerificationRequestDto(
    string Token,
    string? ReturnUrl = null
);