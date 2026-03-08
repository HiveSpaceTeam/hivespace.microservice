using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Application.DTOs.UserAddress;

public record UserAddressDto(
    Guid Id,
    string FullName,
    string PhoneNumber,
    string Street,
    string District,
    string Province,
    string Country,
    string? ZipCode,
    AddressType AddressType,
    bool IsDefault
);
