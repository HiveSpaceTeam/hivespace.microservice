using HiveSpace.Application.Shared.Commands;
using HiveSpace.NotificationService.Core.DomainModels;

namespace HiveSpace.NotificationService.Core.Features.Preferences.Commands.UpsertGroupPreference;

public record UpsertGroupPreferenceCommand(NotificationChannel Channel, string EventGroup, bool Enabled) : ICommand;
