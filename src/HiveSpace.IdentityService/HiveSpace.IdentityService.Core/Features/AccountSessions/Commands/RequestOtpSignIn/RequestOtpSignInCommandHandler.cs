using HiveSpace.Application.Shared.Handlers;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;
using HiveSpace.IdentityService.Core.Interfaces;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.RequestOtpSignIn;

public class RequestOtpSignInCommandHandler(
    UserManager<ApplicationUser> userManager,
    IOtpChallengeRepository otpChallengeRepository,
    IIdentityEventPublisher identityEventPublisher,
    IConfiguration configuration)
    : ICommandHandler<RequestOtpSignInCommand, OtpSignInResponseDto>
{
    public async Task<OtpSignInResponseDto> Handle(RequestOtpSignInCommand command, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var normalizedEmail = command.Email.Trim().ToUpperInvariant();
        var existingChallenge = await otpChallengeRepository.GetLatestActiveByEmailAndPurposeAsync(
            normalizedEmail,
            OtpChallengePurpose.SignIn,
            cancellationToken);

        if (existingChallenge is not null && existingChallenge.CanResendAt > now)
            return new OtpSignInResponseDto(
                existingChallenge.ChallengeToken,
                existingChallenge.ExpiresAt,
                existingChallenge.CanResendAt);

        var user = await userManager.FindByEmailAsync(command.Email.Trim());
        if (!await IsEligibleAsync(user, cancellationToken))
        {
            if (existingChallenge is not null)
            {
                existingChallenge.Invalidate();
                await otpChallengeRepository.SaveChangesAsync(cancellationToken);
            }

            return CreateDummyResponse(now);
        }

        if (existingChallenge is not null)
            existingChallenge.Invalidate();

        var challenge = OtpChallenge.Create(
            normalizedEmail,
            OtpChallengePurpose.SignIn,
            Guid.NewGuid().ToString("N"),
            GenerateCode(),
            now.AddMinutes(GetExpiryMinutes()),
            now.AddSeconds(GetCooldownSeconds()));

        await otpChallengeRepository.AddAsync(challenge, cancellationToken);
        await identityEventPublisher.PublishOtpChallengeRequestedAsync(
            user!,
            challenge.Code,
            challenge.ExpiresAt,
            nameof(OtpChallengePurpose.SignIn),
            cancellationToken);
        await otpChallengeRepository.SaveChangesAsync(cancellationToken);

        return new OtpSignInResponseDto(
            challenge.ChallengeToken,
            challenge.ExpiresAt,
            challenge.CanResendAt);
    }

    private async Task<bool> IsEligibleAsync(ApplicationUser? user, CancellationToken cancellationToken)
    {
        if (user is null || !user.EmailConfirmed || user.Status != UserStatus.Active)
            return false;

        if (await userManager.IsLockedOutAsync(user))
            return false;

        var roles = await AccountSessionHandlerBase.GetRolesAsync(userManager, user);
        return !roles.Contains("Admin") && !roles.Contains("SystemAdmin");
    }

    private OtpSignInResponseDto CreateDummyResponse(DateTimeOffset now)
        => new(
            Guid.NewGuid().ToString("N"),
            now.AddMinutes(GetExpiryMinutes()),
            now.AddSeconds(GetCooldownSeconds()));

    private int GetExpiryMinutes()
        => configuration.GetValue("Otp:ExpiryMinutes", 10);

    private int GetCooldownSeconds()
        => configuration.GetValue("Otp:CooldownSeconds", 60);

    private string GenerateCode()
    {
        var digits = configuration.GetValue("Otp:CodeLengthDigits", 6);
        var maxExclusive = (int)Math.Pow(10, digits);
        return Random.Shared.Next(0, maxExclusive).ToString($"D{digits}");
    }
}
