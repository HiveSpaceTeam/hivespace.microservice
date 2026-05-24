using HiveSpace.Application.Shared.Queries;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Dtos;

namespace HiveSpace.IdentityService.Core.Features.AdminIdentity.Queries.GetAdmins;

public record GetAdminsQuery(int Page, int PageSize, string? SearchTerm) : IQuery<GetAdminsResult>;
