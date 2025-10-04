using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using WINITSharedObjects.Models;
namespace WINITMobile.Data
{
    public class DatabaseUploadService
    {
        private readonly ApiService _apiService;
        private readonly Winit.Shared.Models.Common.IAppConfig _appConfig;
        private readonly IAppUser _appUser;
        private readonly Winit.Modules.Base.DL.DBManager.SqliteDBManager _sqlite;
        Winit.Modules.FileSys.BL.Interfaces.IFileSysBL _fileSysBL { set; get; }
        public DatabaseUploadService(ApiService apiService, Winit.Shared.Models.Common.IAppConfig appConfig,
            IAppUser appUser, Winit.Modules.Base.DL.DBManager.SqliteDBManager sqlite, Winit.Modules.FileSys.BL.Interfaces.IFileSysBL fileSysBL)
        {
            _apiService = apiService;
            _appConfig = appConfig;
            _appUser = appUser;
            _sqlite = sqlite;
            _fileSysBL = fileSysBL;
        }

        public async Task<bool> UploadDatabaseToServer(bool clearDataAfterUpload = false, string caller = "")
        {
            try
            {
                string userCode = GetUserCode();
                string folderPath = await PrepareUploadFolderPath(userCode);
                string serverFolderPath = FileSysTemplateControles.GetUploadDebugLogFolderPath(_appUser.Emp.Code);
                string timestamp = GetFormattedTimestamp();
                string deviceInfo = GetDeviceInfo();
                string zipFileName = $"{userCode}_{deviceInfo}_{timestamp}.zip";

                // Create temp folder for files
                string tempFolderPath = Path.Combine(folderPath, "Temp");
                await CreateFolderIfNotExists(tempFolderPath);

                // Copy SQLite file to temp folder
                string dbSourcePath = Path.Combine(FileSystem.AppDataDirectory, MauiProgram.CurrentProjectName, "Data", "DB", "WINITSQLite.db");
                string dbDestFileName = $"{userCode}_SalesmanDb_{timestamp}.sqlite";
                await _sqlite.EnsureDatabaseReleasedAsync(dbSourcePath);
                await CopyFileAsync(dbSourcePath, Path.Combine(tempFolderPath, dbDestFileName));

                // Compress the folder to a zip file
                string zipFilePath = Path.Combine(folderPath, zipFileName);
                await CompressFolder(tempFolderPath, zipFilePath);

                // Delete temp folder after compression
                await DeleteFolderAsync(tempFolderPath);

                // Upload zip file to server
                bool uploadSuccess = await UploadFileToServer(serverFolderPath, folderPath, zipFilePath);

                // Clear data if requested
                if (clearDataAfterUpload && uploadSuccess)
                {
                    await DeleteFileAsync(dbSourcePath);
                }

                return uploadSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading database: {ex.Message}");
                // Log error
                return false;
            }
        }

        private string GetUserCode()
        {
            if (_appUser?.Emp != null)
            {
                if (!string.IsNullOrEmpty(_appUser.Emp.LoginId))
                {
                    return _appUser.Emp.LoginId;
                }
                //else if (!string.IsNullOrEmpty(_appUser.OldUserName))
                //{
                //    return _appUser.OldUserName;
                //}
            }

            return "Unknown";
        }

        private async Task<string> PrepareUploadFolderPath(string userCode)
        {
            string folderPath = Path.Combine(_appConfig.BaseFolderPath, "Uploads", userCode);
            await CreateFolderIfNotExists(folderPath);
            return folderPath;
        }

        private string GetFormattedTimestamp()
        {
            return DateTime.Now.ToString("ddMMMyyyy_HH_mm_ss");
        }

        private string GetDeviceInfo()
        {
            // You might want to implement this based on your device info retrieval mechanism
            return DeviceInfo.Current.Model;
        }

        private async Task CreateFolderIfNotExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            await Task.CompletedTask;
        }

        private async Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            if (File.Exists(sourcePath))
            {
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
                using (var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }
            }
        }

        private async Task CompressFolder(string sourceFolderPath, string destinationZipPath)
        {
            if (File.Exists(destinationZipPath))
            {
                File.Delete(destinationZipPath);
            }

            ZipFile.CreateFromDirectory(sourceFolderPath, destinationZipPath);
            await Task.CompletedTask;
        }

        private async Task DeleteFolderAsync(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
            await Task.CompletedTask;
        }

        private async Task DeleteFileAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            await Task.CompletedTask;
        }

        private async Task<bool> UploadFileToServer(string serverFolderPath, string folderPath, string filePath)
        {
            try
            {
                using (var formData = new MultipartFormDataContent())
                {
                    // Add the zip file
                    var fileBytes = await File.ReadAllBytesAsync(filePath);
                    var fileContent = new ByteArrayContent(fileBytes);
                    var fileName = Path.GetFileName(filePath);

                    fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "\"file\"",
                        FileName = $"\"{fileName}\""
                    };

                    formData.Add(fileContent);

                    // Add folder path parameter
                    //formData.Add(new StringContent(folderPath), "folderPath");
                    //Add server folder path parameter
                    formData.Add(new StringContent(serverFolderPath), "folderPath");
                    // Send request to server
                    var response = await _apiService.UploadFileAsync($"{_appConfig.ApiBaseUrl}FileUpload/UploadFile", formData);

                    if (response.IsSuccess)
                    {
                        try
                        {
                            // Try to extract the server path from the response
                            var responseBody = response.Data;
                            var uploadResponse = JsonSerializer.Deserialize<ImageUploadResponse>(responseBody);
                            if (uploadResponse != null && uploadResponse.Status == ImageUploadStatus.SUCCESS)
                            {
                                string serverPath = uploadResponse.SavedImgsPath.FirstOrDefault();

                                // Create FileSys object for database insert
                                var fileSys = new Winit.Modules.FileSys.Model.Classes.FileSys
                                {
                                    Id = 0,
                                    UID = Guid.NewGuid().ToString(),
                                    LinkedItemType = "DebugLog",
                                    LinkedItemUID = _appUser?.Emp?.UID ?? "",
                                    FileSysType = "DebugLog",
                                    FileType = "Database",
                                    IsDirectory = false,
                                    FileName = fileName,
                                    DisplayName = fileName,
                                    FileSize = new FileInfo(filePath).Length,
                                    RelativePath = serverPath,
                                    TempPath = serverPath,
                                    Latitude = "0",
                                    Longitude = "0",
                                    CreatedBy = _appUser?.Emp?.UID ?? "",
                                    ModifiedBy = _appUser?.Emp?.UID ?? "",
                                    CreatedTime = DateTime.Now,
                                    ModifiedTime = DateTime.Now,
                                    ServerAddTime = DateTime.Now,
                                    ServerModifiedTime = DateTime.Now,
                                    SS = 1
                                };

                                // Insert into FileSys table
                                if (await _fileSysBL.CreateFileSys(fileSys) > 0)
                                {
                                    // Delete the local file after successful upload and database insert
                                    if (File.Exists(filePath))
                                    {
                                        File.Delete(filePath);
                                    }
                                    Console.WriteLine($"Successfully uploaded and recorded: {fileName}");
                                    return true;
                                }
                                else
                                {
                                    Console.WriteLine($"Failed to insert FileSys record for: {fileName}");
                                    return false;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to parse upload response: " + ex.Message);
                            return false;
                        }
                    }
                    Console.WriteLine($"Failed to upload: {fileName}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                return false;
            }
        }

        //private async Task<bool> UploadFileToServer(string serverFolderPath, string filePath, string fileName)
        //{
        //    try
        //    {
        //        using (var formData = new MultipartFormDataContent())
        //        {
        //            // Add the zip file
        //            var fileBytes = await File.ReadAllBytesAsync(filePath);
        //            var fileContent = new ByteArrayContent(fileBytes);

        //            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        //            {
        //                Name = "\"file\"",
        //                FileName = $"\"{fileName}\""
        //            };

        //            formData.Add(fileContent);

        //            // Add server folder path parameter
        //            formData.Add(new StringContent(serverFolderPath), "folderPath");

        //            // Send request to server
        //            var response = await _apiService.UploadFileAsync($"{_appConfig.ApiBaseUrl}FileUpload/UploadFile", formData);

        //            if (response.IsSuccess)
        //            {
        //                try
        //                {
        //                    // Try to extract the server path from the response
        //                    var responseBody = response.Data;
        //                    var uploadResponse = JsonSerializer.Deserialize<ImageUploadResponse>(responseBody);
        //                    if (uploadResponse != null && uploadResponse.Status == ImageUploadStatus.SUCCESS)
        //                    {
        //                        string serverPath = uploadResponse.SavedImgsPath.FirstOrDefault();

        //                        // Create FileSys object for database insert
        //                        var fileSys = new Winit.Modules.FileSys.Model.Classes.FileSys
        //                        {
        //                            Id = 0,
        //                            UID = Guid.NewGuid().ToString(),
        //                            LinkedItemType = "DebugLog",
        //                            LinkedItemUID = _appUser?.Emp?.UID ?? "",
        //                            FileSysType = "DebugLog",
        //                            FileType = "Database",
        //                            IsDirectory = false,
        //                            FileName = fileName,
        //                            DisplayName = fileName,
        //                            FileSize = new FileInfo(filePath).Length,
        //                            RelativePath = serverPath,
        //                            TempPath = serverPath,
        //                            Latitude = "0",
        //                            Longitude = "0",
        //                            CreatedBy = _appUser?.Emp?.UID ?? "",
        //                            ModifiedBy = _appUser?.Emp?.UID ?? "",
        //                            CreatedTime = DateTime.Now,
        //                            ModifiedTime = DateTime.Now,
        //                            ServerAddTime = DateTime.Now,
        //                            ServerModifiedTime = DateTime.Now,
        //                            SS = 0
        //                        };

        //                        // Insert into FileSys table
        //                        if (await _fileSysBL.CreateFileSys(fileSys) > 0)
        //                        {
        //                            // Delete the local file after successful upload and database insert
        //                            if (File.Exists(filePath))
        //                            {
        //                                File.Delete(filePath);
        //                            }
        //                            Console.WriteLine($"Successfully uploaded and recorded: {fileName}");
        //                            return true;
        //                        }
        //                        else
        //                        {
        //                            Console.WriteLine($"Failed to insert FileSys record for: {fileName}");
        //                            return false;
        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine("Failed to parse upload response: " + ex.Message);
        //                    return false;
        //                }
        //            }

        //            Console.WriteLine($"Failed to upload: {fileName}");
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Upload failed: {ex.Message}");
        //        return false;
        //    }
        //}
    }
}

