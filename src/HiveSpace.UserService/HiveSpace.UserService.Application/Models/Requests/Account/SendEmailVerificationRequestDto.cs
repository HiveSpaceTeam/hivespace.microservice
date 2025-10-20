namespace HiveSpace.UserService.Application.Models.Requests.Account;

/// <summary>
/// Request for sending email verification to user
/// </summary>
/// <param name="CallbackUrl">The base URL for the verification callback (e.g., "https://yourapp.com/verify")</param>
/// <param name="ReturnUrl">Optional return URL after verification completion</param>
public record SendEmailVerificationRequestDto(
    string CallbackUrl,
    string? ReturnUrl = null
);