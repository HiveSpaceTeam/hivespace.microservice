using System;

namespace HiveSpace.UserService.Api.Pages.Account.Login;

public class ViewModel
{
    public bool AllowRememberLogin { get; set; } = true;
    public bool EnableLocalLogin { get; set; } = true;

    public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = Enumerable.Empty<ExternalProvider>();
    public IEnumerable<ExternalProvider> VisibleExternalProviders => ExternalProviders.Where(x => !string.IsNullOrWhiteSpace(x.DisplayName));

    // Holds a single external provider error message (e.g. failed Google login)
    public string? ExternalErrorMessage { get; set; }

    public bool IsExternalLoginOnly => EnableLocalLogin == false && ExternalProviders?.Count() == 1;
    public string? ExternalLoginScheme => IsExternalLoginOnly ? ExternalProviders?.SingleOrDefault()?.AuthenticationScheme : null;
    
    public string? ClientId { get; set; }
    public bool IsAdminPortalClient => string.Equals(ClientId, "adminportal", StringComparison.OrdinalIgnoreCase);

    public class ExternalProvider
    {
        public ExternalProvider(string authenticationScheme, string? displayName = null)
        {
            AuthenticationScheme = authenticationScheme;
            DisplayName = displayName;
        }

        public string? DisplayName { get; set; }
        public string AuthenticationScheme { get; set; }
    }
}
