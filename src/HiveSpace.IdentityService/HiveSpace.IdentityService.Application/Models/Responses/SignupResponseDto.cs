namespace HiveSpace.IdentityService.Application.Models.Responses;

public class SignupResponseDto
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}
