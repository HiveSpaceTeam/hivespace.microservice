using HiveSpace.Core.Contexts;
using HiveSpace.UserService.Application.Models.Requests.Admin;
using HiveSpace.UserService.Application.Models.Responses.Admin;
using HiveSpace.UserService.Application.Interfaces;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Services;
using HiveSpace.UserService.Domain.Repositories;

namespace HiveSpace.UserService.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUserContext _userContext;
    private readonly UserManager _domainUserManager;
    private readonly IUserRepository _userRepository;

    public AdminService(
        IUserContext userContext,
        UserManager domainUserManager,
        IUserRepository userRepository)
    {
        _userContext = userContext;
        _domainUserManager = domainUserManager;
        _userRepository = userRepository;
    }

    public async Task<CreateAdminResponseDto> CreateAdminAsync(CreateAdminRequestDto request, CancellationToken cancellationToken = default)
    {
        // Determine role
        var role = Role.FromName(request.IsSystemAdmin ? Role.RoleNames.SystemAdmin : Role.RoleNames.Admin);

        // Build domain value objects
        var email = Email.Create(request.Email);
        var fullName = request.FullName.Trim();

        // Use domain manager to validate creator and create domain user aggregate
        var domainUser = await _domainUserManager.CreateAdminUserAsync(
            email,
            email,
            fullName,
            role,
            _userContext.UserId,
            cancellationToken);

        // Persist via repository + Identity and get complete aggregate back
        var created = await _userRepository.CreateUserAsync(domainUser, request.Password, cancellationToken);

        return new CreateAdminResponseDto(
            created.Id,
            created.Email.Value,
            created.UserName,
            created.FullName,
            created.Role?.Name ?? string.Empty);
    }
}
