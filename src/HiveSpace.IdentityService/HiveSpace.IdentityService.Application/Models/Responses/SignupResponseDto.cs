namespace HiveSpace.IdentityService.Application.Models.Responses;

public record SignupResponseDto(
    string Email,
    string FullName,
    string UserName,
    Guid UserId
);