using HiveSpace.UserService.Application.Models.Requests.Account;

namespace HiveSpace.UserService.Application.Interfaces.Services;

public interface IAccountService
{
    /// <summary>
    /// Sends an email verification link to the user.
    /// </summary>
    Task SendEmailVerificationAsync(SendEmailVerificationRequestDto request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Confirms email verification using a secure POST request.
    /// </summary>
    Task ConfirmEmailVerificationAsync(ConfirmEmailVerificationRequestDto request, CancellationToken cancellationToken = default);
}