using HiveSpace.Application.Shared.Commands;
using HiveSpace.IdentityService.Core.Features.ExternalLogins.Dtos;

namespace HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.CompleteGoogleCallback;

public record CompleteGoogleCallbackCommand(
    string App,
    string? ReturnUrl,
    string? Culture) : ICommand<GoogleCallbackResult>;
