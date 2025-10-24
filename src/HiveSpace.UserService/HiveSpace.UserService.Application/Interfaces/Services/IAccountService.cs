using HiveSpace.UserService.Application.Models.Requests.Account;

namespace HiveSpace.UserService.Application.Interfaces.Services;

public interface IAccountService
{
    Task SendEmailVerificationAsync(SendEmailVerificationRequestDto request, CancellationToken cancellationToken = default);
    Task VerifyEmailAsync(string token, CancellationToken cancellationToken = default);
}