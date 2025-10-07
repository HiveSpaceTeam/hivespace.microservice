using HiveSpace.Core.Models.Pagination;

namespace HiveSpace.UserService.Application.Models.Responses.Admin;

public record GetUnifiedUsersResponseDto<T>(
    IReadOnlyList<T> Items,
    PaginationMetadata Pagination
) where T : class
{
    // Convenience method to convert to specific response types
    public GetUsersResponseDto ToUsersResponse() =>
        new(Items as IReadOnlyList<UserDto> 
            ?? throw new InvalidOperationException($"Cannot convert {typeof(T).Name} to UserDto"), 
            Pagination);
    public GetAdminResponseDto ToAdminsResponse() =>
        new(Items as IReadOnlyList<AdminDto> 
            ?? throw new InvalidOperationException($"Cannot convert {typeof(T).Name} to AdminDto"), 
            Pagination);
};