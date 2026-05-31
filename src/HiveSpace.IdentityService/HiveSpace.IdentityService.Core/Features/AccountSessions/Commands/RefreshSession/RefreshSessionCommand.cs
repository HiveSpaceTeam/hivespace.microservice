using HiveSpace.Application.Shared.Commands;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Dtos;

namespace HiveSpace.IdentityService.Core.Features.AccountSessions.Commands.RefreshSession;

public record RefreshSessionCommand(string App) : ICommand<SessionResponse>;
