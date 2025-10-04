# Generic Network Error Handling & Loader Management

This implementation provides a comprehensive solution for handling network issues, managing loaders, and showing appropriate alerts across your application.

## 🚀 Features

- **Automatic Loader Management**: Shows/hides loaders based on API call state
- **Network Connectivity Detection**: Checks internet connectivity before API calls
- **Smart Error Classification**: Categorizes different types of network errors
- **User-Friendly Alerts**: Shows appropriate messages for different error types
- **Thread-Safe**: Handles multiple concurrent API calls correctly
- **Generic Implementation**: Works across all components and services

## 📋 Setup Instructions

### 1. Register Services in Dependency Injection

```csharp
// In Program.cs or Startup.cs
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<NetworkErrorHandler>();
```

### 2. Initialize in Your Main Layout/Component

```csharp
// In MainLayout.razor or App.razor
@inject ApiService ApiService
@inject NetworkErrorHandler NetworkErrorHandler
@inject IJSRuntime JSRuntime

@code {
    protected override void OnInitialized()
    {
        // Initialize network error handling
        NetworkErrorHandler.Initialize(ApiService);
    }
}
```

### 3. Add Loader Component to Your Layout

```html
<!-- In MainLayout.razor -->
<LoaderComponent LoadingText="Please wait..." />

<!-- Your main content -->
<div class="page">
    @Body
</div>
```

## 🔧 Usage Examples

### Basic API Call with Automatic Loader

```csharp
// The loader will automatically show/hide
var result = await apiService.FetchDataAsync<User>(
    "api/users", 
    HttpMethod.Get
);

// Network errors are automatically handled with alerts
```

### API Call without Loader (Silent)

```csharp
// For background operations
var result = await apiService.FetchDataAsync<User>(
    "api/users", 
    HttpMethod.Get,
    showLoader: false
);
```

### File Upload with Custom Loader Text

```csharp
// In your component
@inject NetworkErrorHandler NetworkErrorHandler

// Change loader text dynamically
NetworkErrorHandler.OnLoaderStateChanged += (show) => {
    if (show) LoadingText = "Uploading file...";
};

var result = await apiService.UploadFileAsync(endpoint, content);
```

### Custom Error Handling

```csharp
// Subscribe to network error events for custom handling
NetworkErrorHandler.OnNetworkError += async (errorType, message) => {
    if (errorType == ApiService.NetworkErrorType.NoInternetConnection)
    {
        // Custom logic for no internet
        await ShowRetryDialog();
    }
};
```

## 🛡️ Error Types Handled

| Error Type | Description | User Message |
|------------|-------------|--------------|
| `NoInternetConnection` | No network connectivity | "🔌 No Internet Connection" |
| `ServerUnreachable` | Server is down/unreachable | "🔧 Server Unavailable" |
| `RequestTimeout` | Request took too long | "⏱️ Request Timeout" |
| `ConnectionLost` | Connection dropped during request | "📡 Connection Lost" |
| `SlowConnection` | Connection is slow | "🐌 Slow Connection" |
| `ServerError` | Server returned error | "⚠️ Server Error" |

## 🎯 Advanced Features

### Manual Loader Control

```csharp
// Force hide loader in emergency situations
networkErrorHandler.ForceHideLoader();

// Check if loader is currently visible
bool isVisible = networkErrorHandler.IsLoaderVisible();
```

### Custom Alerts

```csharp
// Show custom alert
await networkErrorHandler.ShowAlertAsync("Warning", "This action cannot be undone");

// Show confirmation dialog
bool confirmed = await networkErrorHandler.ShowConfirmationAsync(
    "Confirm Delete", 
    "Are you sure you want to delete this item?"
);
```

### Network Connectivity Check

```csharp
// Manual network check
bool hasInternet = await apiService.IsNetworkAvailableAsync();
if (!hasInternet)
{
    await networkErrorHandler.ShowAlertAsync("Offline", "Please check your connection");
}
```

## 🔄 How It Works

1. **API Call Initiated**: `OnApiCallStarted` event fired → Loader shows
2. **Network Check**: Connectivity verified before making request
3. **Request Processing**: HTTP request sent with proper error handling
4. **Error Detection**: Network errors classified and appropriate alerts shown
5. **API Call Completed**: `OnApiCallCompleted` event fired → Loader hides
6. **Cleanup**: Events ensure loader is always hidden, even on errors

## 📱 Mobile-Specific Considerations

```csharp
// In your mobile app initialization
if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
{
    // Mobile-specific network change monitoring
    Connectivity.ConnectivityChanged += OnConnectivityChanged;
}

void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
{
    if (e.NetworkAccess != NetworkAccess.Internet)
    {
        networkErrorHandler.ForceHideLoader();
        // Show offline message
    }
}
```

## 🧹 Cleanup

```csharp
// In component disposal
public void Dispose()
{
    NetworkErrorHandler.Cleanup(ApiService);
}
```

## 🎨 Customization

### Custom Loader Component

```csharp
// Create your own loader component implementing the same events
NetworkErrorHandler.OnLoaderStateChanged += (show) => {
    // Your custom loader logic
    MyCustomLoader.SetVisible(show);
};
```

### Custom Alert System

```csharp
// Use your own alert system instead of JavaScript alerts
NetworkErrorHandler.OnShowAlert += (title, message) => {
    // Show using your custom alert component
    MyAlertService.ShowAlert(title, message);
};
```

## ✅ Benefits

- **Consistent UX**: Same error handling across all components
- **Automatic Management**: No need to manually show/hide loaders
- **Better User Experience**: Clear error messages with appropriate icons
- **Thread Safety**: Handles multiple concurrent API calls
- **Extensible**: Easy to customize for specific needs
- **Mobile Ready**: Works perfectly with Blazor Hybrid apps

This system ensures that users never see stuck loaders and always get appropriate feedback when network issues occur. 