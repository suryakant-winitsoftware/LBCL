# Network Connectivity Service - Implementation Guide

## Overview
We've implemented **three layers of network connectivity** with proper separation of concerns and no circular dependencies.

## 🏗️ **Architecture: Three-Layer Approach**

### **📚 Shared Library Level: NetworkHelper (Platform-Independent)**
- **Location**: `SharedObjects/Winit.Shared.CommonUtilities/NetworkHelper.cs`
- **Type**: Static class with platform-independent methods
- **Purpose**: Core network connectivity testing using `System.Net.NetworkInformation.Ping`
- **Dependencies**: None (uses only standard .NET APIs)
- **Usage**: Shared by both ApiService and platform-specific services

### **🔧 Service Level: ApiService**
- **Location**: `Modules/Base/Winit.Modules.Base.BL/ApiService.cs`
- **Purpose**: API calls with automatic network validation
- **Dependencies**: Uses shared `NetworkHelper`
- **Usage**: Automatic network checking before API requests

### **🖥️ Platform Level: NetworkConnectivityService (MAUI-Specific)**
- **Location**: `Web/WINITMobile/Services/NetworkConnectivityService.cs`
- **Namespace**: `WINITMobile.Services`
- **Purpose**: Platform-specific features + shared logic delegation
- **Dependencies**: Uses shared `NetworkHelper` + MAUI APIs (`Microsoft.Maui.Networking`)
- **Usage**: UI-level network checking with device status integration

## ✅ **Why This Architecture is Optimal**

| Aspect | Old Approach | New Three-Layer Approach |
|--------|-------------|--------------------------|
| **Code Duplication** | ❌ Same logic in multiple places | ✅ Centralized in NetworkHelper |
| **Platform Independence** | ❌ Mixed platform-specific code | ✅ Clear separation of concerns |
| **Circular Dependencies** | ❌ Potential circular deps | ✅ No circular dependencies |
| **Library Reusability** | ❌ Tied to specific platforms | ✅ NetworkHelper reusable anywhere |
| **Testability** | ❌ Complex dependencies | ✅ Easy to test each layer |
| **Maintainability** | ❌ Logic scattered | ✅ Single source of truth |

## 🔄 **Architecture Layers Breakdown**

### **Layer 1: NetworkHelper (Shared)**
```csharp
// Platform-independent methods in SharedObjects/Winit.Shared.CommonUtilities/
public static class NetworkHelper
{
    // Core internet connectivity testing
    public static async Task<bool> IsInternetAvailableAsync(int timeoutMs = 3000)
    
    // Custom hosts testing
    public static async Task<bool> IsInternetAvailableAsync(string[] hostsToTest, int timeoutMs = 3000)
    
    // Single host testing
    public static async Task<bool> IsHostReachableAsync(string host, int timeoutMs = 3000)
}
```

### **Layer 2: ApiService (Library)**
```csharp
// Uses NetworkHelper for network validation before API calls
public class ApiService
{
    public async Task<ApiResponse<T>> FetchDataAsync<T>(...)
    {
        // Uses shared NetworkHelper
        if (!await NetworkHelper.IsInternetAvailableAsync())
        {
            // Handle no connection
        }
        // ... API call logic
    }
    
    // Backward compatibility wrapper
    public async Task<bool> IsNetworkAvailableAsync(int timeoutMs = 3000)
    {
        return await NetworkHelper.IsInternetAvailableAsync(timeoutMs);
    }
}
```

### **Layer 3: NetworkConnectivityService (Platform-Specific)**
```csharp
// Combines shared logic with MAUI-specific features
public class NetworkConnectivityService
{
    // Delegates to NetworkHelper
    public async Task<bool> IsInternetAvailableAsync(int timeoutMs = 3000)
    {
        return await NetworkHelper.IsInternetAvailableAsync(timeoutMs);
    }
    
    // MAUI-specific device status
    public NetworkAccess GetDeviceNetworkStatus()
    {
        return Connectivity.Current.NetworkAccess; // MAUI API
    }
    
    // Combines device check + internet test
    public async Task<bool> IsConnectedAsync(int timeoutMs = 3000)
    {
        var deviceStatus = GetDeviceNetworkStatus();
        if (deviceStatus != NetworkAccess.Internet) return false;
        
        return await NetworkHelper.IsInternetAvailableAsync(timeoutMs);
    }
}
```

## 🚀 **Available Methods by Layer**

### **NetworkHelper (Static Methods - Available Everywhere)**
```csharp
// Core platform-independent methods
bool isConnected = await NetworkHelper.IsInternetAvailableAsync();
bool isConnected = await NetworkHelper.IsInternetAvailableAsync(5000); // Custom timeout
bool isConnected = await NetworkHelper.IsInternetAvailableAsync(new[] { "company.com", "8.8.8.8" });
bool hostReachable = await NetworkHelper.IsHostReachableAsync("google.com");
```

### **ApiService Methods**
```csharp
// In any service that injects ApiService
var apiResponse = await _apiService.FetchDataAsync<MyType>(...); // Auto network check
bool isConnected = await _apiService.IsNetworkAvailableAsync(); // Wrapper method
```

### **NetworkConnectivityService Methods (MAUI-Specific)**
```csharp
// In MAUI app context
bool isConnected = await _networkService.IsInternetAvailableAsync();
NetworkAccess deviceStatus = _networkService.GetDeviceNetworkStatus();
bool isConnected = await _networkService.IsConnectedAsync(); // Device + Internet check
```

### **BaseComponentBase Helper Methods**
```csharp
// Available in all pages inheriting from BaseComponentBase

// Platform-independent methods (using NetworkHelper)
bool isConnected = await IsInternetAvailableAsync();
bool hostReachable = await IsHostReachableAsync("google.com");
bool isConnected = await CheckInternetConnectivityWithAlert(); // With alert

// Platform-specific methods (using NetworkConnectivityService)
NetworkAccess deviceStatus = GetDeviceNetworkStatus();
bool isConnected = await IsConnectedAsync(); // Device + Internet check
```

## 📝 **Usage Examples**

### **1. Simple Platform-Independent Check**
```csharp
// Available anywhere in the codebase
if (!await NetworkHelper.IsInternetAvailableAsync())
{
    // Handle offline scenario
    return;
}
```

### **2. Page-Level Check with Alert**
```csharp
public partial class SomePage : BaseComponentBase
{
    private async Task OnSaveClick()
    {
        // Uses shared NetworkHelper + shows alert
        if (!await CheckInternetConnectivityWithAlert())
            return;
            
        // Proceed with save
        await SaveData();
    }
}
```

### **3. Service-Level Check with Device Status**
```csharp
public class SomeService
{
    private readonly NetworkConnectivityService _networkService;
    
    public async Task<bool> SyncData()
    {
        // Quick device check first
        if (_networkService.GetDeviceNetworkStatus() != NetworkAccess.Internet)
            return false;
            
        // Then verify actual connectivity
        if (!await _networkService.IsInternetAvailableAsync())
            return false;
        
        return await PerformSync();
    }
}
```

### **4. Automatic API Network Validation**
```csharp
// ApiService automatically checks network before every API call
var result = await _apiService.FetchDataAsync<MyData>("api/data", HttpMethod.Get);
// If no internet, returns ApiResponse with error status automatically
```

## 🔧 **Service Registration**

```csharp
// MauiProgram.cs - All services registered
builder.Services.AddScoped<Winit.Modules.Base.BL.ApiService>();
builder.Services.AddScoped<WINITMobile.Services.NetworkConnectivityService>();
// NetworkHelper is static - no registration needed
```

## 🎯 **Migration Benefits Achieved**

1. ✅ **No Code Duplication**: Core logic centralized in `NetworkHelper`
2. ✅ **Platform Independence**: Shared logic works everywhere
3. ✅ **Platform-Specific Features**: MAUI features available where needed
4. ✅ **No Circular Dependencies**: Clean dependency flow
5. ✅ **Backward Compatibility**: Existing code continues to work
6. ✅ **Better Performance**: Shared logic optimized once
7. ✅ **Easy Testing**: Each layer testable independently
8. ✅ **Future-Proof**: Easy to add features or support new platforms

## 🔮 **Future Enhancements**

- **Parallel Host Testing**: Test multiple hosts simultaneously in NetworkHelper
- **Connection Quality Detection**: Measure latency/speed in shared logic
- **Caching**: Cache connectivity results for short periods
- **Regional Optimization**: Use geographically closer hosts
- **Real-time Monitoring**: Platform-specific connectivity change notifications
- **Custom Retry Logic**: Exponential backoff in shared logic

## 📊 **Performance Characteristics**

| Method | Layer | Performance | Use Case |
|--------|-------|-------------|----------|
| `NetworkHelper.IsInternetAvailableAsync()` | Shared | ~1-3 seconds | Core connectivity test |
| `NetworkConnectivityService.GetDeviceNetworkStatus()` | Platform | ~1 millisecond | Quick device check |
| `NetworkConnectivityService.IsConnectedAsync()` | Platform | ~1-3 seconds | Optimized device + internet |
| `ApiService.FetchDataAsync()` | Service | ~1-3 seconds + API time | API with auto-check |

---

**Implementation Status**: ✅ **Complete - Three-Layer Architecture**
**Breaking Changes**: ❌ **None - Fully Backward Compatible**
**Architecture Benefits**: ✅ **Optimal Separation of Concerns + No Code Duplication** 