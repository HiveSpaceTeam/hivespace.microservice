using HiveSpace.Application.Shared.Commands;
using HiveSpace.IdentityService.Core.Features.ExternalLogins.Dtos;

namespace HiveSpace.IdentityService.Core.Features.ExternalLogins.Commands.StartGoogleChallenge;

public record StartGoogleChallengeCommand(
    string App,
    string? ReturnUrl,
    string? Culture) : ICommand<GoogleChallengeResult>;
