using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.IdentityService.Core.Features.Roles.Commands.AssignSellerRole;

public record AssignSellerRoleCommand(Guid UserId, Guid StoreId) : ICommand;
