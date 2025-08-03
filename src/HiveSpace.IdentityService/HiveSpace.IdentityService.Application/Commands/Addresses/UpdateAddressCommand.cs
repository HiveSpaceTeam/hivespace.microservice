using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.IdentityService.Application.Commands.Addresses;

public record UpdateAddressCommand(
    Guid AddressId,
    string FullName,
    string Street,
    string Ward,
    string District,
    string Province,
    string Country,
    string ZipCode,
    string PhoneNumber
) : ICommand<Models.Responses.AddressResponseDto>
{
    public static UpdateAddressCommand FromDto(Guid addressId, Models.Requests.AddressRequestDto dto) =>
        new(
            addressId,
            dto.FullName,
            dto.Street,
            dto.Ward,
            dto.District,
            dto.Province,
            dto.Country,
            dto.ZipCode,
            dto.PhoneNumber
        );
}
