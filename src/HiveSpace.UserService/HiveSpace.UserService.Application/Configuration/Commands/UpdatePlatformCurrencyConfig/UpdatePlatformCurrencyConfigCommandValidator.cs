using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Domain.Aggregates.Configuration;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Application.Configuration.Commands.UpdatePlatformCurrencyConfig;

public class UpdatePlatformCurrencyConfigCommandValidator : AbstractValidator<UpdatePlatformCurrencyConfigCommand>
{
    public UpdatePlatformCurrencyConfigCommandValidator()
    {
        RuleFor(x => x.DefaultCurrencyCode)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UpdatePlatformCurrencyConfigCommand.DefaultCurrencyCode)))
            .Must(BeSupportedCurrencyCode)
            .WithState(_ => new Error(UserDomainErrorCode.PlatformCurrencyUnsupported, nameof(UpdatePlatformCurrencyConfigCommand.DefaultCurrencyCode)));

        RuleFor(x => x.Version)
            .GreaterThan(0)
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(UpdatePlatformCurrencyConfigCommand.Version)));

        RuleFor(x => x.Currencies)
            .NotEmpty()
            .WithState(_ => new Error(UserDomainErrorCode.PlatformCurrencySetRequired, nameof(UpdatePlatformCurrencyConfigCommand.Currencies)))
            .Must(HaveExactSupportedCurrencySet)
            .WithState(_ => new Error(UserDomainErrorCode.PlatformCurrencySetRequired, nameof(UpdatePlatformCurrencyConfigCommand.Currencies)))
            .Must(HaveUniqueCurrencyCodes)
            .WithState(_ => new Error(UserDomainErrorCode.PlatformCurrencyUnsupported, nameof(UpdatePlatformCurrencyConfigCommand.Currencies)))
            .Must(HaveOnlySupportedCurrencyCodes)
            .WithState(_ => new Error(UserDomainErrorCode.PlatformCurrencyUnsupported, nameof(UpdatePlatformCurrencyConfigCommand.Currencies)))
            .Must(HaveAtLeastOneEnabledCurrency)
            .WithState(_ => new Error(UserDomainErrorCode.PlatformCurrencySetRequired, nameof(UpdatePlatformCurrencyConfigCommand.Currencies)));

        RuleForEach(x => x.Currencies).ChildRules(currency =>
        {
            currency.RuleFor(x => x.CurrencyCode)
                .NotEmpty()
                .WithState(_ => new Error(UserDomainErrorCode.PlatformCurrencyUnsupported, nameof(PlatformCurrencyConfigItemRequest.CurrencyCode)));
        });

        RuleFor(x => x)
            .Must(HaveEnabledDefaultCurrency)
            .WithState(_ => new Error(UserDomainErrorCode.PlatformDefaultCurrencyDisabled, nameof(UpdatePlatformCurrencyConfigCommand.DefaultCurrencyCode)));
    }

    private static bool BeSupportedCurrencyCode(string currencyCode)
        => !string.IsNullOrWhiteSpace(currencyCode)
           && PlatformCurrency.GetSupportedCodes().Contains(currencyCode.ToUpperInvariant(), StringComparer.OrdinalIgnoreCase);

    private static bool HaveExactSupportedCurrencySet(IReadOnlyCollection<PlatformCurrencyConfigItemRequest>? currencies)
        => currencies is not null && currencies.Count == PlatformCurrency.GetSupportedCodes().Count;

    private static bool HaveUniqueCurrencyCodes(IReadOnlyCollection<PlatformCurrencyConfigItemRequest>? currencies)
        => currencies is not null
           && currencies.Select(x => NormalizeCode(x.CurrencyCode)).Distinct(StringComparer.OrdinalIgnoreCase).Count() == currencies.Count;

    private static bool HaveOnlySupportedCurrencyCodes(IReadOnlyCollection<PlatformCurrencyConfigItemRequest>? currencies)
        => currencies is not null
           && currencies.All(x => BeSupportedCurrencyCode(x.CurrencyCode));

    private static bool HaveAtLeastOneEnabledCurrency(IReadOnlyCollection<PlatformCurrencyConfigItemRequest>? currencies)
        => currencies is not null && currencies.Any(x => x.IsEnabled);

    private static bool HaveEnabledDefaultCurrency(UpdatePlatformCurrencyConfigCommand command)
        => BeSupportedCurrencyCode(command.DefaultCurrencyCode)
           && command.Currencies is not null
           && command.Currencies.Any(x =>
               x.IsEnabled &&
               x.CurrencyCode.Equals(command.DefaultCurrencyCode, StringComparison.OrdinalIgnoreCase));

    private static string NormalizeCode(string? currencyCode)
        => currencyCode?.ToUpperInvariant() ?? string.Empty;
}
