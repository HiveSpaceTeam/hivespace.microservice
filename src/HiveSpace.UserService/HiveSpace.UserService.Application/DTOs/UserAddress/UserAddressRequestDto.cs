using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Application.DTOs.UserAddress;

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
