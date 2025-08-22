using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;

namespace HiveSpace.UserService.Domain.Services;

/// <summary>
/// Domain service for managing store registration and related business operations.
/// Enforces domain rules around store creation and ownership.
/// </summary>
public class StoreManager : IDomainService
{
    private readonly IStoreRepository _storeRepository;
    private readonly IUserRepository _userRepository;
    
    public StoreManager(
        IStoreRepository storeRepository,
        IUserRepository userRepository)
    {
        _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }
    
    /// <summary>
    /// Registers a new store with the provided details.
    /// </summary>
    /// <param name="name">Store name</param>
    /// <param name="description">Store description</param>
    /// <param name="ownerId">ID of the user who will own the store</param>
    /// <param name="phoneNumber">Store contact phone number</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The newly created store</returns>
    /// <exception cref="ArgumentException">Thrown when store information is invalid</exception>
    /// <exception cref="UserNotFoundException">Thrown when the owner user is not found</exception>
    /// <exception cref="UserInactiveException">Thrown when the owner user is inactive</exception>
    /// <exception cref="StoreAlreadyExistsException">Thrown when a store with the name already exists</exception>
    
    /// <summary>
    /// Validates the owner for store creation
    /// </summary>
    private async Task<User> ValidateStoreOwnerAsync(
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        if (ownerId == Guid.Empty)
            throw new InvalidUserIdException();
            
        var owner = await _userRepository.GetByIdAsync(ownerId) ?? throw new UserNotFoundException();
        
        if (owner.Status != UserStatus.Active)
            throw new UserInactiveException();
            
        return owner;
    }

    public async Task<Store> RegisterStoreAsync(
        string name,
        string description,
        Guid ownerId,
        PhoneNumber phoneNumber,
        CancellationToken cancellationToken = default)
    {
        // Validate input parameters
        if (!ValidateStoreInformation(name, description))
            throw new InvalidStoreInformationException();
        
        // Validate owner exists and is active
        await ValidateStoreOwnerAsync(ownerId, cancellationToken);
        
        // Check if user can own more stores (max 1 store per user)
        if (!await CanUserCreateStoreAsync(ownerId, cancellationToken))
            throw new UserStoreExistsException();
        
        // Check if store with name already exists (case-insensitive)
        if (!await IsStoreNameAvailableAsync(name, cancellationToken))
            throw new StoreAlreadyExistsException();
        
        // Create new store using internal factory method
        var store = Store.Create(name.Trim(), description.Trim(), phoneNumber, ownerId);
        
        return store;
    }
    
    /// <summary>
    /// Checks if a store name is available for registration.
    /// </summary>
    /// <param name="name">The store name to check</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>True if the name is available, false otherwise</returns>
    private async Task<bool> IsStoreNameAvailableAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
            
        // Use the more efficient exists check if available
        return !await _storeRepository.StoreNameExistsAsync(name.Trim());
    }

    
    /// <summary>
    /// Validates store information before registration.
    /// </summary>
    /// <param name="name">Store name</param>
    /// <param name="description">Store description</param>
    /// <returns>True if all information is valid</returns>
    private bool ValidateStoreInformation(string name, string description)
    {
        // Name validation
        if (string.IsNullOrWhiteSpace(name))
            return false;
            
        var trimmedName = name.Trim();
        if (trimmedName.Length < 2 || trimmedName.Length > 100)
            return false;
        
        // Description validation
        if (string.IsNullOrWhiteSpace(description))
            return false;
            
        var trimmedDescription = description.Trim();
        if (trimmedDescription.Length < 10 || trimmedDescription.Length > 500)
            return false;
        
        // Additional business rules
        if (ContainsInvalidCharacters(trimmedName))
            return false;
        
        return true;
    }
    
    /// <summary>
    /// Checks if a user can create a store.
    /// </summary>
    /// <param name="userId">The user ID to check</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>True if the user can create a store, false otherwise</returns>
    private async Task<bool> CanUserCreateStoreAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Business rule: Each user can only own at most 1 store
        var userStore = await _storeRepository.GetByOwnerIdAsync(userId);
        return userStore is null; // User can register only if they don't have any store
    }

    
    /// <summary>
    /// Checks if the store name contains invalid characters.
    /// </summary>
    /// <param name="name">Store name to validate</param>
    /// <returns>True if contains invalid characters, false otherwise</returns>
    private static bool ContainsInvalidCharacters(string name)
    {
        // Define invalid characters for store names
        var invalidChars = new[] { '<', '>', '"', '|', '\\', '/', '*', '?', ':' };
        return name.Any(c => invalidChars.Contains(c));
    }
}
