using HiveSpace.Core.Models.Pagination;

namespace HiveSpace.UserService.Application.Models.Responses.Admin;

public record GetAdminResponseDto(
    IEnumerable<AdminDto> Admins,
    PaginationMetadata Pagination
);
