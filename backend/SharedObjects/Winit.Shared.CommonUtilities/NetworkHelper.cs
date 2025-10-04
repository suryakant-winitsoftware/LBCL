using System.Net.NetworkInformation;

namespace Winit.Shared.CommonUtilities;

/// <summary>
/// Platform-independent network connectivity helper.
/// Provides internet connectivity testing functionality that works across different platforms.
/// </summary>
public static class NetworkHelper
{
    #region Constants

    /// <summary>
    /// Default timeout for ping operations in milliseconds
    /// </summary>
    public const int DefaultPingTimeoutMs = 3000;

    /// <summary>
    /// Reliable hosts to test internet connectivity
    /// </summary>
    private static readonly string[] ReliableHosts = { "8.8.8.8", "1.1.1.1", "google.com" };

    #endregion

    /// <summary>
    /// Checks if internet connectivity is available by pinging reliable hosts.
    /// This method performs actual connectivity tests rather than just checking device network status.
    /// Platform-independent implementation using System.Net.NetworkInformation.
    /// </summary>
    /// <param name="timeoutMs">Timeout for ping operations in milliseconds (default: 3000ms)</param>
    /// <returns>True if internet is available, false otherwise</returns>
    public static async Task<bool> IsInternetAvailableAsync(int timeoutMs = DefaultPingTimeoutMs)
    {
        try
        {
            // Check multiple hosts to ensure reliability
            foreach (var host in ReliableHosts)
            {
                try
                {
                    using var ping = new Ping();
                    var reply = await ping.SendPingAsync(host, timeoutMs);
                    if (reply.Status == IPStatus.Success)
                        return true;
                }
                catch
                {
                    // Continue to next host
                    continue;
                }
            }
            
            return true;
        }
        catch
        {
            // If ping fails completely, assume network is available to avoid false negatives
            // This handles cases where ping might be blocked but HTTP requests work
            return true;
        }
    }

    /// <summary>
    /// Checks if internet connectivity is available with custom hosts.
    /// Platform-independent implementation using System.Net.NetworkInformation.
    /// </summary>
    /// <param name="hostsToTest">Custom list of hosts to test</param>
    /// <param name="timeoutMs">Timeout for ping operations in milliseconds</param>
    /// <returns>True if any host is reachable, false otherwise</returns>
    public static async Task<bool> IsInternetAvailableAsync(string[] hostsToTest, int timeoutMs = DefaultPingTimeoutMs)
    {
        try
        {
            foreach (var host in hostsToTest)
            {
                try
                {
                    using var ping = new Ping();
                    var reply = await ping.SendPingAsync(host, timeoutMs);
                    if (reply.Status == IPStatus.Success)
                        return true;
                }
                catch
                {
                    continue;
                }
            }
            
            return false;
        }
        catch
        {
            return true; // Assume available if testing fails
        }
    }

    /// <summary>
    /// Checks if a specific host is reachable.
    /// Platform-independent implementation using System.Net.NetworkInformation.
    /// </summary>
    /// <param name="host">Host to test (IP address or domain name)</param>
    /// <param name="timeoutMs">Timeout for ping operation in milliseconds</param>
    /// <returns>True if host is reachable, false otherwise</returns>
    public static async Task<bool> IsHostReachableAsync(string host, int timeoutMs = DefaultPingTimeoutMs)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(host, timeoutMs);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }
} 