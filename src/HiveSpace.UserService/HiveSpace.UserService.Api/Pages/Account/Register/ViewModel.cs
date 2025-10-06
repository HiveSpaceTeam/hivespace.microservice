namespace HiveSpace.UserService.Api.Pages.Account.Register;

public class ViewModel
{
    public bool EnableLocalRegistration { get; set; } = true;

    public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = Enumerable.Empty<ExternalProvider>();
    public IEnumerable<ExternalProvider> VisibleExternalProviders => ExternalProviders.Where(x => !string.IsNullOrWhiteSpace(x.DisplayName));

    // Holds a single external provider error message (e.g. failed Google registration)
    public string? ExternalErrorMessage { get; set; }

    public bool IsExternalRegistrationOnly => EnableLocalRegistration == false && ExternalProviders?.Count() == 1;
    public string? ExternalRegistrationScheme => IsExternalRegistrationOnly ? ExternalProviders?.SingleOrDefault()?.AuthenticationScheme : null;

    public class ExternalProvider
    {
        public ExternalProvider(string authenticationScheme, string? displayName = null)
        {
            AuthenticationScheme = authenticationScheme;
            DisplayName = displayName;
        }

        public string AuthenticationScheme { get; set; }
        public string? DisplayName { get; set; }
    }
}