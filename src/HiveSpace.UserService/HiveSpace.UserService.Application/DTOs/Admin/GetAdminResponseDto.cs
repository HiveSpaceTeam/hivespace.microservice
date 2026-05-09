using HiveSpace.Core.Models.Pagination;

namespace HiveSpace.UserService.Application.DTOs.Admin;

public record GetAdminResponseDto(
    IEnumerable<AdminDto> Admins,
    PaginationMetadata Pagination
);
