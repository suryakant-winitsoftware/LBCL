using System.Text.Json;
using Winit.Modules.Base.BL;

namespace WINITMobile.Services;

/// <summary>
/// Mobile-specific implementation of ILocalStorageService that uses SecureStorage for sensitive data
/// and preferences for non-sensitive data, maintaining full API compatibility.
/// </summary>
public class MobileLocalStorageService : ILocalStorageService
{
    private readonly SecureStorageHelper _secureStorageHelper;

    public MobileLocalStorageService(SecureStorageHelper secureStorageHelper)
    {
        _secureStorageHelper = secureStorageHelper ?? throw new ArgumentNullException(nameof(secureStorageHelper));
    }

    /// <summary>
    /// Gets an item from storage, automatically using secure storage for sensitive keys.
    /// </summary>
    /// <typeparam name="T">Type of the item to retrieve</typeparam>
    /// <param name="key">Storage key</param>
    /// <returns>Item value or default if not found</returns>
    public async Task<T?> GetItem<T>(string key)
    {
        try
        {
            string? jsonValue;

            // Decision point: Check if key is sensitive using keyword-based detection
            if (IsSensitiveKey(key))
            {
                // Route to SecureStorage (hardware-backed encryption)
                jsonValue = await _secureStorageHelper.GetSecureStringAsync(key, string.Empty);
            }
            else
            {
                // Route to regular Preferences (plain text storage)
                jsonValue = _secureStorageHelper.GetStringFromPreferences(key, string.Empty);
            }

            if (string.IsNullOrEmpty(jsonValue))
                return default(T);

            // Handle string type directly (most common case for API tokens, passwords)
            if (typeof(T) == typeof(string))
            {
                return (T)(object)jsonValue;
            }

            // Deserialize other types (user objects, settings, etc.)
            return JsonSerializer.Deserialize<T>(jsonValue);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting item '{key}': {ex.Message}");
            return default(T);
        }
    }

    /// <summary>
    /// Sets an item in storage, automatically using secure storage for sensitive keys.
    /// </summary>
    /// <typeparam name="T">Type of the item to store</typeparam>
    /// <param name="key">Storage key</param>
    /// <param name="value">Value to store</param>
    public async Task SetItem<T>(string key, T value)
    {
        try
        {
            if (value == null)
            {
                await RemoveItem(key);
                return;
            }

            string jsonValue;

            // Handle string type directly (most common case for tokens, passwords)
            if (typeof(T) == typeof(string))
            {
                jsonValue = value as string ?? string.Empty;
            }
            else
            {
                jsonValue = JsonSerializer.Serialize(value);
            }

            // Decision point: Check if key is sensitive using keyword-based detection
            if (IsSensitiveKey(key))
            {
                // Route to SecureStorage (hardware-backed encryption)
                await _secureStorageHelper.SaveSecureStringAsync(key, jsonValue);
            }
            else
            {
                // Route to regular Preferences (plain text storage)
                _secureStorageHelper.SaveStringToPreference(key, jsonValue);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting item '{key}': {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Removes an item from storage, checking both secure storage and preferences.
    /// </summary>
    /// <param name="key">Storage key to remove</param>
    public async Task RemoveItem(string key)
    {
        try
        {
            // Decision point: Check if key is sensitive using keyword-based detection
            if (IsSensitiveKey(key))
            {
                // Remove from SecureStorage
                await _secureStorageHelper.RemoveSecureKeyAsync(key);
            }
            else
            {
                // Remove from regular Preferences
                _secureStorageHelper.RemoveKey(key);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing item '{key}': {ex.Message}");
            // Don't throw on removal errors to maintain app stability
        }
    }

    /// <summary>
    /// 🔍 DECISION-MAKING METHOD: Determines if a key contains sensitive data using keyword-based detection.
    /// 
    /// This method scans the key name for sensitive keywords and patterns to automatically
    /// route data to secure storage vs. regular preferences.
    /// 
    /// SENSITIVE (SecureStorage): password, token, offline, remembered, firebase
    /// NON-SENSITIVE (Preferences): settings, language, theme, lastlogin, version
    /// </summary>
    /// <param name="key">Key to analyze</param>
    /// <returns>True if key should use SecureStorage, False if regular Preferences</returns>
    private static bool IsSensitiveKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        // Convert to lowercase for case-insensitive comparison
        var lowerKey = key.ToLowerInvariant();

        // 🔐 SENSITIVE KEYWORDS - These go to SecureStorage (hardware-backed encryption)
        var sensitiveKeywords = new[]
        {
            "password",      // Any password-related data
            "token",         // API tokens, JWT tokens, refresh tokens
            "tokendata",         // API tokens, JWT tokens, refresh tokens
            "offline",       // Offline login credentials
            "remembered",    // Remember me credentials
            "firebase",      // Firebase keys and tokens
            "auth",          // Authentication data
            "credential",    // Any credential data
            "secret",        // Secret keys
            "key"            // Generic key storage
        };

        // Check if the key contains any sensitive keywords
        return sensitiveKeywords.Any(keyword => lowerKey.Contains(keyword));
    }

    /// <summary>
    /// Gets information about the current storage configuration.
    /// </summary>
    /// <returns>Storage configuration details</returns>
    public string GetStorageInfo()
    {
        try
        {
            // Check if SecureStorage is available on this platform
            var isSecureAvailable = true;
            try
            {
                // Test SecureStorage availability
                _ = SecureStorage.GetAsync("test_availability_check").Result;
            }
            catch
            {
                isSecureAvailable = false;
            }

            return isSecureAvailable 
                ? "SecureStorage Available (Hardware-backed encryption)" 
                : "SecureStorage Unavailable (Fallback to Preferences)";
        }
        catch
        {
            return "Storage info unavailable";
        }
    }
} 