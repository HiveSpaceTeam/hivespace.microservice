using HiveSpace.Application.Shared.Queries;
using HiveSpace.IdentityService.Application.Models.Responses;

namespace HiveSpace.IdentityService.Application.Queries.Addresses;

public record GetAddressQuery(Guid AddressId) : IQuery<AddressResponseDto>
{
    public static GetAddressQuery FromDto(Guid addressId) => new(addressId);
}

