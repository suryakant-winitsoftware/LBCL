using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Winit.Modules.Base.BL
{
    /// <summary>
    /// Centralized service for handling network errors, loader management, and user alerts
    /// </summary>
    public class NetworkErrorHandler
    {
        #region Private Fields

        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager? _navigationManager;
        private int _activeApiCalls = 0;
        private readonly object _lockObject = new object();

        #endregion

        #region Events

        /// <summary>
        /// Event raised when loader state should change
        /// </summary>
        public event Action<bool> OnLoaderStateChanged;

        /// <summary>
        /// Event raised when network alert should be shown
        /// </summary>
        public event Action<string, string> OnShowAlert; // title, message

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the NetworkErrorHandler
        /// </summary>
        /// <param name="jsRuntime">JavaScript runtime for alerts</param>
        /// <param name="navigationManager">Navigation manager for routing</param>
        public NetworkErrorHandler(IJSRuntime jsRuntime, NavigationManager? navigationManager = null)
        {
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
            _navigationManager = navigationManager;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the error handler by subscribing to ApiService events
        /// </summary>
        /// <param name="apiService">ApiService to monitor</param>
        public void Initialize(ApiService apiService)
        {
            if (apiService == null)
                throw new ArgumentNullException(nameof(apiService));

            apiService.OnApiCallStarted += HandleApiCallStarted;
            apiService.OnApiCallCompleted += HandleApiCallCompleted;
            apiService.OnNetworkError += HandleNetworkError;
        }

        /// <summary>
        /// Shows network error alert with appropriate message and title
        /// </summary>
        /// <param name="errorType">Type of network error</param>
        /// <param name="message">Error message to display</param>
        public async Task ShowNetworkErrorAsync(ApiService.NetworkErrorType errorType, string message)
        {
            var title = GetErrorTitle(errorType);
            
            try
            {
                // Try to show JavaScript alert first
                await _jsRuntime.InvokeVoidAsync("alert", $"{title}\n\n{message}");
            }
            catch
            {
                // Fallback to event if JavaScript is not available
                OnShowAlert?.Invoke(title, message);
            }
        }

        /// <summary>
        /// Shows a custom alert with title and message
        /// </summary>
        /// <param name="title">Alert title</param>
        /// <param name="message">Alert message</param>
        public async Task ShowAlertAsync(string title, string message)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("alert", $"{title}\n\n{message}");
            }
            catch
            {
                OnShowAlert?.Invoke(title, message);
            }
        }

        /// <summary>
        /// Shows a confirmation dialog
        /// </summary>
        /// <param name="title">Confirmation title</param>
        /// <param name="message">Confirmation message</param>
        /// <returns>True if user confirms, false otherwise</returns>
        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<bool>("confirm", $"{title}\n\n{message}");
            }
            catch
            {
                // Fallback to true if JavaScript is not available
                return true;
            }
        }

        /// <summary>
        /// Forces loader to hide (useful for emergency situations)
        /// </summary>
        public void ForceHideLoader()
        {
            lock (_lockObject)
            {
                _activeApiCalls = 0;
            }
            OnLoaderStateChanged?.Invoke(false);
        }

        /// <summary>
        /// Gets current loader state
        /// </summary>
        /// <returns>True if loader should be shown</returns>
        public bool IsLoaderVisible()
        {
            lock (_lockObject)
            {
                return _activeApiCalls > 0;
            }
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Handles API call started event
        /// </summary>
        private void HandleApiCallStarted()
        {
            lock (_lockObject)
            {
                _activeApiCalls++;
                if (_activeApiCalls == 1)
                {
                    OnLoaderStateChanged?.Invoke(true);
                }
            }
        }

        /// <summary>
        /// Handles API call completed event
        /// </summary>
        private void HandleApiCallCompleted()
        {
            lock (_lockObject)
            {
                _activeApiCalls = Math.Max(0, _activeApiCalls - 1);
                if (_activeApiCalls == 0)
                {
                    OnLoaderStateChanged?.Invoke(false);
                }
            }
        }

        /// <summary>
        /// Handles network error event
        /// </summary>
        /// <param name="errorType">Type of network error</param>
        /// <param name="message">Error message</param>
        private async void HandleNetworkError(ApiService.NetworkErrorType errorType, string message)
        {
            // Ensure loader is hidden on network errors
            ForceHideLoader();
            
            // Show appropriate error message
            await ShowNetworkErrorAsync(errorType, message);
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Gets appropriate title for error type
        /// </summary>
        /// <param name="errorType">Type of network error</param>
        /// <returns>User-friendly error title</returns>
        private string GetErrorTitle(ApiService.NetworkErrorType errorType)
        {
            return errorType switch
            {
                ApiService.NetworkErrorType.NoInternetConnection => "🔌 No Internet Connection",
                ApiService.NetworkErrorType.ServerUnreachable => "🔧 Server Unavailable",
                ApiService.NetworkErrorType.RequestTimeout => "⏱️ Request Timeout",
                ApiService.NetworkErrorType.ConnectionLost => "📡 Connection Lost",
                ApiService.NetworkErrorType.SlowConnection => "🐌 Slow Connection",
                ApiService.NetworkErrorType.ServerError => "⚠️ Server Error",
                _ => "❌ Network Error"
            };
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Unsubscribes from ApiService events
        /// </summary>
        /// <param name="apiService">ApiService to unsubscribe from</param>
        public void Cleanup(ApiService apiService)
        {
            if (apiService != null)
            {
                apiService.OnApiCallStarted -= HandleApiCallStarted;
                apiService.OnApiCallCompleted -= HandleApiCallCompleted;
                apiService.OnNetworkError -= HandleNetworkError;
            }
        }

        #endregion
    }
} 