using HiveSpace.Application.Shared.Queries;
using HiveSpace.UserService.Application.Configuration.Dtos;

namespace HiveSpace.UserService.Application.Configuration.Queries.GetActivePlatformCurrencyConfig;

public record GetActivePlatformCurrencyConfigQuery() : IQuery<PlatformCurrencyConfigDto>;
