using HiveSpace.IdentityService.Domain.Aggregates.Enums;

namespace HiveSpace.IdentityService.Application.Models.Requests;

public class UpdateUserRequestDto
{
    public string? FullName { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public Gender? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
}
