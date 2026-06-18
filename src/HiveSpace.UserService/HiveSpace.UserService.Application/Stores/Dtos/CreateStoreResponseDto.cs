namespace HiveSpace.UserService.Application.Stores.Dtos;

public record CreateStoreResponseDto(
    Guid StoreId,
    string StoreName,
    string? StoreDescription,
    string StoreLogoFileId,
    string StoreAddress
);
