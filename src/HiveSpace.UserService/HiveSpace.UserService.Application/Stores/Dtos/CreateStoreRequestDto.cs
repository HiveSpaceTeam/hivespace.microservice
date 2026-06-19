namespace HiveSpace.UserService.Application.Stores.Dtos;

public record CreateStoreRequestDto(
    string StoreName,
    string? Description,
    string StoreLogoFileId,
    string Address
);
