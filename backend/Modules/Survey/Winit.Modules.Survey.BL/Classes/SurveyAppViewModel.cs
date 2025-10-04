using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Survey.DL.Interfaces;
using Winit.Modules.Survey.Model.Classes;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using WINITSharedObjects.Models;

namespace Winit.Modules.Survey.BL.Classes
{
    public class SurveyAppViewModel : SurveyBaseViewModel
    {
        Winit.Modules.Survey.BL.Interfaces.ISurveyResponseBL _surveyResponseBL { set; get; }
        Winit.Modules.FileSys.BL.Interfaces.IFileSysBL _fileSysBL { set; get; }
        Winit.Modules.Store.BL.Interfaces.IStoreBL _storeBL { set; get; }
        public SurveyAppViewModel(IServiceProvider serviceProvider, IFilterHelper filter,
            ISortHelper sorter, IListHelper listHelper, IAppUser appUser, IAppSetting appSetting,
            IDataManager dataManager, IAppConfig appConfig, Winit.Modules.Survey.BL.Interfaces.ISurveyResponseBL surveyResponseBL,
            Winit.Modules.FileSys.BL.Interfaces.IFileSysBL fileSysBL, Winit.Modules.Store.BL.Interfaces.IStoreBL storeBL ,
            ApiService apiService)
            : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting, dataManager, appConfig, apiService)
        {
            _surveyResponseBL = surveyResponseBL;
            _fileSysBL = fileSysBL;
            _storeBL = storeBL;
        }


        public override async Task GetSurveySection(string UID)
        {
            SurveySection = new SurveySection();
            SurveySection = await _surveyResponseBL.GetSurveySection(UID);
        }

        private async Task ParseSurveySection()
        {
            if (SurveySection == null) return;

            //SectionTitle = SurveySection.SurveyData;
            Questions = DeserializeQuestions(SurveySection.SurveyData);
        }

        private List<IServeyQuestions> DeserializeQuestions(string surveyData)
        {
            return string.IsNullOrWhiteSpace(surveyData)
                ? new List<IServeyQuestions>()
                : System.Text.Json.JsonSerializer.Deserialize<List<IServeyQuestions>>(surveyData);
        }

        public override async Task<int> SubmitSurveyAsync(ISurveyResponseModel surveyResponse)
        {
            var surveyResponses = Responses.Values.ToList();
            int result = await _surveyResponseBL.CreateSurveyResponse(surveyResponse);

            if (result >= 1 && ImageFileSysList != null && ImageFileSysList.Any())
            {
                try
                {
                    await SaveCapturedImagesAsync(surveyResponse.UID);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error while saving captured images: {ex.Message}");
                    throw;
                }
            }
            return result;
        }
        private async Task SaveCapturedImagesAsync(string SurveyResponseUID)
        {
            try
            {
                // Save files in bulk before uploading
                // Save files in bulk before uploading
                ImageFileSysList?.ForEach(e => e.LinkedItemUID = $"{SurveyResponseUID}-{e.LinkedItemUID}");


                int saveResult = await _fileSysBL.CreateFileSysForBulk(ImageFileSysList);
                if (saveResult <= 0)
                    throw new Exception("Failed to save the captured images in the local database.");

                // Get pending files for upload
               // List<IFileSys> filesToUpload = null;
                List<IFileSys> filesToUpload  = await _fileSysBL.GetPendingFileSyToUpload();
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
                if (file.FileType.Equals("Video", StringComparison.OrdinalIgnoreCase))
                {
                    filePath = Path.Combine(FolderPathVidoes, file.FileName);
                }
                else
                {
                    filePath = Path.Combine(FolderPathImages, "Survey", file.FileName);
                }

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"File not found at path: {filePath}");
                    return false;
                }

                string serverFolderPath = FileSysTemplateControles.GetSurveyFolderPath(
                    _appUser.SelectedJobPosition.UID,
                    file.LinkedItemType,
                    file.LinkedItemUID
                );

                // Read file bytes and prepare content
                byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                formData.Add(fileContent, "file", file.FileName);
                formData.Add(new StringContent(serverFolderPath), "folderPath"); // ✅ Use server path format

                var response = await _apiService.UploadFileAsync($"{_appConfig.ApiBaseUrl}FileUpload/UploadFile", formData);

                if (response.IsSuccess)
                {
                    try
                    {
                        // ✅ Try to extract the server path from the response
                        var responseBody = response.Data;

                        var uploadResponse = JsonSerializer.Deserialize<ImageUploadResponse>(responseBody);

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

        public override async Task<ISurveyResponseModel> GetExistingResponse(string SectionId, string StoreHistoryUID, DateTime? submmitedDate)
        {
            return await _surveyResponseBL.GetSurveyResponseByUID(SectionId, StoreHistoryUID, submmitedDate);
        }
        public override async Task GetExistingSummary(string ActivityType, string LinkedItemUID)
        {
            SurveyResponseModels = await _surveyResponseBL.GetSurveyResponse(ActivityType, LinkedItemUID);
        }

        public override async Task GetCustomersByRoute(string RouteUID)
        {
            StoresListByRoute = await _storeBL.GetStoreByRouteUID(RouteUID);
        }

        public override async Task<int> UpdateSurveyResponse(ISurveyResponseModel surveyResponseModel)
        {
            return await _surveyResponseBL.UpdateSurveyResponse(surveyResponseModel);
        }

    }
}
