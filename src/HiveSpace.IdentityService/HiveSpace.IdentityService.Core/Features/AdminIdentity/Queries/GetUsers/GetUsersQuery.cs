using HiveSpace.Application.Shared.Queries;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Dtos;

namespace HiveSpace.IdentityService.Core.Features.AdminIdentity.Queries.GetUsers;

public record GetUsersQuery(int Page, int PageSize, string? SearchTerm) : IQuery<GetUsersResult>;
