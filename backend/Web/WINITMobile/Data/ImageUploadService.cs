using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.FileSys.BL.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using WINITSharedObjects.Models;

namespace WINITMobile.Data;

public class ImageUploadService : IImageUploadService
{
    public IFileSysBL _fileSysBL { set; get; }
    ApiService _apiService { set; get; }
    IAppConfig _appConfig { set; get; }
    IAppUser _appUser { set; get; }

    public string FolderPathImages = Path.Combine(FileSystem.AppDataDirectory, "Images");
    public ImageUploadService(IFileSysBL fileSysBL, ApiService  apiService, IAppConfig appConfig, IAppUser appUser)
    {
        _fileSysBL = fileSysBL;
        _apiService = apiService;
        _appConfig = appConfig;
        _appUser = appUser;
    }
    // Universal method to post all pending images to server
    public async Task<bool> PostPendingImagesToServer(string UID = null)
    {
        try
        {
            // Get all pending image files from database
            List<IFileSys> pendingImages = await GetPendingImagesFromDatabase(UID);

            if (pendingImages == null || !pendingImages.Any())
            {
                Console.WriteLine("No pending images to upload.");
                return true;
            }

            Console.WriteLine($"Found {pendingImages.Count} pending images to upload.");

            bool allUploadsSuccessful = true;
            int successCount = 0;
            int failureCount = 0;

            foreach (var imageFile in pendingImages)
            {
                bool isUploaded = await UploadSingleImageWithRetry(imageFile, maxRetries: 3);

                if (isUploaded)
                {
                    successCount++;
                    Console.WriteLine($"Successfully uploaded: {imageFile.FileName}");
                }
                else
                {
                    failureCount++;
                    allUploadsSuccessful = false;
                    Console.WriteLine($"Failed to upload image: {imageFile.FileName}");
                }
            }

            Console.WriteLine($"Upload summary: {successCount} successful, {failureCount} failed out of {pendingImages.Count} total images.");
            return allUploadsSuccessful;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in PostPendingImagesToServer: {ex.Message}");
            return false;
        }
    }

    // Get pending images from database (filter only images, exclude videos)
    private async Task<List<IFileSys>> GetPendingImagesFromDatabase(string UID)
    {
        try
        {
            var allPendingFiles = await _fileSysBL.GetPendingFileSyToUpload(UID);

            // Filter only image files (exclude videos and other file types)
            var pendingImages = allPendingFiles?.Where(file =>
                !string.IsNullOrEmpty(file.FileType) &&
                IsImageFile(file.FileType) &&
                !string.IsNullOrEmpty(file.FileName)
            ).ToList();

            return pendingImages ?? new List<IFileSys>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error getting pending images from database: {ex.Message}");
            return new List<IFileSys>();
        }
    }

    // Check if file is an image
    private bool IsImageFile(string fileType)
    {
        var imageTypes = new[] { "Image", "jpg", "jpeg", "png", "gif", "bmp", "webp", "tiff" };
        return imageTypes.Any(type => fileType.Equals(type, StringComparison.OrdinalIgnoreCase));
    }

    // Upload single image with retry logic
    private async Task<bool> UploadSingleImageWithRetry(IFileSys imageFile, int maxRetries)
    {
        int attempt = 0;
        Exception lastException = null;

        while (attempt < maxRetries)
        {
            try
            {
                attempt++;
                Console.WriteLine($"Uploading {imageFile.FileName} - Attempt {attempt}/{maxRetries}");

                bool isSuccess = await UploadImageToServer(imageFile);
                if (isSuccess)
                {
                    Console.WriteLine($"Upload successful for {imageFile.FileName}");
                    return true;
                }

                if (attempt < maxRetries)
                {
                    await Task.Delay(1000 * attempt); // Progressive delay: 1s, 2s, 3s...
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine($"Attempt {attempt} failed for {imageFile.FileName}: {ex.Message}");

                if (attempt < maxRetries)
                {
                    await Task.Delay(1000 * attempt);
                }
            }
        }

        Console.WriteLine($"All {maxRetries} attempts failed for {imageFile.FileName}. Last error: {lastException?.Message}");
        return false;
    }

    // Core method to upload single image to server
    
    private async Task<bool> UploadImageToServer(IFileSys imageFile)
    {
        string imagePath = "";

        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(3);

            using var formData = new MultipartFormDataContent();

            // Build image file path
            imagePath = Path.Combine(FolderPathImages, imageFile.FileName);

            if (!File.Exists(imagePath))
            {
                Console.WriteLine($"Image file not found at: {imagePath}");
                return false;
            }

            // Get server folder path for the image
            string serverFolderPath = imageFile.RelativePath;

            // Prepare image content for upload
            byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

            formData.Add(imageContent, "file", imageFile.FileName);
            formData.Add(new StringContent(serverFolderPath), "folderPath");

            // Upload to server
            var response = await _apiService.UploadFileAsync($"{_appConfig.ApiBaseUrl}FileUpload/UploadFile", formData);

            if (response.IsSuccess)
            {
                return await ProcessUploadResponse(response, imageFile, imagePath);
            }
            else
            {
                Console.WriteLine($"Upload API failed for: {imageFile.FileName}. Error: {response.ErrorMessage}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Upload failed for {imageFile.FileName}: {ex.Message}");
            return false;
        }
    }

    // Process upload response and update database
    private async Task<bool> ProcessUploadResponse(dynamic response, IFileSys imageFile, string localImagePath)
    {
        try
        {
            var responseBody = response.Data;
            var uploadResponse = JsonSerializer.Deserialize<ImageUploadResponse>(responseBody);

            if (uploadResponse != null && uploadResponse.Status == ImageUploadStatus.SUCCESS)
            {
                //string serverPath = uploadResponse.SavedImgsPath?.FirstOrDefault();
                string serverPath = (uploadResponse.SavedImgsPath != null && uploadResponse.SavedImgsPath.Count > 0)
                        ? uploadResponse.SavedImgsPath[0]
                        : null;


                if (string.IsNullOrEmpty(serverPath))
                {
                    Console.WriteLine($"Server path is empty for: {imageFile.FileName}");
                    return false;
                }

                // Update database with server path
                bool dbUpdated = await UpdateImageInDatabase(imageFile, serverPath);

                if (dbUpdated)
                {
                    // Delete local image file after successful upload
                    DeleteLocalImageFile(localImagePath);
                    return true;
                }
            }

            Console.WriteLine($"Upload response indicates failure for: {imageFile.FileName}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to process upload response for {imageFile.FileName}: {ex.Message}");
            return false;
        }
    }

    // Update image record in database
    private async Task<bool> UpdateImageInDatabase(IFileSys imageFile, string serverPath)
    {
        try
        {
            imageFile.TempPath = serverPath;
            //imageFile.RelativePath = serverPath;
            imageFile.SS = 1; // Mark as synced
            imageFile.ModifiedBy = _appUser?.Emp?.UID ?? "";
            imageFile.ModifiedTime = DateTime.Now;

            int updateResult = await _fileSysBL.UpdateFileSys(imageFile);

            if (updateResult > 0)
            {
                Console.WriteLine($"Database updated for: {imageFile.FileName}");
                return true;
            }
            else
            {
                Console.WriteLine($"Failed to update database for: {imageFile.FileName}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database update error for {imageFile.FileName}: {ex.Message}");
            return false;
        }
    }

    // Get server folder path for the image
    private string GetServerFolderPath(IFileSys imageFile)
    {
        try
        {
            return FileSysTemplateControles.GetSurveyFolderPath(
                _appUser.SelectedJobPosition.UID,
                imageFile.LinkedItemType,
                imageFile.LinkedItemUID
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting server folder path: {ex.Message}");
            return ""; // Return empty string as fallback
        }
    }

    // Delete local image file after successful upload
    private void DeleteLocalImageFile(string imagePath)
    {
        try
        {
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
                Console.WriteLine($"Local image deleted: {imagePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete local image {imagePath}: {ex.Message}");
            // Don't throw - file deletion failure shouldn't fail the upload
        }
    }

}
