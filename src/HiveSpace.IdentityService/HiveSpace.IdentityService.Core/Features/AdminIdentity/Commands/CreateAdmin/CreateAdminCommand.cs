using HiveSpace.Application.Shared.Commands;
using HiveSpace.IdentityService.Core.Features.AdminIdentity.Dtos;

namespace HiveSpace.IdentityService.Core.Features.AdminIdentity.Commands.CreateAdmin;

public record CreateAdminCommand(
    string Email,
    string Password,
    string FullName,
    string ConfirmPassword,
    bool IsSystemAdmin) : ICommand<CreateAdminResult>;
