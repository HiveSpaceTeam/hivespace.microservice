using System.Globalization;
using System.Text.Json;
using System.Collections.Concurrent;

namespace HiveSpace.UserService.Api.Services.Localization;

/// <summary>
/// JSON-based localization service with caching and fallback support
/// Supports hierarchical keys (e.g., "pages.login.title")
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalizationService> _logger;
    private readonly ConcurrentDictionary<string, Dictionary<string, object>> _cache = new();
    
    // Configuration
    private const string DefaultCulture = "vi";
    private static readonly string[] SupportedCultures = { "vi", "en" };
    private const string ResourcePath = "wwwroot/localization";

    public LocalizationService(IWebHostEnvironment environment, ILogger<LocalizationService> logger)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Pre-load all supported cultures
        LoadAllCultures();
    }

    public string GetString(string key)
    {
        return GetString(key, GetCurrentCulture());
    }

    public string GetString(string key, string? culture)
    {
        if (string.IsNullOrEmpty(key))
            return key ?? string.Empty;

        culture ??= GetCurrentCulture();
        
        // Ensure culture is supported, fallback to default
        if (!IsCultureSupported(culture))
        {
            _logger.LogWarning("Unsupported culture '{Culture}', falling back to '{DefaultCulture}'", culture, DefaultCulture);
            culture = DefaultCulture;
        }

        var resources = GetCultureResources(culture);
        var value = GetNestedValue(resources, key);
        
        if (value != null)
            return value.ToString() ?? key;

        // Fallback to default culture if not found
        if (culture != DefaultCulture)
        {
            _logger.LogDebug("Key '{Key}' not found in culture '{Culture}', falling back to '{DefaultCulture}'", key, culture, DefaultCulture);
            var defaultResources = GetCultureResources(DefaultCulture);
            var defaultValue = GetNestedValue(defaultResources, key);
            return defaultValue?.ToString() ?? key;
        }

        _logger.LogWarning("Localization key '{Key}' not found in any culture", key);
        return key;
    }

    public string GetString(string key, params object[] args)
    {
        return GetString(key, GetCurrentCulture(), args);
    }

    public string GetString(string key, string? culture, params object[] args)
    {
        var template = GetString(key, culture);

        if (args?.Length > 0)
        {
            try
            {
                var cultureName = culture;
                if (string.IsNullOrEmpty(cultureName) || !IsCultureSupported(cultureName))
                {
                    cultureName = GetDefaultCulture();
                }

                CultureInfo cultureInfo;
                try
                {
                    cultureInfo = CultureInfo.GetCultureInfo(cultureName);
                }
                catch (CultureNotFoundException)
                {
                    _logger.LogWarning("Requested culture '{Culture}' is not recognized by .NET, falling back to default/invariant", cultureName);
                    try
                    {
                        cultureInfo = CultureInfo.GetCultureInfo(GetDefaultCulture());
                    }
                    catch (CultureNotFoundException)
                    {
                        cultureInfo = CultureInfo.InvariantCulture;
                    }
                }

                return string.Format(cultureInfo, template, args);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Failed to format string template '{Template}' with args for key '{Key}'", template, key);
                return template;
            }
        }

        return template;
    }

    public string[] GetSupportedCultures()
    {
        return SupportedCultures.ToArray();
    }

    public string GetCurrentCulture()
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
    }

    public bool IsCultureSupported(string culture)
    {
        return !string.IsNullOrEmpty(culture) && 
               SupportedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase);
    }

    public string GetDefaultCulture()
    {
        return DefaultCulture;
    }

    /// <summary>
    /// Load all culture resources at startup for better performance
    /// </summary>
    private void LoadAllCultures()
    {
        foreach (var culture in SupportedCultures)
        {
            try
            {
                LoadCultureResources(culture);
                _logger.LogInformation("Loaded localization resources for culture '{Culture}'", culture);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load localization resources for culture '{Culture}'", culture);
            }
        }
    }

    /// <summary>
    /// Get cached culture resources or load from file
    /// </summary>
    private Dictionary<string, object> GetCultureResources(string culture)
    {
        return _cache.GetOrAdd(culture, LoadCultureResources);
    }

    /// <summary>
    /// Load culture resources from JSON file
    /// </summary>
    private Dictionary<string, object> LoadCultureResources(string culture)
    {
        var filePath = Path.Combine(_environment.ContentRootPath, ResourcePath, $"{culture}.json");
        
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Localization file not found: {FilePath}", filePath);
            return new Dictionary<string, object>();
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var resources = JsonSerializer.Deserialize<Dictionary<string, object>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return resources ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse localization file: {FilePath}", filePath);
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Get nested value from dictionary using dot notation (e.g., "pages.login.title")
    /// </summary>
    private static object? GetNestedValue(Dictionary<string, object> resources, string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        var keys = key.Split('.');
        object? current = resources;

        foreach (var k in keys)
        {
            if (current is Dictionary<string, object> dict && dict.ContainsKey(k))
            {
                current = dict[k];
            }
            else if (current is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(k, out var property))
                {
                    current = property;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        return current;
    }
}