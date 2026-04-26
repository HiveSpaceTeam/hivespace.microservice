using HiveSpace.Application.Shared.Commands;
using HiveSpace.NotificationService.Core.DomainModels;

namespace HiveSpace.NotificationService.Core.Features.Preferences.Commands.UpsertChannelPreference;

public record UpsertChannelPreferenceCommand(NotificationChannel Channel, bool Enabled) : ICommand;
