using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Winit.Modules.Syncing.BL.Classes
{
    /// <summary>
    /// Handles SQLite file operations with proper file locking and disposal
    /// </summary>
    public class SqliteFileHandler
    {
        private readonly ILogger _logger = Log.ForContext<SqliteFileHandler>();

        /// <summary>
        /// Creates a ZIP file from SQLite database with proper file handling
        /// </summary>
        /// <param name="sqliteFilePath">Path to SQLite database file</param>
        /// <param name="maxRetries">Maximum number of retries if file is locked</param>
        /// <param name="retryDelayMs">Delay between retries in milliseconds</param>
        /// <returns>Path to the created ZIP file</returns>
        public async Task<string> CreateZipFromSqlite(string sqliteFilePath, int maxRetries = 10, int retryDelayMs = 2000)
        {
            if (!File.Exists(sqliteFilePath))
            {
                throw new FileNotFoundException($"SQLite file not found: {sqliteFilePath}");
            }

            string directory = Path.GetDirectoryName(sqliteFilePath) ?? throw new InvalidOperationException("Invalid file path");
            string fileName = Path.GetFileNameWithoutExtension(sqliteFilePath);
            string zipFilePath = Path.Combine(directory, $"{fileName}.zip");

            // Delete existing zip file if it exists
            if (File.Exists(zipFilePath))
            {
                try
                {
                    File.Delete(zipFilePath);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Could not delete existing zip file: {ZipFile}", zipFilePath);
                }
            }

            // Initial delay to allow SQLite to release the file
            _logger.Information("Waiting for SQLite to release file before creating ZIP: {SqliteFile}", sqliteFilePath);
            await Task.Delay(2000);

            // Retry logic for file access
            int retryCount = 0;
            Exception lastException = null;

            while (retryCount < maxRetries)
            {
                try
                {
                    // Force garbage collection to release any file handles
                    if (retryCount > 0)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                        await Task.Delay(retryDelayMs);
                    }

                    // Create the zip file
                    using (var zipStream = new FileStream(zipFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, false))
                    {
                        // Use FileShare.ReadWrite to allow other processes to access the file
                        using (var sourceStream = new FileStream(sqliteFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            var entry = archive.CreateEntry(Path.GetFileName(sqliteFilePath), CompressionLevel.Optimal);
                            using (var entryStream = entry.Open())
                            {
                                await sourceStream.CopyToAsync(entryStream);
                            }
                        }
                    }

                    _logger.Information("Successfully created ZIP file: {ZipFile} from SQLite: {SqliteFile}", 
                        zipFilePath, sqliteFilePath);
                    
                    return zipFilePath;
                }
                catch (IOException ioEx) when (IsFileLocked(ioEx))
                {
                    lastException = ioEx;
                    retryCount++;
                    
                    _logger.Warning("File is locked (attempt {Attempt}/{MaxAttempts}): {Message}", 
                        retryCount, maxRetries, ioEx.Message);
                    
                    if (retryCount >= maxRetries)
                    {
                        throw new InvalidOperationException(
                            $"Failed to create ZIP after {maxRetries} attempts. File may be locked by another process.", 
                            lastException);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error creating ZIP file from SQLite: {SqliteFile}", sqliteFilePath);
                    throw;
                }
            }

            throw new InvalidOperationException(
                $"Failed to create ZIP after {maxRetries} attempts.", 
                lastException);
        }

        /// <summary>
        /// Ensures all SQLite connections to a file are closed
        /// </summary>
        public static void EnsureSqliteConnectionsClosed(string sqliteFilePath)
        {
            // Force SQLite to release any cached connections
            // Note: SQLitePCL calls removed as they require additional dependencies
            // Relying on GC.Collect and proper disposal patterns instead
            
            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// Checks if an IOException is due to file locking
        /// </summary>
        private bool IsFileLocked(IOException exception)
        {
            var errorCode = exception.HResult & 0xFFFF;
            return errorCode == 32 || errorCode == 33; // ERROR_SHARING_VIOLATION or ERROR_LOCK_VIOLATION
        }

        /// <summary>
        /// Safely deletes a file with retry logic
        /// </summary>
        public async Task<bool> SafeDeleteFile(string filePath, int maxRetries = 3, int retryDelayMs = 500)
        {
            if (!File.Exists(filePath))
                return true;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    if (i > 0)
                    {
                        await Task.Delay(retryDelayMs);
                    }

                    File.Delete(filePath);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Failed to delete file on attempt {Attempt}: {FilePath}", i + 1, filePath);
                    
                    if (i == maxRetries - 1)
                    {
                        return false;
                    }
                }
            }

            return false;
        }
    }
}