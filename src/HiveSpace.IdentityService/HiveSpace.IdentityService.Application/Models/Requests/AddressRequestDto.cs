namespace HiveSpace.IdentityService.Application.Models.Requests;

public record AddressRequestDto(
    string FullName = "",
    string Street = "",
    string Ward = "",
    string District = "",
    string Province = "",
    string Country = "",
    string ZipCode = "",
    string PhoneNumber = "",
    bool IsDefault = false
);
