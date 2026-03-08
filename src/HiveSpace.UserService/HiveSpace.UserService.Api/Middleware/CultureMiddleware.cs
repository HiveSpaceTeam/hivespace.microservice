using System.Globalization;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.UserService.Api.Middleware;

/// <summary>
/// Middleware to handle culture/language switching based on cookies and query parameters
/// Supports Identity Server OAuth flow with culture preservation
/// </summary>
public class CultureMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CultureMiddleware> _logger;

    // Configuration
    private const string CultureCookieName = "culture";
    private const string CultureQueryParam = "culture";
    private const string DefaultCulture = "vi";
    private static readonly string[] SupportedCultures = { "vi", "en" };

    public CultureMiddleware(RequestDelegate next, ILogger<CultureMiddleware> logger)
    {
        _next = next ?? throw new InvalidFieldException(DomainErrorCode.ArgumentNull, nameof(next));
        _logger = logger ?? throw new InvalidFieldException(DomainErrorCode.ArgumentNull, nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var culture = DetermineCulture(context);
            SetCulture(culture);
            _logger.LogDebug("Processing request with culture: {Culture}", culture);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CultureMiddleware, falling back to default culture");
            SetCulture(DefaultCulture);
        }

        await _next(context);
    }

    /// <summary>
    /// Determine culture from storage (cookie), URL parameter, or default
    /// Priority: 1. Storage (cookie) 2. URL parameter 3. Default
    /// </summary>
    private string DetermineCulture(HttpContext context)
    {
        // 1. Check storage (represented by culture cookie set from frontend)
        if (context.Request.Cookies.TryGetValue(CultureCookieName, out var storedCulture))
        {
            if (IsCultureSupported(storedCulture))
            {
                _logger.LogDebug("Culture determined from storage/cookie: {Culture}", storedCulture);
                return storedCulture;
            }
        }

        // 2. Check culture in ReturnUrl parameter (for OIDC flows)
        if (context.Request.Query.TryGetValue("ReturnUrl", out var returnUrl))
        {
            var cultureFromReturnUrl = ExtractCultureFromReturnUrl(returnUrl.ToString());
            if (!string.IsNullOrEmpty(cultureFromReturnUrl) && IsCultureSupported(cultureFromReturnUrl))
            {
                _logger.LogDebug("Culture determined from ReturnUrl parameter: {Culture}", cultureFromReturnUrl);
                return cultureFromReturnUrl;
            }
        }

        // 3. Default culture
        _logger.LogDebug("Using default culture: {Culture}", DefaultCulture);
        return DefaultCulture;
    }

    /// <summary>
    /// Set thread culture for localization
    /// </summary>
    private static void SetCulture(string culture)
    {
        var cultureInfo = new CultureInfo(culture);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
    }

    /// <summary>
    /// Check if culture is supported
    /// </summary>
    private static bool IsCultureSupported(string? culture)
    {
        return !string.IsNullOrEmpty(culture) && 
               SupportedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Extract culture from ReturnUrl parameter (handles URL-encoded values)
    /// Example: ReturnUrl=/connect/authorize/callback?...&amp;culture=en
    /// </summary>
    /// <param name="returnUrl">The ReturnUrl query parameter value</param>
    /// <returns>Culture code if found, null otherwise</returns>
    private string? ExtractCultureFromReturnUrl(string returnUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(returnUrl))
                return null;

            _logger.LogDebug("Extracting culture from ReturnUrl: {ReturnUrl}", returnUrl);

            // URL decode the return URL to get the actual parameters
            var decodedUrl = Uri.UnescapeDataString(returnUrl);
            _logger.LogDebug("Decoded ReturnUrl: {DecodedUrl}", decodedUrl);

            // Look for culture parameter in the decoded URL
            // It could be in format: ...&culture=en or ?culture=en
            var culturePattern = @"[?&]culture=([^&]+)";
            var match = System.Text.RegularExpressions.Regex.Match(decodedUrl, culturePattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (match.Success && match.Groups.Count > 1)
            {
                var culture = match.Groups[1].Value;
                _logger.LogDebug("Found culture in ReturnUrl: {Culture}", culture);
                return culture;
            }

            _logger.LogDebug("No culture parameter found in ReturnUrl");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract culture from ReturnUrl: {ReturnUrl}", returnUrl);
            return null;
        }
    }

    /// <summary>
    /// Get supported cultures for configuration
    /// </summary>
    public static string[] GetSupportedCultures() => SupportedCultures.ToArray();

    /// <summary>
    /// Get default culture for configuration
    /// </summary>
    public static string GetDefaultCulture() => DefaultCulture;
}