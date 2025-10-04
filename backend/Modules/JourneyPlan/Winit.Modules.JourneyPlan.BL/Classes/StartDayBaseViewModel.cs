using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Modules.FileSys.Model.Interfaces;
using Nest;
using Winit.Shared.CommonUtilities.Common;
using WINITSharedObjects.Models;

namespace Winit.Modules.JourneyPlan.BL.Classes
{

    public class StartDayBaseViewModel : IStartDayViewModel
    {
        public IServiceProvider _serviceProvider;
        public IFilterHelper _filter;
        public ISortHelper _sorter;
        public IListHelper _listHelper;
        public IAppUser _appUser;
        public IAppConfig _appConfigs;
        public ApiService _apiService;
        private Winit.Modules.Route.BL.Interfaces.IRouteBL _routeBL;
        public Winit.Modules.Store.BL.Interfaces.IStoreBL _storeBL { get; set; }
        Winit.Modules.FileSys.BL.Interfaces.IFileSysBL _fileSysBL { set; get; }
        public IBeatHistoryBL _BeatHistoryBL { get; set; }
        public List<IFileSys> ImageFileSysList { get; set; } = new List<IFileSys>();
        public StartDayBaseViewModel(IServiceProvider serviceProvider, IFilterHelper filter, ISortHelper sorter, IListHelper listHelper,
            IAppUser appUser, IAppConfig appConfigs, Winit.Modules.FileSys.BL.Interfaces.IFileSysBL fileSysBL, ApiService apiService, IBeatHistoryBL BeatHistoryBL, IStoreBL storeBL, IUserJourney UserJourney)
        {
            _BeatHistoryBL = BeatHistoryBL;
            _storeBL = storeBL;
            this._serviceProvider = serviceProvider;
            this._filter = filter;
            this._sorter = sorter;
            this._listHelper = listHelper;
            this._appUser = appUser;
            this._appConfigs = appConfigs;
            this._apiService = apiService;
            this.UserJourney = UserJourney;
            this._fileSysBL = fileSysBL;
        }
        public IUserJourney UserJourney { get; set; }
        public IRoute SelectedRoute { get; set; }
        public string FolderName { get; set; }
        public async Task<int> UploadStartDayUserJourney()
        {
            int result = await _BeatHistoryBL.InsertUserJourney(UserJourney);
            
            if (result >= 1 && ImageFileSysList != null && ImageFileSysList.Any())
            {
                try
                {
                    // Update linked Item UID as User Journey UID
                    ImageFileSysList.ForEach(e => e.LinkedItemUID = UserJourney.UID); 
                    await SaveCapturedImagesAsync();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error while saving captured images: {ex.Message}");
                    throw;
                }
            }
            return result;
        }
        private async Task SaveCapturedImagesAsync()
        {
            try
            {
                int saveResult = await _fileSysBL.CreateFileSysForBulk(ImageFileSysList);
                if (saveResult <= 0)
                    throw new Exception("Failed to save the captured images in the local database.");

                // Get pending files for upload
                // List<IFileSys> filesToUpload = null;
                List<IFileSys> filesToUpload = await _fileSysBL.GetPendingFileSyToUpload();
                if (filesToUpload == null || !filesToUpload.Any()) return;

                foreach (var file in filesToUpload)
                {
                    bool isUploaded = await UploadFileWithRetry(file, maxRetries: 2);
                    if (!isUploaded)
                    {
                        Console.WriteLine($"Failed to upload file: {file.FileName} after multiple attempts.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in SaveCapturedImagesAsync: {ex.Message}");
            }
        }
        private async Task<bool> UploadFileWithRetry(IFileSys file, int maxRetries)
        {
            int attempt = 0;
            while (attempt < maxRetries)
            {
                try
                {
                    attempt++;
                    bool isSuccess = await UploadFileAsync(file);
                    if (isSuccess) return true;

                    Console.WriteLine($"Retrying upload ({attempt}/{maxRetries}) for file: {file.FileName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Attempt {attempt} failed for file {file.FileName}: {ex.Message}");
                }
            }

            return false;
        }
        private async Task<bool> UploadFileAsync(IFileSys file)
        {
            try
            {
                using var httpClient = new HttpClient();
                using var formData = new MultipartFormDataContent();

                string filePath;

                filePath = Path.Combine(FolderName, file.FileName);

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"File not found at path: {filePath}");
                    return false;
                }

                string serverFolderPath = FileSysTemplateControles.GetAttendenceFolderPath(
                    _appUser.SelectedJobPosition.UID,
                    _appUser.Emp.UID
                );

                // Read file bytes and prepare content
                byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                formData.Add(fileContent, "file", file.FileName);
                formData.Add(new StringContent(serverFolderPath), "folderPath"); // ✅ Use server path format

                var response = await _apiService.UploadFileAsync($"{_appConfigs.ApiBaseUrl}FileUpload/UploadFile", formData);

                if (response.IsSuccess)
                {
                    try
                    {
                        // ✅ Try to extract the server path from the response
                        var responseBody = response.Data;

                        var uploadResponse = System.Text.Json.JsonSerializer.Deserialize<ImageUploadResponse>(responseBody);

                        if (uploadResponse != null && uploadResponse.Status == ImageUploadStatus.SUCCESS)
                        {
                            string serverPath = uploadResponse.SavedImgsPath.FirstOrDefault();

                            // ✅ Update database with the server path
                            file.TempPath = serverPath;
                            file.RelativePath = serverPath;
                            file.SS = 1;
                            file.ModifiedBy = _appUser?.Emp?.UID ?? "";
                            file.ModifiedTime = DateTime.Now;

                            //file.FileSize = uploadResponse.
                            if (await _fileSysBL.UpdateFileSys(file) > 0)
                            {
                                // ✅ Delete the local file after successful upload
                                if (File.Exists(filePath))
                                {
                                    File.Delete(filePath);
                                }

                                Console.WriteLine($"Successfully uploaded: {file.FileName}");
                                return true;

                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to parse upload response: " + ex.Message);
                    }
                }

                Console.WriteLine($"Failed to upload: {file.FileName}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Upload failed: {ex.Message}");
                return false;
            }
        }

    }
}
