namespace HiveSpace.IdentityService.Api.Contracts;

public record VerifyOtpSignInRequest(string ChallengeToken, string Code);
