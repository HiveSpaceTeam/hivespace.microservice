namespace HiveSpace.UserService.Api.Pages.Account.Logout;

public static class LogoutOptions
{
    // ShowLogoutPrompt=false disables the prompt, but logout must only occur on POST (not GET).
    public static readonly bool ShowLogoutPrompt = false;
    public static readonly bool AutomaticRedirectAfterSignOut = true;
}
