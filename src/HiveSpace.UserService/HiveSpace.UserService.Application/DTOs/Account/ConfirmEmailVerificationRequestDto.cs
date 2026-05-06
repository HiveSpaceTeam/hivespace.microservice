namespace HiveSpace.UserService.Application.DTOs.Account;

public record ConfirmEmailVerificationRequestDto(
    string UserId,
    string Token,
    string? ReturnUrl = null
);