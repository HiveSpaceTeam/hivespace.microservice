using HiveSpace.Core.Models.Pagination;

namespace HiveSpace.UserService.Application.DTOs.Admin;

public record GetUsersResponseDto(
    IReadOnlyList<UserDto> Users,
    PaginationMetadata Pagination
);
