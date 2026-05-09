namespace HiveSpace.UserService.Application.DTOs.Store;

public record CreateStoreRequestDto(
    string StoreName,
    string? Description,
    string StoreLogoFileId,
    string Address
);