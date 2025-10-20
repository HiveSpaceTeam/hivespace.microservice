namespace HiveSpace.UserService.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string toEmail, string toName, string verificationLink, CancellationToken cancellationToken = default);
    Task SendEmailVerificationSuccessAsync(string toEmail, string toName, CancellationToken cancellationToken = default);
    Task<bool> ValidateEmailConfigurationAsync(CancellationToken cancellationToken = default);
}