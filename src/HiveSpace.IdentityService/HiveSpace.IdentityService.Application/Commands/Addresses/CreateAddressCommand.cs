using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.IdentityService.Application.Commands.Addresses;

public record CreateAddressCommand(
    string FullName,
    string Street,
    string Ward,
    string District,
    string Province,
    string Country,
    string ZipCode,
    string PhoneNumber,
    bool IsDefault
) : ICommand<Models.Responses.AddressResponseDto>
{
    public static CreateAddressCommand FromDto(Models.Requests.AddressRequestDto dto) =>
        new(
            dto.FullName,
            dto.Street,
            dto.Ward,
            dto.District,
            dto.Province,
            dto.Country,
            dto.ZipCode,
            dto.PhoneNumber,
            dto.IsDefault
        );
}
