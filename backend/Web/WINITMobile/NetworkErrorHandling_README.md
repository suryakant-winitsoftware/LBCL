# Network Error Handling - BaseComponentBase Integration

## Overview
Network error handling has been integrated directly into `BaseComponentBase.cs` to provide automatic error management and **smart loader control** that respects manual loader operations.

## 🔥 CRITICAL FEATURE: Manual vs Automatic Loader Control

### The Problem (Now Solved)
- ❌ User clicks Login → Shows loader manually
- ❌ During login, API calls fail → NetworkErrorHandler hides loader automatically  
- ❌ Login logic still running → User sees no feedback, thinks app is broken

### The Solution ✅
**Manual loaders are PROTECTED** from automatic network error interference:

```csharp
// PROTECTED MANUAL LOADER (Login/Important Operations)
ShowLoader("Logging in..."); // Marks as manual control - PROTECTED
try 
{
    // Multiple API calls during login
    var authResult = await _apiService.FetchDataAsync<AuthData>("api/auth");
    var deviceResult = await _apiService.FetchDataAsync<DeviceData>("api/device");
    // Even if network errors occur, loader STAYS VISIBLE
}
finally 
{
    HideLoader(); // Only YOU control when to hide
}
```

## What Was Implemented

### 1. Enhanced BaseComponentBase
- **Manual Loader Protection** - ShowLoader() marks loader as manually controlled
- **Smart State Tracking** - Distinguishes manual vs automatic loader control
- **Safe Error Handling** - Network errors don't interfere with manual operations
- **Automatic initialization** in `OnInitializedAsync()`
- **Event subscription** for loader and alert management
- **Proper cleanup** in `Dispose()`

### 2. Service Registration
Added to `MauiProgram.cs`:
```csharp
builder.Services.AddScoped<Winit.Modules.Base.BL.NetworkErrorHandler>();
```

### 3. Key Features

#### Manual Loader Control (NEW):
- ✅ `ShowLoader(message)` - **Marks as manually controlled** (protected from auto-hide)
- ✅ `HideLoader()` - **Clears manual control** and hides loader  
- ✅ `IsLoaderManuallyControlled()` - Check if under manual control
- ✅ `ForceHideLoader()` - Emergency hide (only if not manually controlled)
- ✅ `ShowNetworkErrorSafe()` - Show error without affecting manual loaders

#### Automatic Features (Only When No Manual Control):
- ✅ **Automatic API monitoring** - All `_apiService` calls are tracked
- ✅ **Smart loader management** - Handles concurrent API calls automatically
- ✅ **Network error alerts** - Automatic user-friendly error messages
- ✅ **Respectful behavior** - Never interferes with manual loader control

## Usage Examples

### 1. Protected Manual Loader (Login/Critical Operations)
```csharp
ShowLoader("Logging in..."); // PROTECTED from auto-hide
try 
{
    // Multiple API calls - even if they fail, loader stays visible
    var authResult = await _apiService.FetchDataAsync<AuthData>("api/auth");
    var deviceResult = await _apiService.FetchDataAsync<DeviceData>("api/device");
    var userResult = await _apiService.FetchDataAsync<UserData>("api/user");
    
    // Process results...
}
catch (Exception ex)
{
    // Show error but keep loader visible
    await ShowNetworkErrorSafe("Login Failed", ex.Message, hideLoaderOnError: false);
    // Continue processing or handle manually
}
finally 
{
    HideLoader(); // Only YOU control when to hide
}
```

### 2. Automatic Loader (Simple Data Fetch)
```csharp
// No manual ShowLoader() call - automatic management
try 
{
    var result = await _apiService.FetchDataAsync<SomeData>("api/endpoint");
    // NetworkErrorHandler auto-manages loader
    // On error: auto-hides loader and shows alert
}
catch (Exception ex)
{
    // Handle if needed, loader already hidden automatically
}
```

### 3. Safe Network Error Handling
```csharp
ShowLoader("Processing complex operation...");
try 
{
    var step1 = await _apiService.FetchDataAsync<Data>("api/step1");
    var step2 = await _apiService.FetchDataAsync<Data>("api/step2");
}
catch (NetworkException ex)
{
    // Show error but DON'T hide loader (we control it)
    await ShowNetworkErrorSafe("Network Error", ex.Message, hideLoaderOnError: false);
    // Continue with offline logic or retry
}
finally 
{
    HideLoader(); // Manual control - hide when WE decide
}
```

## Benefits

### For Developers:
- ✅ **Zero code changes** required for existing pages
- ✅ **Consistent error handling** across the entire app
- ✅ **Automatic loader management** prevents stuck loaders
- ✅ **Enhanced debugging** with proper error categorization

### For Users:
- ✅ **User-friendly error messages** with clear explanations
- ✅ **No stuck loaders** during network issues
- ✅ **Consistent UX** across all app screens
- ✅ **Better feedback** on network status

## Architecture

```
BaseComponentBase (All Pages Inherit)
├── NetworkErrorHandler (Injected)
├── ApiService (Injected)  
├── Automatic Initialization (OnInitializedAsync)
├── Event Subscription (Loader + Alert events)
└── Proper Cleanup (Dispose)
```

## How It Works

1. **Page Load**: `BaseComponentBase` initializes and connects `NetworkErrorHandler` to `ApiService`
2. **API Call**: When any page calls `_apiService.FetchDataAsync()`, it's automatically monitored
3. **Network Error**: If error occurs, loader is hidden and appropriate alert is shown
4. **Page Dispose**: Cleanup removes event subscriptions

## Testing

### Test Network Scenarios:
1. **Turn off WiFi/Data** - Should show "No Internet Connection" 
2. **Server down** - Should show "Server Unavailable"
3. **Slow connection** - Should show timeout or slow connection messages
4. **Multiple API calls** - Loader should show/hide correctly for concurrent calls

### Test Emergency Cases:
1. Call `ForceHideLoader()` when loader is showing - Should immediately hide
2. Check `IsLoaderVisible()` - Should return correct state
3. Multiple rapid API calls - Should handle loader state correctly

## Migration Notes

- ✅ **No existing code changes needed**
- ✅ **All pages automatically get these features**
- ✅ **Existing `ShowLoader/HideLoader` calls work the same**
- ✅ **Service is registered in DI container**
- ✅ **Thread-safe for concurrent operations**

## Future Enhancements

- Add retry mechanism for failed requests
- Add offline queue for failed operations  
- Add network status indicators in UI
- Add custom error messages based on API endpoints

---

**Implementation Status**: ✅ Complete and Ready for Use
**Breaking Changes**: ❌ None - Backward Compatible
**Testing Required**: ✅ Test network scenarios and emergency cases 