namespace HiveSpace.OrderService.Application.DTOs;

public record ShippingAddressDto(
    string FullName,
    string PhoneNumber,
    string OtherDetails,
    string Street,
    string Ward,
    string District,
    string Province,
    string Country
);