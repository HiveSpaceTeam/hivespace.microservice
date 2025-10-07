using HiveSpace.Core.Contexts;
using HiveSpace.UserService.Application.Models.Requests.Admin;
using HiveSpace.UserService.Application.Models.Responses.Admin;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Services;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Application.Models.Queries;
using HiveSpace.UserService.Application.Interfaces.Services;
using HiveSpace.UserService.Application.Interfaces.DataQueries;
using HiveSpace.UserService.Application.Constant.Enum;
using HiveSpace.UserService.Application.Extensions;

namespace HiveSpace.UserService.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUserContext _userContext;
    private readonly UserManager _domainUserManager;
    private readonly IUserRepository _userRepository;
    private readonly IUnifiedUserDataQuery _unifiedUserDataQuery;

    public AdminService(
        IUserContext userContext,
        UserManager domainUserManager,
        IUserRepository userRepository,
        IUnifiedUserDataQuery unifiedUserDataQuery)
    {
        _userContext = userContext;
        _domainUserManager = domainUserManager;
        _userRepository = userRepository;
        _unifiedUserDataQuery = unifiedUserDataQuery;
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
        // Use the unified method
        var baseRequest = new GetUsersBaseRequestDto(request.Page, request.PageSize, request.Role, request.Status, request.SearchTerm, request.Sort);
        var unifiedResult = await GetUnifiedUsersAsync<UserDto>(baseRequest, UserQueryType.Users, cancellationToken);
        
        return unifiedResult.ToUsersResponse();
    }

    public async Task<GetAdminResponseDto> GetAdminsAsync(GetAdminRequestDto request, CancellationToken cancellationToken = default)
    {
        // Use the unified method
        var baseRequest = new GetUsersBaseRequestDto(request.Page, request.PageSize, request.Role, request.Status, request.SearchTerm, request.Sort);
        var unifiedResult = await GetUnifiedUsersAsync<AdminDto>(baseRequest, UserQueryType.Admins, cancellationToken);
        
        return unifiedResult.ToAdminsResponse();
    }

    public async Task<GetUnifiedUsersResponseDto<T>> GetUnifiedUsersAsync<T>(GetUsersBaseRequestDto request, UserQueryType queryType, CancellationToken cancellationToken = default) where T : class
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

        // Get paginated results from unified query
        var pagedUsers = await _unifiedUserDataQuery.GetPagingUsersAsync(filterRequest, queryType, cancellationToken);

        // Convert to specific DTO types based on query type
        IReadOnlyList<T> items = queryType switch
        {
            UserQueryType.Users => pagedUsers.Items.Select(x => x.ToUserDto()).ToList() as IReadOnlyList<T> ?? throw new InvalidOperationException("Type conversion failed"),
            UserQueryType.Admins => pagedUsers.Items.Select(x => x.ToAdminDto()).ToList() as IReadOnlyList<T> ?? throw new InvalidOperationException("Type conversion failed"),
            _ => throw new ArgumentOutOfRangeException(nameof(queryType), queryType, "Invalid query type")
        };

        return new GetUnifiedUsersResponseDto<T>(items, pagedUsers.Pagination);
    }

    public async Task<object> SetUserStatusAsync(SetUserStatusRequestDto request, CancellationToken cancellationToken = default)
    {
        // Use domain manager to set user status with proper validation
        var updatedUser = await _domainUserManager.SetUserActiveStatusAsync(
            request.UserId,
            request.IsActive,
            _userContext.UserId,
            cancellationToken);

        // Save changes
        await _userRepository.UpdateUserAsync(updatedUser, cancellationToken);

        // Return appropriate DTO based on ResponseType using extension methods
        return request.ResponseType switch
        {
            UserQueryType.Users => updatedUser.ToUserDto(),
            UserQueryType.Admins => updatedUser.ToAdminDto(),
            _ => throw new ArgumentOutOfRangeException(nameof(request.ResponseType), request.ResponseType, "Invalid response type")
        };
    }
}
