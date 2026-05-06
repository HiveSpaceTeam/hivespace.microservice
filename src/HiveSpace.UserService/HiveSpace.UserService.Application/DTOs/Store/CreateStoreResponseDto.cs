namespace HiveSpace.UserService.Application.DTOs.Store;

public record CreateStoreResponseDto(
    Guid StoreId,
    string StoreName,
    string? StoreDescription,
    string StoreLogoFileId,
    string StoreAddress
);