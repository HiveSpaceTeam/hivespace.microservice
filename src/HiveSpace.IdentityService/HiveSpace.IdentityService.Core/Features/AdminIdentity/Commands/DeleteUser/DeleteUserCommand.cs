using HiveSpace.Application.Shared.Commands;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Dtos;

namespace HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.DeleteUser;

public record DeleteUserCommand(Guid UserId, string? DeletedBy) : ICommand<DeleteIdentityUserResult>;
