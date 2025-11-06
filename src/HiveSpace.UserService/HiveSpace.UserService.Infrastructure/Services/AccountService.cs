using HiveSpace.Core.Contexts;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Application.Interfaces.Services;
using HiveSpace.UserService.Application.Models.Requests.Account;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Text;

namespace HiveSpace.UserService.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserContext _userContext;
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        IUserContext userContext,
        IEmailService emailService,
        ILogger<AccountService> logger)
    {
        _userManager = userManager;
        _userContext = userContext;
        _emailService = emailService;
        _logger = logger;
    }
    
    public async Task ConfirmEmailVerificationAsync(
        ConfirmEmailVerificationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Use the existing verification logic but with the new DTO
        await VerifyEmailAsync(request.Token, cancellationToken);
    }

    public async Task SendEmailVerificationAsync(
        SendEmailVerificationRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        var userId = _userContext.UserId;
        
        // Get current user using UserManager
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, "user");

        // Check if email is already confirmed
        var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        if (isEmailConfirmed)
        {
            throw new ConflictException(UserDomainErrorCode.EmailAlreadyVerified, nameof(user.Email));
        }

        // Generate email confirmation token using UserManager
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        
        // Encode the token for URL safety
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        // Build verification link
        var verificationLink = $"{request.CallbackUrl}?token={encodedToken}";
        if (!string.IsNullOrWhiteSpace(request.ReturnUrl))
        {
            verificationLink += $"&returnUrl={Uri.EscapeDataString(request.ReturnUrl)}";
        }

        // Send email
        await _emailService.SendEmailVerificationAsync(
            user.Email!, 
            user.FullName, 
            verificationLink, 
            cancellationToken);

        _logger.LogInformation("Email verification sent to user {UserId} at {Email}", userId, user.Email);
    }

    public async Task VerifyEmailAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        // Get current user using UserManager
        var userId = _userContext.UserId.ToString();
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, "user");

        // Check if already verified
        var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        if (isEmailConfirmed)
        {
            throw new ConflictException(UserDomainErrorCode.EmailAlreadyVerified, nameof(user.Email));
        }

        // Decode the token
        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch
        {
            throw new BadRequestException([new Error(UserDomainErrorCode.EmailVerificationFailed, "token")]);
        }

        // Verify email using UserManager
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        
        if (!result.Succeeded)
        {
            _logger.LogWarning("Email verification failed for user {UserId}. Errors: {Errors}", 
                userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            throw new BadRequestException([new Error(UserDomainErrorCode.EmailVerificationFailed, "token")]);
        }

        _logger.LogInformation("Email verification completed for user {UserId} at {Email}", userId, user.Email);

        // Send confirmation email
        try
        {
            await _emailService.SendEmailVerificationSuccessAsync(
                user.Email!, 
                user.FullName, 
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Don't fail the verification if success email fails
            _logger.LogWarning(ex, "Failed to send email verification success notification to user {UserId}", userId);
        }
    }
}