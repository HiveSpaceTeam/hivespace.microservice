using HiveSpace.UserService.Application.DTOs.Store;

namespace HiveSpace.UserService.Application.Interfaces.Services;

public interface IStoreService
{
    Task<CreateStoreResponseDto> CreateStoreAsync(CreateStoreRequestDto request, CancellationToken cancellationToken = default);
}