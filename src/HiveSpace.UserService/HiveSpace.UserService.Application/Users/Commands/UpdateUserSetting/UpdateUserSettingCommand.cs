using HiveSpace.Application.Shared.Commands;
using HiveSpace.UserService.Application.Users.Dtos;

namespace HiveSpace.UserService.Application.Users.Commands.UpdateUserSetting;

public record UpdateUserSettingCommand(UpdateUserSettingRequestDto Payload) : ICommand;
