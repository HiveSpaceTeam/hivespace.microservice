using HiveSpace.Application.Shared.Commands;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Dtos;

namespace HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.SetUserStatus;

public record SetUserStatusCommand(Guid UserId, bool IsActive) : ICommand<SetIdentityStatusResult>;
