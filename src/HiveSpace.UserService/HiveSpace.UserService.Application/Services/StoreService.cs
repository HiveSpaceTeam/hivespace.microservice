using HiveSpace.Core.Contexts;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Errors;
using HiveSpace.UserService.Application.Interfaces.Services;
using HiveSpace.UserService.Application.DTOs.Store;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Domain.Services;

namespace HiveSpace.UserService.Application.Services;

public class StoreService : IStoreService
{
    private readonly IUserContext _userContext;
    private readonly StoreManager _storeManager;
    private readonly IStoreRepository _storeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IStoreEventPublisher _storeEventPublisher;

    public StoreService(
        IUserContext userContext,
        StoreManager storeManager,
        IStoreRepository storeRepository,
        IUserRepository userRepository,
        IStoreEventPublisher storeEventPublisher)
    {
        _userContext = userContext ?? throw new InvalidFieldException(DomainErrorCode.ArgumentNull, nameof(userContext));
        _storeManager = storeManager ?? throw new InvalidFieldException(DomainErrorCode.ArgumentNull, nameof(storeManager));
        _storeRepository = storeRepository ?? throw new InvalidFieldException(DomainErrorCode.ArgumentNull, nameof(storeRepository));
        _userRepository = userRepository ?? throw new InvalidFieldException(DomainErrorCode.ArgumentNull, nameof(userRepository));
        _storeEventPublisher = storeEventPublisher ?? throw new InvalidFieldException(DomainErrorCode.ArgumentNull, nameof(storeEventPublisher));
    }
    
    public async Task<CreateStoreResponseDto> CreateStoreAsync(CreateStoreRequestDto request, CancellationToken cancellationToken = default)
    {
        var registration = await _storeManager.RegisterStoreAsync(
            request.StoreName,
            request.Description,
            request.StoreLogoFileId,
            request.Address,
            _userContext.UserId,
            null,
            cancellationToken);

        // Save through repository
        _storeRepository.Add(registration.Store);
        
        await _storeEventPublisher.PublishStoreCreatedAsync(registration.Store, cancellationToken);
        await _userRepository.UpdateUserAsync(registration.Owner, cancellationToken);
        return new CreateStoreResponseDto(
            registration.Store.Id,
            registration.Store.StoreName,
            registration.Store.Description,
            registration.Store.LogoUrl,
            registration.Store.Address);
    }
}
