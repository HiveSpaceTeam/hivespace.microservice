namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;

public record OtpSignInResponseDto(
    string ChallengeToken,
    DateTimeOffset ExpiresAt,
    DateTimeOffset CanResendAt);
