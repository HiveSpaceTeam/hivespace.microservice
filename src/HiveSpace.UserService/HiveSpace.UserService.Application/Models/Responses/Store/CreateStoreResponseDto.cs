using System;

namespace HiveSpace.UserService.Application.Models.Responses.Store;

public record CreateStoreResponseDto(
    Guid StoreId,
    string StoreName,
    string? StoreDescription,
    string StoreLogo,
    string StoreAddress
);