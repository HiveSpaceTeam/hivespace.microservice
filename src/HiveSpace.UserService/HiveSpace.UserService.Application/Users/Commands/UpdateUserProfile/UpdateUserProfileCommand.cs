using HiveSpace.Application.Shared.Commands;
using HiveSpace.UserService.Application.Users.Dtos;

namespace HiveSpace.UserService.Application.Users.Commands.UpdateUserProfile;

public record UpdateUserProfileCommand(UpdateUserProfileRequestDto Payload) : ICommand;
