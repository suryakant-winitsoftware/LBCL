using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;


namespace WINITMobile.Services;
/// <summary>
/// Hybrid storage helper that uses secure storage for sensitive data and preferences for non-sensitive data.
/// </summary>
public class SecureStorageHelper
{
    private readonly IPreferences _preferences;
    
    // Define which keys should use secure storage (sensitive data)
    private static readonly HashSet<string> SecureKeys = new()
    {
        // Offline login credentials - highly sensitive
        Winit.Modules.Base.Model.Constants.LocalStorageKeys.OfflineUsername,
        Winit.Modules.Base.Model.Constants.LocalStorageKeys.OfflinePasswordHash,
        Winit.Modules.Base.Model.Constants.LocalStorageKeys.OfflineUserData,
        
        // Remember me credentials - sensitive
        Winit.Modules.Base.Model.Constants.LocalStorageKeys.RememberedUsername,
        
        // Tokens - highly sensitive
        Winit.Modules.Base.Model.Constants.LocalStorageKeys.Token,
        Winit.Modules.Base.Model.Constants.LocalStorageKeys.TokenData,
        Winit.Modules.Base.Model.Constants.LocalStorageKeys.FirebaseKey,
    };

    public SecureStorageHelper()
    {
        _preferences = Preferences.Default;
    }

    #region Secure Storage Methods (for sensitive data)

    /// <summary>
    /// Saves sensitive string data to secure storage with encryption.
    /// </summary>
    /// <param name="key">Storage key</param>
    /// <param name="value">Value to store securely</param>
    public async Task SaveSecureStringAsync(string key, string value)
    {
        try
        {
            await SecureStorage.SetAsync(key, value);
        }
        catch (Exception ex)
        {
            // Log error and fallback to preferences as last resort
            Console.WriteLine($"Secure storage failed for key '{key}': {ex.Message}");
            // Note: In production, you might want to show an error to user instead of fallback
            _preferences.Set(key, value);
        }
    }

    /// <summary>
    /// Gets sensitive string data from secure storage.
    /// </summary>
    /// <param name="key">Storage key</param>
    /// <param name="defaultValue">Default value if not found</param>
    /// <returns>Securely stored value or default</returns>
    public async Task<string> GetSecureStringAsync(string key, string defaultValue = "")
    {
        try
        {
            var result = await SecureStorage.GetAsync(key);
            return result ?? defaultValue;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Secure storage retrieval failed for key '{key}': {ex.Message}");
            // Fallback to preferences
            return _preferences.Get(key, defaultValue);
        }
    }

    /// <summary>
    /// Removes sensitive data from secure storage.
    /// </summary>
    /// <param name="key">Key to remove</param>
    public async Task RemoveSecureKeyAsync(string key)
    {
        try
        {
            SecureStorage.Remove(key);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Secure storage removal failed for key '{key}': {ex.Message}");
            // Fallback to preferences removal
            _preferences.Remove(key);
        }
    }

    /// <summary>
    /// Removes all secure storage data (use with caution).
    /// </summary>
    public async Task ClearAllSecureDataAsync()
    {
        try
        {
            SecureStorage.RemoveAll();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Clear all secure storage failed: {ex.Message}");
        }
    }

    #endregion

    #region Hybrid Methods (auto-detect secure vs non-secure)

    /// <summary>
    /// Removes a key from either secure storage or preferences based on sensitivity.
    /// </summary>
    /// <param name="key">Key to remove</param>
    public async Task RemoveKeyAsync(string key)
    {
        if (SecureKeys.Contains(key))
        {
            await RemoveSecureKeyAsync(key);
        }
        else
        {
            _preferences.Remove(key);
        }
    }

    /// <summary>
    /// Synchronous version for backward compatibility.
    /// </summary>
    /// <param name="key">Key to remove</param>
    public void RemoveKey(string key)
    {
        // For immediate removal, we'll do both for safety
        if (SecureKeys.Contains(key))
        {
            try
            {
                SecureStorage.Remove(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Secure storage removal failed for key '{key}': {ex.Message}");
            }
        }
        _preferences.Remove(key);
    }

    /// <summary>
    /// Saves string data to appropriate storage based on sensitivity.
    /// </summary>
    /// <param name="key">Storage key</param>
    /// <param name="value">Value to store</param>
    public async Task SaveStringAsync(string key, string value)
    {
        if (SecureKeys.Contains(key))
        {
            await SaveSecureStringAsync(key, value);
        }
        else
        {
            _preferences.Set(key, value);
        }
    }

    /// <summary>
    /// Gets string data from appropriate storage based on sensitivity.
    /// </summary>
    /// <param name="key">Storage key</param>
    /// <param name="defaultValue">Default value if not found</param>
    /// <returns>Stored value or default</returns>
    public async Task<string> GetStringAsync(string key, string defaultValue = "")
    {
        if (SecureKeys.Contains(key))
        {
            return await GetSecureStringAsync(key, defaultValue);
        }
        else
        {
            return _preferences.Get(key, defaultValue);
        }
    }

    #endregion

    #region Legacy Methods (for backward compatibility - non-sensitive data)

    public void SaveStringToPreference(string key, string value)
    {
        if (SecureKeys.Contains(key))
        {
            // For sensitive data, we should use async methods, but for backward compatibility
            // we'll save to both secure storage and preferences
            Task.Run(async () => await SaveSecureStringAsync(key, value));
        }
        else
        {
            _preferences.Set(key, value);
        }
    }

    public void SaveBooleanToPreference(string key, bool value)
    {
        _preferences.Set(key, value);
    }

    public void SaveIntToPreference(string key, int value)
    {
        _preferences.Set(key, value);
    }

    public void SaveDoubleToPreference(string key, double value)
    {
        _preferences.Set(key, value);
    }
    
    public void SaveDateTimeToPreference(string key, DateTime value)
    {
        _preferences.Set(key, value);
    }
    
    public string GetStringFromPreferences(string key, string defaultValue = "")
    {
        if (SecureKeys.Contains(key))
        {
            // For sensitive data, try to get from secure storage first
            try
            {
                var secureResult = Task.Run(async () => await GetSecureStringAsync(key, defaultValue)).Result;
                return secureResult;
            }
            catch
            {
                // Fallback to preferences
                return _preferences.Get(key, defaultValue);
            }
        }
        else
        {
            return _preferences.Get(key, defaultValue);
        }
    }

    public bool GetBooleanFromPreference(string key, bool defaultValue = false)
    {
        return _preferences.Get(key, defaultValue);
    }

    public int GetIntFromPreferences(string key, int defaultValue = 0)
    {
        return _preferences.Get(key, defaultValue);
    }

    public double GetDoubleFromPreference(string key, double defaultValue = 0.0)
    {
        return _preferences.Get(key, defaultValue);
    }
    
    public DateTime GetDateTimeFromPreference(string key, DateTime defaultValue)
    {
        return _preferences.Get(key, defaultValue);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Checks if a key should use secure storage.
    /// </summary>
    /// <param name="key">Key to check</param>
    /// <returns>True if key should use secure storage</returns>
    public static bool IsSecureKey(string key)
    {
        return SecureKeys.Contains(key);
    }

    /// <summary>
    /// Adds a key to the secure keys list for future use.
    /// </summary>
    /// <param name="key">Key to add to secure storage</param>
    public static void AddSecureKey(string key)
    {
        SecureKeys.Add(key);
    }
    #endregion
}
