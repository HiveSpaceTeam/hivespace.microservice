using HiveSpace.Application.Shared.Queries;
using HiveSpace.IdentityService.Application.Models.Responses;

namespace HiveSpace.IdentityService.Application.Queries.Addresses;

public record GetAddressesQuery() : IQuery<IEnumerable<AddressResponseDto>>;

