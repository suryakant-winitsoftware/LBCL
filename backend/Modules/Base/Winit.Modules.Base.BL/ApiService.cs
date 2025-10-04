using Azure.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model.Constants;
using Winit.Modules.Common.Model.Exceptions;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.CommonUtilities;
namespace Winit.Modules.Base.BL;

/// <summary>
/// Provides HTTP API communication services with authentication, error handling, and timeout management.
/// </summary>
public class ApiService
{
    #region Constants

    // Routes
    private const string SessionExpiredRoute = "SessionExpired";
    
    // Content Types
    private const string JsonContentType = "application/json";
    
    // HTTP Headers
    private const string BearerPrefix = "Bearer";
    
    // Default Timeout Values (in seconds)
    private const int DefaultTimeoutSeconds = 300;
    private const int DefaultLongTimeoutSeconds = 300;
    private const int DefaultUploadTimeoutSeconds = 300;
    
    // Error Messages
    private const string ConnectionErrorMessage = "Connection Error: ";
    private const string TimeoutErrorMessage = "Request Timed Out";
    private const string UnauthorizedErrorMessage = "Unauthorized";
    private const string InternalServerErrorMessage = "Internal Server Error";
    private const string BadRequestErrorMessage = "Bad Request";
    private const string NotFoundErrorMessage = "Resource Not Found";
    private const string ForbiddenErrorMessage = "Forbidden";
    private const string MethodNotAllowedErrorMessage = "Method Not Allowed";
    private const string TooManyRequestsErrorMessage = "Too Many Requests";
    private const string ApiRequestFailedMessage = "API Request Failed";
    private const string SuccessMessage = "Success";
    
    // Response Structure Identifiers
    private const string DataProperty = "\"Data\":";
    private const string StatusCodeProperty = "\"StatusCode\":";
    
    #endregion

    #region Private Fields

    private readonly HttpClient _httpClient;
    private readonly NavigationManager? _navigationManager;
    private readonly CommonFunctions _commonFunctions;
    private readonly ILocalStorageService? _localStorageService;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    #endregion

    #region Events and Delegates

    /// <summary>
    /// Event raised when API call starts (for loader management)
    /// </summary>
    public event Action OnApiCallStarted;

    /// <summary>
    /// Event raised when API call completes (for loader management)
    /// </summary>
    public event Action OnApiCallCompleted;

    /// <summary>
    /// Event raised when network error occurs (for alert management)
    /// </summary>
    public event Action<NetworkErrorType, string> OnNetworkError;

    #endregion

    #region Enums

    /// <summary>
    /// Types of network errors for specific handling
    /// </summary>
    public enum NetworkErrorType
    {
        NoInternetConnection,
        ServerUnreachable,
        RequestTimeout,
        ConnectionLost,
        SlowConnection,
        ServerError
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the ApiService class with full dependencies.
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls</param>
    /// <param name="jsonSerializerSettings">JSON serialization settings</param>
    /// <param name="authStateProvider">Authentication state provider</param>
    /// <param name="commonFunctions">Common utility functions</param>
    /// <param name="navigationManager">Navigation manager for routing</param>
    /// <param name="localStorageService">Local storage service for token management</param>
    public ApiService(
        HttpClient httpClient, 
        JsonSerializerSettings jsonSerializerSettings, 
        AuthenticationStateProvider authStateProvider = null,
        CommonFunctions commonFunctions = null, 
        NavigationManager? navigationManager = null,
        ILocalStorageService? localStorageService = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _navigationManager = navigationManager;
        _jsonSerializerSettings = jsonSerializerSettings ?? throw new ArgumentNullException(nameof(jsonSerializerSettings));
        _localStorageService = localStorageService;
        _authStateProvider = authStateProvider;
        _commonFunctions = commonFunctions;
        
        ConfigureHttpClientDefaults();
    }

    /// <summary>
    /// Initializes a new instance of the ApiService class with minimal dependencies.
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls</param>
    /// <param name="commonFunctions">Common utility functions</param>
    public ApiService(HttpClient httpClient, CommonFunctions commonFunctions = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _commonFunctions = commonFunctions;
        
        ConfigureHttpClientDefaults();
    }

    #endregion

    #region Private Configuration Methods

    /// <summary>
    /// Configures default settings for the HTTP client including timeout.
    /// </summary>
    private void ConfigureHttpClientDefaults()
    {
        if (_httpClient.Timeout == TimeSpan.Zero || _httpClient.Timeout == Timeout.InfiniteTimeSpan)
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(DefaultTimeoutSeconds);
        }
    }

    #endregion

    #region Public API Methods

    /// <summary>
    /// Fetches data from API endpoint with typed response handling.
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="endpoint">API endpoint URL</param>
    /// <param name="httpMethod">HTTP method to use</param>
    /// <param name="requestData">Request payload data</param>
    /// <param name="headers">Additional HTTP headers</param>
    /// <param name="timeoutSeconds">Request timeout in seconds (optional)</param>
    /// <param name="temporaryToken">Temporary authentication token to use instead of localStorage token</param>
    /// <param name="showLoader">Whether to show loader during API call (default: true)</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>API response with typed data</returns>
    public async Task<ApiResponse<T>> FetchDataAsync<T>(
        string endpoint, 
        HttpMethod httpMethod, 
        object? requestData = null, 
        IDictionary<string, string>? headers = null, 
        int? timeoutSeconds = null,
        string? temporaryToken = null,
        bool showLoader = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));

        try
        {
            if (!await NetworkHelper.IsInternetAvailableAsync())
            {
                HandleNetworkError(NetworkErrorType.NoInternetConnection, "No internet connection available. Please check your network settings.");
                return new ApiResponse<T>(default, 0, "No internet connection");
            }

            if (showLoader)
                OnApiCallStarted?.Invoke();

            var requestMessage = await CreateHttpRequestMessage(endpoint, httpMethod, requestData, headers, temporaryToken);
            
            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            
            await HandleUnauthorizedResponse(response);
            
            return await HandleTypedResponse<T>(response);
        }
        catch (OperationCanceledException ex) when (ex is TaskCanceledException || cancellationToken.IsCancellationRequested)
        {
            var cancelMessage = "Operation was cancelled by user";
            HandleNetworkError(NetworkErrorType.RequestTimeout, cancelMessage);
            return new ApiResponse<T>(default, 0, cancelMessage);
        }
        catch (OperationCanceledException ex)
        {
            var timeoutMessage = $"Request timed out after {_httpClient.Timeout.TotalSeconds} seconds. Error: {ex.Message}";
            HandleNetworkError(NetworkErrorType.RequestTimeout, timeoutMessage);
            return new ApiResponse<T>(default, 0, timeoutMessage);
        }
        catch (HttpRequestException ex)
        {
            HandleNetworkError(DetermineNetworkErrorType(ex), GetNetworkErrorMessage(ex));
            return new ApiResponse<T>(default, 0, ConnectionErrorMessage + ex.Message);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Unexpected error: {ex.Message} | Stack trace: {ex.StackTrace}";
            HandleNetworkError(NetworkErrorType.ServerError, errorMessage);
            return new ApiResponse<T>(default, 0, errorMessage);
        }
        finally
        {
            if (showLoader)
                OnApiCallCompleted?.Invoke();
        }
    }

    /// <summary>
    /// Fetches data from API endpoint with string response handling.
    /// </summary>
    /// <param name="endpoint">API endpoint URL</param>
    /// <param name="httpMethod">HTTP method to use</param>
    /// <param name="requestData">Request payload data</param>
    /// <param name="headers">Additional HTTP headers</param>
    /// <param name="timeoutSeconds">Request timeout in seconds (optional)</param>
    /// <param name="temporaryToken">Temporary authentication token to use instead of localStorage token</param>
    /// <param name="showLoader">Whether to show loader during API call (default: true)</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>API response with string data</returns>
    public async Task<ApiResponse<string>> FetchDataAsync(
        string endpoint, 
        HttpMethod httpMethod, 
        object? requestData = null, 
        IDictionary<string, string>? headers = null, 
        int? timeoutSeconds = null,
        string? temporaryToken = null,
        bool showLoader = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));

        try
        {
            if (!await NetworkHelper.IsInternetAvailableAsync())
            {
                HandleNetworkError(NetworkErrorType.NoInternetConnection, "No internet connection available. Please check your network settings.");
                return new ApiResponse<string>(default, 0, "No internet connection");
            }

            if (showLoader)
                OnApiCallStarted?.Invoke();

            var requestMessage = await CreateHttpRequestMessage(endpoint, httpMethod, requestData, headers, temporaryToken);
            
            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            
            await HandleUnauthorizedResponse(response);
            
            return await HandleStringResponse(response);
        }
        catch (OperationCanceledException ex) when (ex is TaskCanceledException || cancellationToken.IsCancellationRequested)
        {
            var cancelMessage = "Operation was cancelled by user";
            HandleNetworkError(NetworkErrorType.RequestTimeout, cancelMessage);
            return new ApiResponse<string>(default, 0, cancelMessage);
        }
        catch (OperationCanceledException ex)
        {
            var timeoutMessage = $"Request timed out after {_httpClient.Timeout.TotalSeconds} seconds. Error: {ex.Message}";
            HandleNetworkError(NetworkErrorType.RequestTimeout, timeoutMessage);
            return new ApiResponse<string>(default, 0, timeoutMessage);
        }
        catch (HttpRequestException ex)
        {
            HandleNetworkError(DetermineNetworkErrorType(ex), GetNetworkErrorMessage(ex));
            return new ApiResponse<string>(default, 0, ConnectionErrorMessage + ex.Message);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Unexpected error: {ex.Message} | Stack trace: {ex.StackTrace}";
            HandleNetworkError(NetworkErrorType.ServerError, errorMessage);
            return new ApiResponse<string>(default, 0, errorMessage);
        }
        finally
        {
            if (showLoader)
                OnApiCallCompleted?.Invoke();
        }
    }

    /// <summary>
    /// Uploads file to API endpoint with extended timeout.
    /// </summary>
    /// <param name="endpoint">Upload endpoint URL</param>
    /// <param name="content">File content to upload</param>
    /// <param name="headers">Additional HTTP headers</param>
    /// <param name="timeoutSeconds">Upload timeout in seconds (optional, defaults to extended timeout)</param>
    /// <param name="showLoader">Whether to show loader during upload (default: true)</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>API response with upload result</returns>
    public async Task<ApiResponse<string>> UploadFileAsync(
        string endpoint, 
        HttpContent? content,
        IDictionary<string, string>? headers = null,
        int? timeoutSeconds = null,
        bool showLoader = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));

        try
        {
            if (!await NetworkHelper.IsInternetAvailableAsync())
            {
                HandleNetworkError(NetworkErrorType.NoInternetConnection, "No internet connection available. Please check your network settings.");
                return new ApiResponse<string>(default, 0, "No internet connection");
            }

            if (showLoader)
                OnApiCallStarted?.Invoke();

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = content
            };

            await SetAuthorizationHeaderAsync(requestMessage);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    requestMessage.Headers.Add(header.Key, header.Value);
                }
            }

            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            await HandleUnauthorizedResponse(response);
            return await HandleStringResponse(response);
        }
        catch (OperationCanceledException ex) when (ex is TaskCanceledException || cancellationToken.IsCancellationRequested)
        {
            var cancelMessage = "File upload was cancelled by user";
            HandleNetworkError(NetworkErrorType.RequestTimeout, cancelMessage);
            return new ApiResponse<string>(default, 0, cancelMessage);
        }
        catch (OperationCanceledException ex)
        {
            var timeoutMessage = $"File upload timed out after {_httpClient.Timeout.TotalSeconds} seconds. Error: {ex.Message}";
            HandleNetworkError(NetworkErrorType.RequestTimeout, timeoutMessage);
            return new ApiResponse<string>(default, 0, timeoutMessage);
        }
        catch (HttpRequestException ex)
        {
            HandleNetworkError(DetermineNetworkErrorType(ex), GetNetworkErrorMessage(ex));
            return new ApiResponse<string>(default, 0, ConnectionErrorMessage + ex.Message);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Upload error: {ex.Message} | Stack trace: {ex.StackTrace}";
            HandleNetworkError(NetworkErrorType.ServerError, errorMessage);
            return new ApiResponse<string>(default, 0, errorMessage);
        }
        finally
        {
            if (showLoader)
                OnApiCallCompleted?.Invoke();
        }
    }

    #endregion

    #region Network Connectivity Methods

    /// <summary>
    /// Wrapper method for checking internet connectivity using shared NetworkHelper.
    /// Maintains backward compatibility while delegating to centralized network logic.
    /// </summary>
    /// <param name="timeoutMs">Timeout for ping operations in milliseconds (default: 3000ms)</param>
    /// <returns>True if internet is available, false otherwise</returns>
    public async Task<bool> IsNetworkAvailableAsync(int timeoutMs = 3000)
    {
        return await NetworkHelper.IsInternetAvailableAsync(timeoutMs);
    }

    /// <summary>
    /// Determines the type of network error based on HttpRequestException
    /// </summary>
    /// <param name="ex">HttpRequestException to analyze</param>
    /// <returns>NetworkErrorType</returns>
    private NetworkErrorType DetermineNetworkErrorType(HttpRequestException ex)
    {
        var message = ex.Message.ToLower();
        
        if (message.Contains("timeout") || message.Contains("timed out"))
            return NetworkErrorType.RequestTimeout;
        
        if (message.Contains("unreachable") || message.Contains("connection refused"))
            return NetworkErrorType.ServerUnreachable;
        
        if (message.Contains("connection") && (message.Contains("lost") || message.Contains("reset")))
            return NetworkErrorType.ConnectionLost;
        
        if (message.Contains("network") || message.Contains("dns"))
            return NetworkErrorType.NoInternetConnection;
        
        return NetworkErrorType.ServerError;
    }

    /// <summary>
    /// Gets user-friendly error message based on HttpRequestException
    /// </summary>
    /// <param name="ex">HttpRequestException to analyze</param>
    /// <returns>User-friendly error message</returns>
    private string GetNetworkErrorMessage(HttpRequestException ex)
    {
        var errorType = DetermineNetworkErrorType(ex);
        
        return errorType switch
        {
            NetworkErrorType.NoInternetConnection => "No internet connection. Please check your network settings and try again.",
            NetworkErrorType.ServerUnreachable => "Server is currently unreachable. Please try again later.",
            NetworkErrorType.RequestTimeout => "Request timed out. Please check your connection and try again.",
            NetworkErrorType.ConnectionLost => "Connection was lost during the request. Please try again.",
            NetworkErrorType.SlowConnection => "Connection is slow. Please check your network and try again.",
            NetworkErrorType.ServerError => "Server error occurred. Please try again later.",
            _ => "Network error occurred. Please try again."
        };
    }

    /// <summary>
    /// Handles network errors by invoking the appropriate event
    /// </summary>
    /// <param name="errorType">Type of network error</param>
    /// <param name="message">Error message</param>
    private void HandleNetworkError(NetworkErrorType errorType, string message)
    {
        OnNetworkError?.Invoke(errorType, message);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Creates HTTP request message with proper content and headers.
    /// </summary>
    /// <param name="endpoint">API endpoint URL</param>
    /// <param name="httpMethod">HTTP method</param>
    /// <param name="requestData">Request payload data</param>
    /// <param name="headers">Custom headers to include</param>
    /// <param name="temporaryToken">Temporary authentication token to use instead of localStorage token</param>
    /// <returns>Configured HTTP request message</returns>
    private async Task<HttpRequestMessage> CreateHttpRequestMessage(
        string endpoint, 
        HttpMethod httpMethod, 
        object? requestData, 
        IDictionary<string, string>? headers,
        string? temporaryToken = null)
    {
        var requestMessage = new HttpRequestMessage(httpMethod, endpoint);

        if (httpMethod != HttpMethod.Get && requestData != null)
        {
            // Use configured JsonSerializerSettings instead of default serialization
            string jsonContent;
            if (_jsonSerializerSettings != null)
            {
                jsonContent = JsonConvert.SerializeObject(requestData, _jsonSerializerSettings);
            }
            else
            {
                jsonContent = JsonConvert.SerializeObject(requestData);
            }
            
            requestMessage.Content = new StringContent(jsonContent, Encoding.UTF8, JsonContentType);
        }

        await SetAuthorizationHeaderAsync(requestMessage, temporaryToken);

        if (headers != null)
        {
            foreach (var header in headers)
            {
                requestMessage.Headers.Add(header.Key, header.Value);
            }
        }

        return requestMessage;
    }

    /// <summary>
    /// Creates cancellation token source with specified timeout.
    /// </summary>
    /// <param name="timeoutSeconds">Timeout in seconds</param>
    /// <returns>Configured cancellation token source</returns>
    private CancellationTokenSource CreateCancellationTokenSource(int? timeoutSeconds)
    {
        var effectiveTimeout = timeoutSeconds ?? DefaultTimeoutSeconds;
        return new CancellationTokenSource(TimeSpan.FromSeconds(effectiveTimeout));
    }

    /// <summary>
    /// Sets authorization header for HTTP request if token is available and valid.
    /// </summary>
    /// <param name="requestMessage">HTTP request message to configure</param>
    /// <param name="temporaryToken">Temporary authentication token to use instead of localStorage token</param>
    private async Task SetAuthorizationHeaderAsync(HttpRequestMessage requestMessage, string? temporaryToken = null)
    {
        if (_localStorageService == null)
            return;

        string? token = temporaryToken ?? await _localStorageService.GetItem<string>(LocalStorageKeys.Token);
        
        if (string.IsNullOrEmpty(token))
            return;

        if (IsTokenExpired(token))
        {
            await _localStorageService.RemoveItem(LocalStorageKeys.Token);
            _navigationManager?.NavigateTo(SessionExpiredRoute);
            throw new UnauthorizedAccessException("Token is expired");
        }

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(BearerPrefix, token);
    }

    /// <summary>
    /// Handles unauthorized response by clearing token and updating authentication state.
    /// </summary>
    /// <param name="response">HTTP response to check</param>
    private async Task HandleUnauthorizedResponse(HttpResponseMessage response)
    {
        if (response.StatusCode != HttpStatusCode.Unauthorized || _localStorageService == null)
            return;

        await _localStorageService.RemoveItem(LocalStorageKeys.Token);
        // Consider uncommenting this when auth state provider is properly configured
        // await _authStateProvider.GetAuthenticationStateAsync();
    }

    /// <summary>
    /// Handles API response and returns string-based result.
    /// </summary>
    /// <param name="response">HTTP response message</param>
    /// <returns>API response with string data</returns>
    private async Task<ApiResponse<string>> HandleStringResponse(HttpResponseMessage response)
    {
        var responseBody = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            return new ApiResponse<string>(responseBody, (int)response.StatusCode, SuccessMessage);
        }
        else
        {
            var defaultErrorMessage = GetDefaultErrorMessage(response.StatusCode);
            return new ApiResponse<string>(responseBody, (int)response.StatusCode, defaultErrorMessage);
        }
    }

    /// <summary>
    /// Handles API response and returns typed result.
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    /// <param name="response">HTTP response message</param>
    /// <returns>API response with typed data</returns>
    private async Task<ApiResponse<T>> HandleTypedResponse<T>(HttpResponseMessage response)
    {
        var responseBody = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<ApiResponse<T>>(responseBody, _jsonSerializerSettings);
        }
        else
        {
            var defaultErrorMessage = GetDefaultErrorMessage(response.StatusCode);
            return new ApiResponse<T>(default, (int)response.StatusCode, defaultErrorMessage);
        }
    }

    /// <summary>
    /// Checks if the response body matches ApiResponse structure.
    /// </summary>
    /// <param name="responseBody">Response body to check</param>
    /// <returns>True if response matches ApiResponse structure</returns>
    private bool IsApiResponseStructure(string responseBody)
    {
        return !string.IsNullOrEmpty(responseBody) && 
               responseBody.Contains(DataProperty) && 
               responseBody.Contains(StatusCodeProperty);
    }

    /// <summary>
    /// Gets default error message based on HTTP status code.
    /// </summary>
    /// <param name="statusCode">HTTP status code</param>
    /// <returns>User-friendly error message</returns>
    private string GetDefaultErrorMessage(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.InternalServerError => InternalServerErrorMessage,
            HttpStatusCode.BadRequest => BadRequestErrorMessage,
            HttpStatusCode.Unauthorized => UnauthorizedErrorMessage,
            HttpStatusCode.NotFound => NotFoundErrorMessage,
            HttpStatusCode.Forbidden => ForbiddenErrorMessage,
            HttpStatusCode.MethodNotAllowed => MethodNotAllowedErrorMessage,
            HttpStatusCode.TooManyRequests => TooManyRequestsErrorMessage,
            _ => ApiRequestFailedMessage
        };
    }

    /// <summary>
    /// Checks if JWT token is expired.
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>True if token is expired or invalid, false otherwise</returns>
    private bool IsTokenExpired(string token)
    {
        if (string.IsNullOrEmpty(token))
            return true;

        var tokenHandler = new JwtSecurityTokenHandler();
        
        try
        {
            var securityToken = tokenHandler.ReadToken(token);
            
            if (securityToken is not JwtSecurityToken jwtToken)
                return true; // Not a valid JWT token

            return jwtToken.ValidTo < DateTime.UtcNow;
        }
        catch (ArgumentException)
        {
            return true; // Treat parsing errors as expired for safety
        }
        catch (SecurityTokenException)
        {
            return true; // Treat validation errors as expired for safety
        }
    }

    #endregion
}
