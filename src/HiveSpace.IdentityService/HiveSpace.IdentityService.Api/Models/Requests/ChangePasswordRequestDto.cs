namespace HiveSpace.IdentityService.Application.Models.Requests;

public record ChangePasswordRequestDto(
    string Password,
    string NewPassword
);
