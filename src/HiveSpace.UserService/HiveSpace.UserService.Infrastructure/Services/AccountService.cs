using HiveSpace.Core.Contexts;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Users;
using HiveSpace.UserService.Application.Interfaces.Services;
using HiveSpace.UserService.Application.DTOs.Account;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Infrastructure.Data;
using HiveSpace.UserService.Infrastructure.Identity;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Text;

namespace HiveSpace.UserService.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserContext _userContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly UserDbContext _dbContext;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        IUserContext userContext,
        IPublishEndpoint publishEndpoint,
        UserDbContext dbContext,
        ILogger<AccountService> logger)
    {
        _userManager     = userManager;
        _userContext     = userContext;
        _publishEndpoint = publishEndpoint;
        _dbContext       = dbContext;
        _logger          = logger;
    }

    public async Task ConfirmEmailVerificationAsync(
        ConfirmEmailVerificationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await VerifyEmailAsync(request.UserId, request.Token, cancellationToken);
    }

    public async Task SendEmailVerificationAsync(
        SendEmailVerificationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var userId = _userContext.UserId;

        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, "user");

        var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        if (isEmailConfirmed)
        {
            throw new ConflictException(UserDomainErrorCode.EmailAlreadyVerified, nameof(user.Email));
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var verificationLink = $"{request.CallbackUrl}?userId={Uri.EscapeDataString(userId.ToString())}&token={encodedToken}";
        if (!string.IsNullOrWhiteSpace(request.ReturnUrl))
        {
            verificationLink += $"&returnUrl={Uri.EscapeDataString(request.ReturnUrl)}";
        }

        await _publishEndpoint.Publish(new UserEmailVerificationRequestedIntegrationEvent
        {
            UserId           = userId,
            ToEmail          = user.Email!,
            ToName           = user.FullName,
            VerificationLink = verificationLink,
            ExpiresAt        = DateTime.UtcNow.AddHours(24),
            Locale           = user.Culture,
        }, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email verification event published for user {UserId} at {Email}", userId, user.Email);
    }

    private async Task VerifyEmailAsync(
        string userId,
        string token,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        if (isEmailConfirmed)
        {
            throw new ConflictException(UserDomainErrorCode.EmailAlreadyVerified, nameof(user.Email));
        }

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch
        {
            throw new BadRequestException([new Error(UserDomainErrorCode.EmailVerificationFailed, "token")]);
        }

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Email verification failed for user {UserId}. Errors: {Errors}",
                userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            throw new BadRequestException([new Error(UserDomainErrorCode.EmailVerificationFailed, "token")]);
        }

        _logger.LogInformation("Email verification completed for user {UserId} at {Email}", userId, user.Email);

        try
        {
            await _publishEndpoint.Publish(new UserEmailVerifiedIntegrationEvent
            {
                UserId  = Guid.Parse(userId),
                ToEmail = user.Email!,
                ToName  = user.FullName,
                Locale  = user.Culture,
            }, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Don't fail the verification if the success notification fails to publish
            _logger.LogWarning(ex, "Failed to publish email verified event for user {UserId}", userId);
        }
    }
}
