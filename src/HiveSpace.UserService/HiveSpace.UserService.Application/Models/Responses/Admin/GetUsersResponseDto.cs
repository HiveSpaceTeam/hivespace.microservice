using HiveSpace.Core.Models.Pagination;

namespace HiveSpace.UserService.Application.Models.Responses.Admin;

public record GetUsersResponseDto(
    IReadOnlyList<UserListItemDto> Users,
    PaginationMetadata Pagination
);
