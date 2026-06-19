using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Application.UserAddresses.Dtos;

public record UserAddressDto(
    Guid Id,
    string FullName,
    string PhoneNumber,
    string Street,
    string Commune,
    string Province,
    string Country,
    string? ZipCode,
    AddressType AddressType,
    bool IsDefault
)
{
    internal static UserAddressDto FromAddress(Address address) => new(
        address.Id,
        address.FullName,
        address.PhoneNumber,
        address.Street,
        address.Commune,
        address.Province,
        address.Country,
        address.ZipCode,
        address.AddressType,
        address.IsDefault
    );
}
