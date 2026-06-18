using HiveSpace.Application.Shared.Commands;
using HiveSpace.UserService.Application.Stores.Dtos;

namespace HiveSpace.UserService.Application.Stores.Commands.CreateStore;

public record CreateStoreCommand(CreateStoreRequestDto Payload) : ICommand<CreateStoreResponseDto>;
