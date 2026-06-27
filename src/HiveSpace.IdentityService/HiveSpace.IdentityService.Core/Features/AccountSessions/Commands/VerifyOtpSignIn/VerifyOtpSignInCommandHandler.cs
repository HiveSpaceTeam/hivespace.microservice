using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;
using HiveSpace.IdentityService.Core.Interfaces;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.VerifyOtpSignIn;

public class VerifyOtpSignInCommandHandler(
    UserManager<ApplicationUser> userManager,
    IOtpChallengeRepository otpChallengeRepository,
    IAccountSessionIssuer accountSessionIssuer,
    IConfiguration configuration)
    : ICommandHandler<VerifyOtpSignInCommand, VerifyOtpSignInResponseDto>
{
    public async Task<VerifyOtpSignInResponseDto> Handle(VerifyOtpSignInCommand command, CancellationToken cancellationToken)
    {
        var challenge = await otpChallengeRepository.GetActiveByChallengeTokenAsync(command.ChallengeToken, cancellationToken);
        if (challenge is null || challenge.Purpose != OtpChallengePurpose.SignIn)
            throw InvalidOrExpiredCode();

        if (!string.Equals(challenge.Code, command.Code, StringComparison.Ordinal))
        {
            challenge.IncrementAttempt();
            if (challenge.AttemptCount >= GetMaxAttempts())
            {
                challenge.Invalidate();
                await otpChallengeRepository.SaveChangesAsync(cancellationToken);
                throw MaxAttemptsExceeded();
            }

            await otpChallengeRepository.SaveChangesAsync(cancellationToken);
            throw InvalidOrExpiredCode();
        }

        var user = await userManager.FindByEmailAsync(challenge.EmailNormalized);
        if (user is null)
        {
            challenge.Invalidate();
            await otpChallengeRepository.SaveChangesAsync(cancellationToken);
            throw InvalidOrExpiredCode();
        }

        var roles = await accountSessionIssuer.ValidateCanIssueAsync(user, command.App, cancellationToken);

        challenge.MarkUsed();
        await otpChallengeRepository.SaveChangesAsync(cancellationToken);

        var session = await accountSessionIssuer.IssueAsync(
            user,
            command.App,
            command.ReturnUrl,
            updateLastLogin: true,
            roles,
            cancellationToken);

        return new VerifyOtpSignInResponseDto(SanitizeRedirectUrl(session.RedirectTo));
    }

    private int GetMaxAttempts()
        => configuration.GetValue("Otp:MaxAttempts", 5);

    private static UnauthorizedException InvalidOrExpiredCode()
        => new([new Error(IdentityDomainErrorCode.InvalidOrExpiredOtpCode, nameof(VerifyOtpSignInCommand.Code))]);

    private static UnauthorizedException MaxAttemptsExceeded()
        => new([new Error(IdentityDomainErrorCode.MaxOtpAttemptsExceeded, nameof(VerifyOtpSignInCommand.Code))]);

    private static string? SanitizeRedirectUrl(string? redirectUrl)
    {
        if (string.IsNullOrWhiteSpace(redirectUrl))
            return null;

        return redirectUrl.StartsWith('/')
            && (redirectUrl.Length == 1 || redirectUrl[1] is not '/' and not '\\')
            ? redirectUrl
            : null;
    }
}
