using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Application.UserAddresses.Dtos;

public record UserAddressRequestDto(
    string FullName,
    string PhoneNumber,
    string Street,
    string Commune,
    string Province,
    string Country,
    string? ZipCode,
    AddressType AddressType,
    bool IsDefault
);
