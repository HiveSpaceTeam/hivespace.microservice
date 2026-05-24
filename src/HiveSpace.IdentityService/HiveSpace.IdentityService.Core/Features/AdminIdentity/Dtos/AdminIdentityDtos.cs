using HiveSpace.Core.Models.Pagination;

namespace HiveSpace.IdentityService.Core.Features.AdminIdentity.Dtos;

public record CreateAdminResult(
    Guid Id,
    string Email,
    string FullName,
    bool IsSystemAdmin,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset LastLoginAt,
    bool IsActive = true);

public record GetUsersResult(IReadOnlyList<UserIdentityDto> Users, PaginationMetadata Pagination);

public record GetAdminsResult(IReadOnlyList<AdminIdentityDto> Admins, PaginationMetadata Pagination);

public record UserIdentityDto(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    int Status,
    bool IsSeller,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? LastLoginAt,
    string? AvatarUrl);

public record AdminIdentityDto(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    int Status,
    bool IsSystemAdmin,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? LastLoginAt,
    string? AvatarUrl);

public record SetIdentityStatusResult(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    int Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? LastLoginAt,
    string? AvatarUrl);

public record DeleteIdentityUserResult(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    int Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset? DeletedAt,
    string? AvatarUrl,
    bool IsSeller,
    string? DeletedBy);
