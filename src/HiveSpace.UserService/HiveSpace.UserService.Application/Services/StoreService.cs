using HiveSpace.Core.Contexts;
using HiveSpace.UserService.Application.DTOs.Store;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Application.Interfaces.Services;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Domain.Services;

namespace HiveSpace.UserService.Application.Services;

public class StoreService(
    IUserContext userContext,
    StoreManager storeManager,
    IStoreRepository storeRepository,
    IStoreEventPublisher storeEventPublisher)
    : IStoreService
{
    public async Task<CreateStoreResponseDto> CreateStoreAsync(CreateStoreRequestDto request, CancellationToken cancellationToken = default)
    {
        var registration = await storeManager.RegisterStoreAsync(
            request.StoreName,
            request.Description,
            request.StoreLogoFileId,
            request.Address,
            userContext.UserId,
            null,
            cancellationToken);

        storeRepository.Add(registration.Store);
        await storeEventPublisher.PublishStoreCreatedAsync(registration.Store, cancellationToken);
        await storeRepository.SaveChangesAsync(cancellationToken);

        return new CreateStoreResponseDto(
            registration.Store.Id,
            registration.Store.StoreName,
            registration.Store.Description,
            registration.Store.LogoFileId,
            registration.Store.Address);
    }
}
