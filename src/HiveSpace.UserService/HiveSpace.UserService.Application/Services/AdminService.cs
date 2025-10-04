using HiveSpace.Core.Contexts;
using HiveSpace.UserService.Application.Models.Requests.Admin;
using HiveSpace.UserService.Application.Models.Responses.Admin;
using HiveSpace.UserService.Application.Interfaces;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Services;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Application.Models.Queries;
using HiveSpace.UserService.Application.Interfaces.DataQueries;
using HiveSpace.UserService.Application.Constant.Enum;

namespace HiveSpace.UserService.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUserContext _userContext;
    private readonly UserManager _domainUserManager;
    private readonly IUserRepository _userRepository;
    private readonly IUserDataQuery _userDataQuery;
    private readonly IAdminDataQuery _adminDataQuery;

    public AdminService(
        IUserContext userContext,
        UserManager domainUserManager,
        IUserRepository userRepository,
        IUserDataQuery userDataQuery,
        IAdminDataQuery adminDataQuery)
    {
        _userContext = userContext;
        _domainUserManager = domainUserManager;
        _userRepository = userRepository;
        _userDataQuery = userDataQuery;
        _adminDataQuery = adminDataQuery;
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
            created.FullName,
            created.IsSystemAdmin,
            created.CreatedAt,
            created.UpdatedAt ?? created.CreatedAt,
            created.LastLoginAt ?? created.CreatedAt);
    }

    public async Task<GetUsersResponseDto> GetUsersAsync(GetUsersRequestDto request, CancellationToken cancellationToken = default)
    {
        // Map to domain filter
        var filterRequest = new AdminUserFilterRequest
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Role = (RoleFilter)request.Role,
            Status = (StatusFilter)request.Status,
            SearchTerm = request.SearchTerm?.Trim(),
            Sort = request.Sort
        };

        filterRequest.Validate();

        // Get paginated results from Dapper query
        var pagedUsers = await _userDataQuery.GetPagingUsersAsync(filterRequest, cancellationToken);

        return new GetUsersResponseDto(pagedUsers.Items, pagedUsers.Pagination);
    }

    public async Task<GetAdminResponseDto> GetAdminsAsync(GetAdminRequestDto request, CancellationToken cancellationToken = default)
    {
        var filterRequest = new AdminUserFilterRequest
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Role = (RoleFilter)request.Role,
            Status = (StatusFilter)request.Status,
            SearchTerm = request.SearchTerm?.Trim(),
            Sort = request.Sort
        };

        filterRequest.Validate();

        var paged = await _adminDataQuery.GetPagingAdminsAsync(filterRequest, cancellationToken);

        return new GetAdminResponseDto(paged.Items, paged.Pagination);
    }

    public async Task<SetUserStatusResponseDto> SetUserStatusAsync(SetUserStatusRequestDto request, CancellationToken cancellationToken = default)
    {
        // Use domain manager to set user status with proper validation
        var updatedUser = await _domainUserManager.SetUserActiveStatusAsync(
            request.UserId,
            request.IsActive,
            _userContext.UserId,
            cancellationToken);

        // Save changes
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new SetUserStatusResponseDto(
            updatedUser.Id,
            updatedUser.UserName,
            updatedUser.FullName,
            updatedUser.Email.Value,
            (int)updatedUser.Status,
            updatedUser.IsSeller,
            updatedUser.IsAdmin,
            updatedUser.IsSystemAdmin,
            updatedUser.CreatedAt,
            updatedUser.UpdatedAt,
            updatedUser.LastLoginAt,
            null // AvatarUrl not available in domain model
        );
    }
}
