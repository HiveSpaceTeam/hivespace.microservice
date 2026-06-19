using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Enums;
using HiveSpace.UserService.Domain.Exceptions;

namespace HiveSpace.UserService.Application.Users.Dtos;

internal static class UserSettingValues
{
    public const string LightTheme = "light";
    public const string DarkTheme = "dark";
    public const string VietnameseCulture = "vi";
    public const string EnglishCulture = "en";

    public static readonly string[] SupportedThemes = [LightTheme, DarkTheme];
    public static readonly string[] SupportedCultures = [VietnameseCulture, EnglishCulture];

    public static string ToApiValue(Theme theme) => theme switch
    {
        Theme.Light => LightTheme,
        Theme.Dark => DarkTheme,
        _ => throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(theme))
    };

    public static string ToApiValue(Culture culture) => culture switch
    {
        Culture.Vi => VietnameseCulture,
        Culture.En => EnglishCulture,
        _ => throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(culture))
    };

    public static Theme ToTheme(string theme) => theme switch
    {
        LightTheme => Theme.Light,
        DarkTheme => Theme.Dark,
        _ => throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(theme))
    };

    public static Culture ToCulture(string culture) => culture switch
    {
        VietnameseCulture => Culture.Vi,
        EnglishCulture => Culture.En,
        _ => throw new InvalidFieldException(UserDomainErrorCode.InvalidField, nameof(culture))
    };
}
