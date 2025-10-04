using Microsoft.Maui.Networking;
using Winit.Shared.CommonUtilities;

namespace WINITMobile.Services;

/// <summary>
/// Provides network connectivity checking functionality for the MAUI application.
/// Combines platform-independent network testing (via NetworkHelper) with MAUI-specific device status checks.
/// </summary>
public class NetworkConnectivityService
{
    /// <summary>
    /// Checks if internet connectivity is available by pinging reliable hosts.
    /// Uses platform-independent NetworkHelper for actual connectivity testing.
    /// </summary>
    /// <param name="timeoutMs">Timeout for ping operations in milliseconds (default: 3000ms)</param>
    /// <returns>True if internet is available, false otherwise</returns>
    public async Task<bool> IsInternetAvailableAsync(int timeoutMs = NetworkHelper.DefaultPingTimeoutMs)
    {
        return await NetworkHelper.IsInternetAvailableAsync(timeoutMs);
    }

    /// <summary>
    /// Checks if internet connectivity is available with custom hosts.
    /// Uses platform-independent NetworkHelper for actual connectivity testing.
    /// </summary>
    /// <param name="hostsToTest">Custom list of hosts to test</param>
    /// <param name="timeoutMs">Timeout for ping operations in milliseconds</param>
    /// <returns>True if any host is reachable, false otherwise</returns>
    public async Task<bool> IsInternetAvailableAsync(string[] hostsToTest, int timeoutMs = NetworkHelper.DefaultPingTimeoutMs)
    {
        return await NetworkHelper.IsInternetAvailableAsync(hostsToTest, timeoutMs);
    }

    /// <summary>
    /// Checks if a specific host is reachable.
    /// Uses platform-independent NetworkHelper for actual connectivity testing.
    /// </summary>
    /// <param name="host">Host to test (IP address or domain name)</param>
    /// <param name="timeoutMs">Timeout for ping operation in milliseconds</param>
    /// <returns>True if host is reachable, false otherwise</returns>
    public async Task<bool> IsHostReachableAsync(string host, int timeoutMs = NetworkHelper.DefaultPingTimeoutMs)
    {
        return await NetworkHelper.IsHostReachableAsync(host, timeoutMs);
    }

    /// <summary>
    /// Gets the device's network access status (doesn't guarantee internet connectivity).
    /// This is MAUI-specific and only checks if the device has network interface connectivity.
    /// Use IsInternetAvailableAsync() for actual connectivity verification.
    /// </summary>
    /// <returns>Network access status from device</returns>
    public NetworkAccess GetDeviceNetworkStatus()
    {
        try
        {
            return Connectivity.Current.NetworkAccess;
        }
        catch
        {
            return NetworkAccess.Unknown;
        }
    }

    /// <summary>
    /// Performs a quick check combining device status and internet connectivity.
    /// First checks device status (fast), then verifies actual connectivity if needed.
    /// This method combines MAUI-specific device checking with platform-independent connectivity testing.
    /// </summary>
    /// <param name="timeoutMs">Timeout for internet connectivity test</param>
    /// <returns>True if internet is available, false otherwise</returns>
    public async Task<bool> IsConnectedAsync(int timeoutMs = NetworkHelper.DefaultPingTimeoutMs)
    {
        // Quick device check first using MAUI-specific API
        var deviceStatus = GetDeviceNetworkStatus();
        
        if (deviceStatus != NetworkAccess.Internet)
        {
            return false; // No point testing further if device says no network
        }

        // Device says connected, verify actual internet access using shared helper
        return await NetworkHelper.IsInternetAvailableAsync(timeoutMs);
    }
} 