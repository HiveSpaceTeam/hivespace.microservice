using HiveSpace.Core.Models.Pagination;

namespace HiveSpace.UserService.Application.Models.Responses.Admin;

public record GetUsersResponseDto(
    IReadOnlyList<UserDto> Users,
    PaginationMetadata Pagination
);
