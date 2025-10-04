using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace WINITMobile.Data;
using Microsoft.Maui.Storage;

public class SecureStorageHelperOld
{
    private readonly IPreferences _preferences;

    public SecureStorageHelperOld()
    {
        _preferences = Preferences.Default;
    }

    public void RemoveKey(string key)
    {
        _preferences.Remove(key);
    }

    public void SaveStringToPreference(string key, string value)
    {
        _preferences.Set(key, value);
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
        return _preferences.Get(key, defaultValue);
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
}
