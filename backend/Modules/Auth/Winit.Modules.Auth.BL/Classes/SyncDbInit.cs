using Winit.Modules.Auth.BL.Interfaces;
using Winit.Modules.Auth.Model.Constants;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Mobile.Model.Classes;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Shared.Models.Common;
using System.IO.Compression;
using System.Net.Sockets;

namespace Winit.Modules.Auth.BL.Classes;

/// <summary>
/// Manages SQLite database synchronization initialization process including 
/// database creation requests, status monitoring, and file download with progress reporting.
/// </summary>
public class SyncDbInit
{
    #region Constants

    private const int MaxRetryAttempts = 60; // 5 minutes with 5-second intervals
    private const int RetryIntervalSeconds = 5;
    private const int StatusCheckTimeoutMinutes = 5;
    private const string TempZipFileName = "downloaded_sqlite.zip";
    
    // Download retry constants
    private const int MaxDownloadRetryAttempts = 5;
    private const int DownloadRetryDelaySeconds = 3;

    #endregion

    #region Private Fields

    private readonly ISyncViewModel _syncViewModel;
    private readonly IAppUser _appUser;
    private readonly IAppConfig _appConfigs;
    private readonly ApiService _apiService;

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the current operation status.
    /// </summary>
    public DatabaseInitializationStatus Status { get; private set; } = DatabaseInitializationStatus.NotStarted;

    /// <summary>
    /// Gets the error message if operation failed.
    /// </summary>
    public string ErrorMessage { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the SQLite database file path if operation succeeded.
    /// </summary>
    public string SqlitePath { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess => Status == DatabaseInitializationStatus.Success;

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailed => Status == DatabaseInitializationStatus.Failed;

    /// <summary>
    /// Gets a value indicating whether the operation is in progress.
    /// </summary>
    public bool IsInProgress => Status == DatabaseInitializationStatus.InProgress;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the SyncDbInit class.
    /// </summary>
    /// <param name="syncViewModel">The sync view model</param>
    /// <param name="appUser">The current application user</param>
    /// <param name="appConfig">The application configuration</param>
    /// <param name="apiService">The API service for making HTTP requests</param>
    public SyncDbInit(ISyncViewModel syncViewModel, IAppUser appUser, IAppConfig appConfig, ApiService apiService)
    {
        _syncViewModel = syncViewModel ?? throw new ArgumentNullException(nameof(syncViewModel));
        _appUser = appUser ?? throw new ArgumentNullException(nameof(appUser));
        _appConfigs = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initiates the complete database creation and download process.
    /// This method orchestrates the entire workflow from initiation to file download.
    /// </summary>
    /// <param name="progressCallback">Optional callback to report progress during download</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task<bool> InitiateDbCreationAsync(
        IProgress<DatabaseInitializationProgress> progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Status = DatabaseInitializationStatus.InProgress;
            ErrorMessage = string.Empty;
            SqlitePath = string.Empty;

            progressCallback?.Report(new DatabaseInitializationProgress
            {
                Stage = "Validating prerequisites",
                PercentageComplete = 0,
                Message = "Checking prerequisites for database creation..."
            });

            // Validate prerequisites
            if (!ValidatePrerequisites())
            {
                return false;
            }

            progressCallback?.Report(new DatabaseInitializationProgress
            {
                Stage = "Initiating database creation",
                PercentageComplete = 0,
                Message = "Requesting database creation from server..."
            });

            // Step 1: Initiate database creation
            bool initiationSuccess = await InitiateDbCreationApiAsync(cancellationToken);
            if (!initiationSuccess)
            {
                SetFailureStatus("Unable to initiate database creation. Please try again.");
                return false;
            }

            progressCallback?.Report(new DatabaseInitializationProgress
            {
                Stage = "Waiting for database creation",
                PercentageComplete = 0,
                Message = "Database creation requested. Waiting for server to complete..."
            });

            // Step 2: Monitor database creation status
            string downloadUrl = await MonitorDbCreationStatusAsync(progressCallback, cancellationToken);
            if (string.IsNullOrEmpty(downloadUrl))
            {
                return false;
            }

            progressCallback?.Report(new DatabaseInitializationProgress
            {
                Stage = "Downloading database",
                PercentageComplete = 0,
                Message = "Starting database download..."
            });

            // Step 3: Download and extract database
            string extractedFilePath = await DownloadAndExtractDatabaseAsync(
                downloadUrl, 
                progressCallback, 
                cancellationToken);

            if (string.IsNullOrEmpty(extractedFilePath))
            {
                SetFailureStatus("Failed to download or extract database file.");
                return false;
            }

            // Step 4: Finalize success
            SqlitePath = extractedFilePath;
            Status = DatabaseInitializationStatus.Success;
            _syncViewModel.SqlitePath = SqlitePath;
            _syncViewModel.IsValid = true;

            progressCallback?.Report(new DatabaseInitializationProgress
            {
                Stage = "Completed",
                PercentageComplete = 100,
                Message = "Database initialization completed successfully."
            });

            return true;
        }
        catch (OperationCanceledException)
        {
            SetFailureStatus("Operation was cancelled.");
            return false;
        }
        catch (Exception ex)
        {
            SetFailureStatus($"An unexpected error occurred: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Downloads a file from the specified URL with progress reporting and extracts it if it's a ZIP file.
    /// </summary>
    /// <param name="fileUrl">The URL of the file to download</param>
    /// <param name="destinationFolderPath">The folder path where the file should be saved</param>
    /// <param name="extractedFileName">The desired name for the extracted file</param>
    /// <param name="progressCallback">Optional callback to report download progress</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The path to the extracted file, or null if operation failed</returns>
    public async Task<string> DownloadAndExtractFileAsync(
        string fileUrl,
        string destinationFolderPath,
        string extractedFileName,
        IProgress<DownloadProgress> progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create destination directory if it doesn't exist
            if (!Directory.Exists(destinationFolderPath))
            {
                Directory.CreateDirectory(destinationFolderPath);
            }

            string zipFilePath = Path.Combine(destinationFolderPath, TempZipFileName);

            // Download the file
            await DownloadFileWithProgressAsync(fileUrl, zipFilePath, progressCallback, cancellationToken);

            // Extract the file
            string extractedFilePath = await ExtractZipFileAsync(
                zipFilePath, 
                destinationFolderPath, 
                extractedFileName, 
                cancellationToken);

            // Clean up temporary ZIP file
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            return extractedFilePath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to download and extract file from {fileUrl}: {ex.Message}", ex);
        }
    }



    #endregion

    #region Private Methods

    /// <summary>
    /// Validates prerequisites for database creation.
    /// </summary>
    /// <returns>True if prerequisites are met, false otherwise</returns>
    private bool ValidatePrerequisites()
    {
        if (_appUser?.Emp?.UID == null || _appUser?.SelectedJobPosition?.UID == null || _appUser?.Role?.UID == null)
        {
            SetFailureStatus("Required user information is missing. Please ensure you are properly logged in.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Initiates database creation via API call.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if initiation was successful, false otherwise</returns>
    private async Task<bool> InitiateDbCreationApiAsync(CancellationToken cancellationToken)
    {
        try
        {
            string employeeUID = _appUser.Emp?.UID;
            string jobPositionUID = _appUser.SelectedJobPosition?.UID;
            string roleUID = _appUser.Role?.UID;
            string orgUID = _appUser.SelectedJobPosition?.OrgUID;
            string vehicleUID = _appUser.Vehicle?.UID;
            string empCode = _appUser.Emp?.Code;

            string apiUrl = $"{_appConfigs.ApiBaseUrl}MobileAppAction/InitiateDBCreation" +
                          $"?employeeUID={Uri.EscapeDataString(employeeUID ?? string.Empty)}" +
                          $"&jobPositionUID={Uri.EscapeDataString(jobPositionUID ?? string.Empty)}" +
                          $"&roleUID={Uri.EscapeDataString(roleUID ?? string.Empty)}" +
                          $"&orgUID={Uri.EscapeDataString(orgUID ?? string.Empty)}" +
                          $"&vehicleUID={Uri.EscapeDataString(vehicleUID ?? string.Empty)}" +
                          $"&empCode={Uri.EscapeDataString(empCode ?? string.Empty)}";

            ApiResponse<int> apiResponse = await _apiService.FetchDataAsync<int>(apiUrl, HttpMethod.Post);

            return apiResponse?.IsSuccess == true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initiate database creation: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Monitors database creation status with explicit retry logic and timeout protection.
    /// Polls the server until database is ready, fails, or timeout is reached.
    /// </summary>
    /// <param name="progressCallback">Progress callback for status updates (not actual progress)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download URL if successful, null if failed</returns>
    private async Task<string> MonitorDbCreationStatusAsync(
        IProgress<DatabaseInitializationProgress> progressCallback,
        CancellationToken cancellationToken)
    {
        int attemptCount = 0;
        DateTime operationStartTime = DateTime.UtcNow;

        // Keep checking status until ready, failed, or timeout
        while (attemptCount < MaxRetryAttempts)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // TIMEOUT PROTECTION: Prevent infinite waiting if server gets stuck
                // This is a safety mechanism in case the server never responds or database creation hangs
                TimeSpan elapsedTime = DateTime.UtcNow - operationStartTime;
                if (elapsedTime > TimeSpan.FromMinutes(StatusCheckTimeoutMinutes))
                {
                    SetFailureStatus($"Database creation timed out after {StatusCheckTimeoutMinutes} minutes. Please try again.");
                    return null;
                }

                // Check current database creation status from server
                ISqlitePreparation sqlitePreparation = await GetDbCreationStatusAsync(cancellationToken);

                // NOTE: sqlitePreparation should never be null because InitiateDBCreation creates a record
                // Only check for null in case of unexpected API/network errors
                if (sqlitePreparation == null)
                {
                    // This is an unexpected error - API should always return the status record
                    SetFailureStatus("Unexpected error: Unable to retrieve database creation status from server.");
                    return null;
                }

                // Process the status response - this is where the actual retry logic happens
                switch (sqlitePreparation.Status)
                {
                    case SqlitePreparationStatus.READY:
                        // SUCCESS: Database is ready for download
                        progressCallback?.Report(new DatabaseInitializationProgress
                        {
                            Stage = "Database ready",
                            PercentageComplete = 0, // Ready to start download
                            Message = "Database created successfully. Starting download..."
                        });
                        return sqlitePreparation.SqlitePath;

                    case SqlitePreparationStatus.NOT_READY:
                    case SqlitePreparationStatus.IN_PROCESS:
                        // STILL PROCESSING: Wait and check again
                        attemptCount++;
                        
                        // Simple status update - no fake progress during server processing
                        progressCallback?.Report(new DatabaseInitializationProgress
                        {
                            Stage = "Waiting for database creation",
                            PercentageComplete = 0, // Still waiting
                            Message = $"Database creation in progress..."
                            //Message = $"Database creation in progress... (Checking attempt {attemptCount}/{MaxRetryAttempts})"
                        });

                        // RETRY LOGIC: Wait before checking status again
                        await Task.Delay(TimeSpan.FromSeconds(RetryIntervalSeconds), cancellationToken);
                        continue; // RETRY: Go back to start of while loop

                    case SqlitePreparationStatus.FAILURE:
                        // FAILURE: Server reported database creation failed
                        string failureMessage = !string.IsNullOrEmpty(sqlitePreparation.ErrorMessage)
                            ? sqlitePreparation.ErrorMessage
                            : "Database creation failed on server.";
                        SetFailureStatus(failureMessage);
                        return null;

                    default:
                        // UNKNOWN STATUS: Unexpected response from server
                        SetFailureStatus($"Unknown database creation status: {sqlitePreparation.Status}");
                        return null;
                }
            }
            catch (OperationCanceledException)
            {
                // User cancelled or timeout - let it propagate
                throw;
            }
            catch (Exception ex)
            {
                // NETWORK/API ERROR: Count as failed attempt and retry
                attemptCount++;
                
                if (attemptCount >= MaxRetryAttempts)
                {
                    SetFailureStatus($"Failed to check database creation status after {MaxRetryAttempts} attempts: {ex.Message}");
                    return null;
                }
                
                // RETRY LOGIC: Wait before retrying after error
                await Task.Delay(TimeSpan.FromSeconds(RetryIntervalSeconds), cancellationToken);
                continue; // RETRY: Go back to start of while loop
            }
        }

        // If we exit the while loop, we've exceeded max attempts
        SetFailureStatus($"Database creation status check exceeded maximum attempts ({MaxRetryAttempts}). Please try again.");
        return null;
    }

    /// <summary>
    /// Retrieves database creation status from the API.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SQLite preparation status information</returns>
    private async Task<ISqlitePreparation> GetDbCreationStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            string employeeUID = _appUser.Emp?.UID;
            string jobPositionUID = _appUser.SelectedJobPosition?.UID;
            string roleUID = _appUser.Role?.UID;

            string apiUrl = $"{_appConfigs.ApiBaseUrl}MobileAppAction/GetDBCreationStatus" +
                          $"?employeeUID={Uri.EscapeDataString(employeeUID ?? string.Empty)}" +
                          $"&jobPositionUID={Uri.EscapeDataString(jobPositionUID ?? string.Empty)}" +
                          $"&roleUID={Uri.EscapeDataString(roleUID ?? string.Empty)}";

            ApiResponse<SqlitePreparation> apiResponse = await _apiService.FetchDataAsync<SqlitePreparation>(
                apiUrl, HttpMethod.Get);

            return apiResponse?.IsSuccess == true ? apiResponse.Data : null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get database creation status: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Downloads and extracts the database file.
    /// </summary>
    /// <param name="downloadUrl">URL to download the database from</param>
    /// <param name="progressCallback">Progress callback for initialization</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Path to the extracted database file</returns>
    private async Task<string> DownloadAndExtractDatabaseAsync(
        string downloadUrl,
        IProgress<DatabaseInitializationProgress> progressCallback,
        CancellationToken cancellationToken)
    {
        try
        {
            // Use app configuration base folder path (not platform-specific FileSystem API)
            string folderPath = Path.Combine(_appConfigs.BaseFolderPath, "DB");
            string databaseFileName = "WINITSQLite.db";

            // Create progress wrapper for download
            var downloadProgress = new Progress<DownloadProgress>(progress =>
            {
                if (progress.IsRetrying)
                {
                    // Show retry message
                    progressCallback?.Report(new DatabaseInitializationProgress
                    {
                        Stage = "Downloading database",
                        PercentageComplete = 0,
                        Message = $"Network issue detected. Retrying download... (Attempt {progress.RetryAttempt + 1}/{MaxDownloadRetryAttempts + 1})"
                    });
                }
                else
                {
                    // Map download progress directly (0% to 90% for download, 90-100% for extraction)
                    int totalPercentage = (int)(progress.PercentageComplete * 0.9);
                    
                    string retryInfo = progress.RetryAttempt > 0 ? $" [Retry {progress.RetryAttempt + 1}]" : "";
                    
                    progressCallback?.Report(new DatabaseInitializationProgress
                    {
                        Stage = "Downloading database",
                        PercentageComplete = totalPercentage,
                        Message = $"Downloading database... {totalPercentage}% " +
                                 $"({FormatBytes(progress.BytesReceived)} / {FormatBytes(progress.TotalBytes)}){retryInfo}"
                    });
                }
            });

            progressCallback?.Report(new DatabaseInitializationProgress
            {
                Stage = "Extracting database",
                PercentageComplete = 90,
                Message = "Extracting database file..."
            });

            string extractedFilePath = await DownloadAndExtractFileAsync(
                downloadUrl,
                folderPath,
                databaseFileName,
                downloadProgress,
                cancellationToken);

            return extractedFilePath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to download and extract database: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Downloads a file from URL with progress reporting and automatic retry logic for network failures.
    /// </summary>
    /// <param name="fileUrl">URL of the file to download</param>
    /// <param name="destinationPath">Local path where file should be saved</param>
    /// <param name="progressCallback">Progress callback</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Task DownloadFileWithProgressAsync(
        string fileUrl,
        string destinationPath,
        IProgress<DownloadProgress> progressCallback,
        CancellationToken cancellationToken)
    {
        int retryCount = 0;
        Exception lastException = null;

        while (retryCount <= MaxDownloadRetryAttempts)
        {
            try
            {
                // Report retry attempt if this is not the first try
                if (retryCount > 0)
                {
                    progressCallback?.Report(new DownloadProgress
                    {
                        BytesReceived = 0,
                        TotalBytes = 0,
                        PercentageComplete = 0,
                        RetryAttempt = retryCount,
                        IsRetrying = true
                    });
                }

                await PerformDownloadAsync(fileUrl, destinationPath, progressCallback, cancellationToken);
                return; // Success - exit the retry loop
            }
            catch (Exception ex) when (IsRetryableException(ex) && retryCount < MaxDownloadRetryAttempts)
            {
                lastException = ex;
                retryCount++;
                
                // Wait before retrying (with exponential backoff)
                int delaySeconds = DownloadRetryDelaySeconds * retryCount;
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                
                // Clean up partially downloaded file before retry
                if (File.Exists(destinationPath))
                {
                    try
                    {
                        File.Delete(destinationPath);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
            catch (Exception ex)
            {
                // Non-retryable exception or max retries exceeded
                throw new InvalidOperationException($"Download failed after {retryCount} attempts: {ex.Message}", ex);
            }
        }

        // If we get here, we've exceeded max retries
        throw new InvalidOperationException(
            $"Download failed after {MaxDownloadRetryAttempts} retry attempts. Last error: {lastException?.Message}", 
            lastException);
    }

    /// <summary>
    /// Performs the actual file download operation.
    /// </summary>
    private async Task PerformDownloadAsync(
        string fileUrl,
        string destinationPath,
        IProgress<DownloadProgress> progressCallback,
        CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(10); // 10-minute timeout for large files

        using var response = await httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        long? totalBytes = response.Content.Headers.ContentLength;
        long bytesReceived = 0;

        using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        byte[] buffer = new byte[8192]; // 8KB buffer
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            bytesReceived += bytesRead;

            if (totalBytes.HasValue && progressCallback != null)
            {
                double percentage = (bytesReceived / (double)totalBytes.Value) * 100;
                progressCallback.Report(new DownloadProgress
                {
                    BytesReceived = bytesReceived,
                    TotalBytes = totalBytes.Value,
                    PercentageComplete = percentage
                });
            }
        }
    }

    /// <summary>
    /// Determines if an exception is retryable (network-related issues).
    /// </summary>
    /// <param name="exception">The exception to check</param>
    /// <returns>True if the exception indicates a retryable network issue</returns>
    private static bool IsRetryableException(Exception exception)
    {
        return exception switch
        {
            HttpRequestException => true,           // Network connectivity issues
            TaskCanceledException tce => !tce.CancellationToken.IsCancellationRequested, // Timeout (not user cancellation)
            SocketException => true,               // Socket-level network errors
            IOException => true,                   // I/O errors during network operations
            _ => false                            // Other exceptions are not retryable
        };
    }

    /// <summary>
    /// Extracts a ZIP file to the specified destination.
    /// </summary>
    /// <param name="zipFilePath">Path to the ZIP file</param>
    /// <param name="destinationFolder">Destination folder for extraction</param>
    /// <param name="extractedFileName">Desired name for the extracted file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Path to the extracted file</returns>
    private async Task<string> ExtractZipFileAsync(
        string zipFilePath,
        string destinationFolder,
        string extractedFileName,
        CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            using var archive = ZipFile.OpenRead(zipFilePath);
            
            // Find the first non-directory entry (should be the database file)
            var databaseEntry = archive.Entries.FirstOrDefault(entry => 
                !string.IsNullOrEmpty(entry.Name) && !entry.FullName.EndsWith("/"));

            if (databaseEntry == null)
            {
                throw new InvalidOperationException("No valid database file found in the ZIP archive.");
            }

            string finalExtractedFilePath = Path.Combine(destinationFolder, extractedFileName);

            // Remove existing file if it exists
            if (File.Exists(finalExtractedFilePath))
            {
                File.Delete(finalExtractedFilePath);
            }

            // Extract the database file
            databaseEntry.ExtractToFile(finalExtractedFilePath, true);
        }, cancellationToken);

        return Path.Combine(destinationFolder, extractedFileName);
    }

    /// <summary>
    /// Sets the failure status with the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    private void SetFailureStatus(string errorMessage)
    {
        Status = DatabaseInitializationStatus.Failed;
        ErrorMessage = errorMessage;
        _syncViewModel.IsValid = false;
        _syncViewModel.ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Formats bytes into human-readable format.
    /// </summary>
    /// <param name="bytes">Number of bytes</param>
    /// <returns>Formatted string (e.g., "1.5 MB")</returns>
    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        
        return $"{number:N1} {suffixes[counter]}";
    }

    #endregion
}

/// <summary>
/// Represents the status of database initialization operation.
/// </summary>
public enum DatabaseInitializationStatus
{
    /// <summary>
    /// Operation has not started yet.
    /// </summary>
    NotStarted,

    /// <summary>
    /// Operation is currently in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// Operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// Operation failed with an error.
    /// </summary>
    Failed
}

/// <summary>
/// Represents progress information for database initialization.
/// </summary>
public class DatabaseInitializationProgress
{
    /// <summary>
    /// Current stage of the initialization process.
    /// </summary>
    public string Stage { get; set; } = string.Empty;

    /// <summary>
    /// Percentage complete (0-100).
    /// </summary>
    public int PercentageComplete { get; set; }

    /// <summary>
    /// Detailed progress message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Represents progress information for file download.
/// </summary>
public class DownloadProgress
{
    /// <summary>
    /// Number of bytes received so far.
    /// </summary>
    public long BytesReceived { get; set; }

    /// <summary>
    /// Total number of bytes to download.
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// Download progress as percentage (0-100).
    /// </summary>
    public double PercentageComplete { get; set; }

    /// <summary>
    /// Current retry attempt number (0 for first attempt).
    /// </summary>
    public int RetryAttempt { get; set; }

    /// <summary>
    /// Indicates if this progress report is for a retry attempt.
    /// </summary>
    public bool IsRetrying { get; set; }
} 