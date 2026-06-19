using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Application.Stores.Dtos;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Domain.Services;

namespace HiveSpace.UserService.Application.Stores.Commands.CreateStore;

public class CreateStoreCommandHandler(
    IUserContext userContext,
    StoreManager storeManager,
    IStoreRepository storeRepository,
    IStoreEventPublisher storeEventPublisher)
    : ICommandHandler<CreateStoreCommand, CreateStoreResponseDto>
{
    public async Task<CreateStoreResponseDto> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        var payload = request.Payload;
        var registration = await storeManager.RegisterStoreAsync(
            payload.StoreName,
            payload.Description,
            payload.StoreLogoFileId,
            payload.Address,
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
