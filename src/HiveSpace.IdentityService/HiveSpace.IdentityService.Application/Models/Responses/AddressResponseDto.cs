namespace HiveSpace.IdentityService.Application.Models.Responses;

public record AddressResponseDto(
    Guid Id,
    string FullName,
    string Street,
    string Ward,
    string District,
    string Province,
    string Country,
    string? ZipCode,
    string? PhoneNumber,
    bool IsDefault,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
