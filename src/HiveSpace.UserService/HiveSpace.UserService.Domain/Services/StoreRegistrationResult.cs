using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Domain.Services;

public sealed record StoreRegistrationResult(Store Store, User Owner);
