namespace HiveSpace.UserService.Application.Models.Requests.Store;

public record CreateStoreRequestDto(
    string StoreName,
    string? Description,
    string StoreLogoFileId,
    string Address
);