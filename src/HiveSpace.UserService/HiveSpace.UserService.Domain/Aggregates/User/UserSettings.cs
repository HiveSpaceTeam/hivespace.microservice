using HiveSpace.Domain.Shared.Entities;
using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Domain.Aggregates.User;

public class UserSettings : ValueObject
{
    public Theme Theme { get; }
    public Culture Culture { get; }

    private UserSettings()
    {
        Theme = Theme.Light;
        Culture = Culture.Vi;
    }

    internal UserSettings(Theme theme, Culture culture)
    {
        Theme = theme;
        Culture = culture;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Theme;
        yield return Culture;
    }

    public UserSettings WithTheme(Theme theme)
    {
        return new UserSettings(theme, this.Culture);
    }

    public UserSettings WithCulture(Culture culture)
    {
        return new UserSettings(this.Theme, culture);
    }

    public static UserSettings Default => new UserSettings(Theme.Light, Culture.En);
}