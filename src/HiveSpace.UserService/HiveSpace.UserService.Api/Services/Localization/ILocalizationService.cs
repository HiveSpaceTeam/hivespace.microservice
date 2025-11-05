using System.Globalization;

namespace HiveSpace.UserService.Api.Services.Localization;

/// <summary>
/// Service interface for handling localization of UI strings
/// Supports JSON-based resources with culture fallback
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Get localized string by key using current thread culture
    /// </summary>
    /// <param name="key">Resource key (e.g., "pages.login.title")</param>
    /// <returns>Localized string or key if not found</returns>
    string GetString(string key);

    /// <summary>
    /// Get localized string by key with specific culture
    /// </summary>
    /// <param name="key">Resource key</param>
    /// <param name="culture">Target culture (e.g., "vi", "en")</param>
    /// <returns>Localized string or key if not found</returns>
    string GetString(string key, string? culture);

    /// <summary>
    /// Get localized string with format arguments
    /// </summary>
    /// <param name="key">Resource key</param>
    /// <param name="args">Format arguments</param>
    /// <returns>Formatted localized string</returns>
    string GetString(string key, params object[] args);

    /// <summary>
    /// Get localized string with format arguments and specific culture
    /// </summary>
    /// <param name="key">Resource key</param>
    /// <param name="culture">Target culture</param>
    /// <param name="args">Format arguments</param>
    /// <returns>Formatted localized string</returns>
    string GetString(string key, string? culture, params object[] args);

    /// <summary>
    /// Get all supported culture codes
    /// </summary>
    /// <returns>Array of supported culture codes (e.g., ["vi", "en"])</returns>
    string[] GetSupportedCultures();

    /// <summary>
    /// Get current culture code
    /// </summary>
    /// <returns>Current culture code (e.g., "vi")</returns>
    string GetCurrentCulture();

    /// <summary>
    /// Check if a culture is supported
    /// </summary>
    /// <param name="culture">Culture code to check</param>
    /// <returns>True if supported</returns>
    bool IsCultureSupported(string culture);

    /// <summary>
    /// Get default culture code
    /// </summary>
    /// <returns>Default culture code</returns>
    string GetDefaultCulture();
}