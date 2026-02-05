using FluentEmail.Core;
using HiveSpace.UserService.Application.Interfaces.Services;
using HiveSpace.UserService.Domain.Models;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using HiveSpace.UserService.Infrastructure.Settings;

namespace HiveSpace.UserService.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IFluentEmail _fluentEmail;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IFluentEmail fluentEmail,
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger)
    {
        _fluentEmail = fluentEmail;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailVerificationAsync(string toEmail, string toName, string verificationLink, CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine($"[EmailService] Sending email verification to: {toEmail}");
            
            var model = new EmailVerificationModel
            {
                UserName = toName,
                VerificationLink = verificationLink,
                AppName = "HiveSpace",
                ExpiresAt = DateTime.UtcNow.AddHours(24) // 24-hour expiration
            };

            var email = _fluentEmail
                .To(toEmail, toName)
                .Subject($"Account Activation - {model.AppName}")
                .UsingTemplate(EmailTemplates.VerificationEmailTemplate, model);

            var result = await email.SendAsync(cancellationToken);

            if (result.Successful)
            {
                Console.WriteLine($"[EmailService] Email verification sent successfully to: {toEmail}");
                _logger.LogInformation("Email verification sent successfully to {Email}", toEmail);
            }
            else
            {
                var errors = string.Join(", ", result.ErrorMessages);
                Console.WriteLine($"[EmailService] Failed to send email verification to: {toEmail}. Errors: {errors}");
                _logger.LogError("Failed to send email verification to {Email}. Errors: {Errors}", toEmail, errors);
                throw new DomainException(500, UserDomainErrorCode.EmailSendFailed, nameof(toEmail));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EmailService] Exception occurred while sending email verification to {toEmail}: {ex.Message}");
            _logger.LogError(ex, "Exception occurred while sending email verification to {Email}", toEmail);
            throw;
        }
    }

    public async Task SendEmailVerificationSuccessAsync(string toEmail, string toName, CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine($"[EmailService] Sending email verification success notification to: {toEmail}");
            
            var model = new EmailVerificationModel
            {
                UserName = toName,
                AppName = "HiveSpace",
                VerificationLink = string.Empty, // Not needed for success email
                ExpiresAt = DateTime.UtcNow // Not needed for success email
            };

            var email = _fluentEmail
                .To(toEmail, toName)
                .Subject($"Email Verified Successfully - {model.AppName}")
                .UsingTemplate(EmailTemplates.VerificationSuccessTemplate, model);

            var result = await email.SendAsync(cancellationToken);

            if (result.Successful)
            {
                Console.WriteLine($"[EmailService] Email verification success notification sent successfully to: {toEmail}");
                _logger.LogInformation("Email verification success notification sent successfully to {Email}", toEmail);
            }
            else
            {
                var errors = string.Join(", ", result.ErrorMessages);
                Console.WriteLine($"[EmailService] Failed to send email verification success notification to: {toEmail}. Errors: {errors}");
                _logger.LogError("Failed to send email verification success notification to {Email}. Errors: {Errors}", toEmail, errors);
                throw new DomainException(500, UserDomainErrorCode.EmailSendFailed, nameof(toEmail));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EmailService] Exception occurred while sending email verification success notification to {toEmail}: {ex.Message}");
            _logger.LogError(ex, "Exception occurred while sending email verification success notification to {Email}", toEmail);
            throw;
        }
    }

    public async Task<bool> ValidateEmailConfigurationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine("[EmailService] Validating email configuration...");
            
            // Check if configuration values are present
            if (!_emailSettings.IsValid())
            {
                Console.WriteLine("[EmailService] Email configuration is invalid - missing required settings");
                _logger.LogWarning("Email configuration is invalid - missing required settings");
                return false;
            }

            Console.WriteLine($"[EmailService] Configuration values are present. Testing SMTP connection to {_emailSettings.SmtpServer}:{_emailSettings.SmtpPort}");

            // Test SMTP connection
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(_emailSettings.TimeoutSeconds), cancellationToken);
                
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    Console.WriteLine($"[EmailService] SMTP connection timeout after {_emailSettings.TimeoutSeconds} seconds");
                    _logger.LogWarning("SMTP connection timeout to {SmtpServer}:{SmtpPort}", _emailSettings.SmtpServer, _emailSettings.SmtpPort);
                    return false;
                }

                if (connectTask.Exception != null)
                {
                    Console.WriteLine($"[EmailService] SMTP connection failed: {connectTask.Exception.GetBaseException().Message}");
                    _logger.LogWarning("SMTP connection failed to {SmtpServer}:{SmtpPort}. Error: {Error}", 
                        _emailSettings.SmtpServer, _emailSettings.SmtpPort, connectTask.Exception.GetBaseException().Message);
                    return false;
                }

                Console.WriteLine("[EmailService] SMTP connection test successful");
                _logger.LogInformation("SMTP connection test successful to {SmtpServer}:{SmtpPort}", _emailSettings.SmtpServer, _emailSettings.SmtpPort);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] SMTP connection test failed: {ex.Message}");
                _logger.LogWarning(ex, "SMTP connection test failed to {SmtpServer}:{SmtpPort}", _emailSettings.SmtpServer, _emailSettings.SmtpPort);
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EmailService] Exception occurred during email configuration validation: {ex.Message}");
            _logger.LogError(ex, "Exception occurred during email configuration validation");
            return false;
        }
    }
}