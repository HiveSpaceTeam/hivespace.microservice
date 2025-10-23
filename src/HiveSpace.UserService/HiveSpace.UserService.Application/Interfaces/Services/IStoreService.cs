using HiveSpace.UserService.Application.Models.Requests.Store;
using HiveSpace.UserService.Application.Models.Responses.Store;

namespace HiveSpace.UserService.Application.Interfaces.Services;

public interface IStoreService
{
    Task<CreateStoreResponseDto> CreateStoreAsync(CreateStoreRequestDto request, CancellationToken cancellationToken = default);
}