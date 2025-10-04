using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Common;
using Microsoft.SqlServer.Server;
using WINITSharedObjects.Models;
using Winit.Modules.FileSys.Model.Classes;
using Nest;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Common.Model.Constants;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using Winit.UIComponents.Common;
using Winit.Shared.CommonUtilities.Common;
using Winit.UIComponents.Common.Services;

namespace Winit.FileUploader
{
    public class FileUploaderBaseViewModel : IFileUploaderBaseViewModel
    {
        public FileUploaderBaseViewModel(Winit.Shared.Models.Common.IAppConfig appConfig, ApiService apiService
            , IAppUser appUser, IAlertService alertService, IConfiguration configuration, ILoadingService loadingService)
        {
            _appConfig = appConfig;
            _apiService = apiService;
            _appUser = appUser;
            _alertService = alertService;
            _configuration = configuration;
            _loadingService = loadingService;
            UploadVedioMaxSize = (1024 * 1024) * CommonFunctions.GetIntValue(configuration["FileUploader:UploadVedioMaxSize"]!);
            UploadImageMaxSize = (1024 * 1024) * CommonFunctions.GetIntValue(configuration["FileUploader:UploadImageMaxSize"]!);
        }
        long UploadVedioMaxSize;
        long UploadImageMaxSize;
        ILoadingService _loadingService;
        IConfiguration _configuration { get; set; }
        IAlertService _alertService { get; set; }
        string _LinkedItemType { get; set; }
        string _LinkedItemUID { get; set; }
        string _FileSysType { get; set; }
        string _FileType { get; set; }
        public ApiService _apiService { get; set; }
        Winit.Shared.Models.Common.IAppConfig _appConfig;
        IAppUser _appUser { get; set; }
        public IBrowserFile SelectedFile { get; set; }
        public List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> AllImages { get; set; }

        public string imageUrl;
        public List<string> ImageList = [];
        public string FileType { get; set; }
        public List<string> SavedImageList { get; set; } = new();

        public string errorMessage;

        public bool isError { get; set; } = true;
        public string FilePath { get; set; }
        private string[] FileExtensions { get; set; }
        public string TempPath { get; set; }
        List<IBrowserFile> Files = [];
        long _FileSize;
        public void PopulateViewModel(string filePath, string FileExtension, long FileSize, string ErrorMessage)
        {
            FilePath = filePath;
            TempPath = filePath.Replace("Data", "Data/TempData");
            this.FileExtensions = FileExtension.Split(',');
            //this._LinkedItemType = LinkedItemType;
            //this._LinkedItemUID = LinkedItemUID;
            //this._FileType = FileType;
            //this._FileSysType = FileSysType;
            _FileSize = FileSize;
            if (AllImages == null)
            {
                AllImages = new();
            }
        }
        public void PopulateWhenParameterChanged(string FileExtension)
        {
            this.FileExtensions = FileExtension.Split(',');
            if (AllImages != null && AllImages.Any())
            {
                foreach (Winit.Modules.FileSys.Model.Classes.FileSys fileSys in AllImages)
                {
                    fileSys.FileSysFileType = IsImageOrPDF(fileSys.FileName);
                    //  AllImages.Add(fileSys);
                }
                //AllImages = FileSysList;
            }
            else
            {
                AllImages = new();
            }
        }

        bool IsSizeAllowed(string fileExtension, long fileSize)
        {
            if (fileExtension.Equals(".mp4", StringComparison.OrdinalIgnoreCase) || FileTypeConstants.Vedio.Equals(FileType, StringComparison.OrdinalIgnoreCase))
            {
                return fileSize <= UploadVedioMaxSize;
            }
            else
            {
                return fileSize <= UploadImageMaxSize;
            }
        }
        public async Task HandleFileSelected(InputFileChangeEventArgs e, string fileExtension)
        {
            try
            {
                _loadingService.ShowLoading();
                Files.Clear();
                ImageList.Clear();

                string fileSizeError = string.Empty;
                string fileTypeError = string.Empty;
                bool isUploadImage = true;
                FileExtensions = fileExtension.Split(",");
                foreach (var file in e.GetMultipleFiles())
                {

                    long fileLength = file.Size;
                    string fileExtn = Path.GetExtension(file.Name);
                    bool isAllowedFileType = FileExtensions.Any(p => p.Equals(fileExtn, StringComparison.OrdinalIgnoreCase));

                    if (isAllowedFileType)
                    {
                        if (IsSizeAllowed(fileExtension, fileLength))
                        {
                            Files.Add(file);
                        }
                        else
                        {
                            isUploadImage = false;
                            fileSizeError += $"{file.Name} ,";
                        }
                    }
                    else
                    {
                        isUploadImage = false;
                        fileTypeError += $"{file.Name} ,";
                    }
                }
                if (!isUploadImage)
                {
                    string message = string.Empty;
                    if (!string.IsNullOrEmpty(fileTypeError))
                    {
                        message = $"{errorMessage.Substring(0, errorMessage.Length - 2)} File(s) Type is not allowed.;";
                    }
                    if (!string.IsNullOrEmpty(fileSizeError))
                    {
                        if ("Video".Equals(FileType))
                        {
                            message += $" ; {fileSizeError.Substring(0, fileSizeError.Length - 2)} File(s) Size for Video Shoud be less than {CommonFunctions.GetIntValue(_configuration["FileUploader:UploadVedioMaxSize"]!)}MB ";
                        }
                        else
                        {
                            message += $" ; {fileSizeError.Substring(0, fileSizeError.Length - 2)} File(s) Size for Image Shoud be less than {CommonFunctions.GetIntValue(_configuration["FileUploader:UploadImageMaxSize"]!)}MB ";
                        }
                    }
                    await _alertService.ShowErrorAlert("Error", message);

                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _loadingService.HideLoading();
            }


        }

        public async Task<bool> UploadFile(string linkedItemType, string linkedItemUID,
            string fileType, string fileSysType, string relativePath, bool isDirectory)
        {
            if (Files.Count == 0)
            {
                return false;
            }
            string tempPath = relativePath;
            if (!string.IsNullOrEmpty(relativePath))
            {
                tempPath = GetTempPath(relativePath);
            }
            List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> files = [];
            if (Files != null)
            {
                try
                {
                    using (var formData = new MultipartFormDataContent())
                    {
                        foreach (var file in Files)
                        {
                            using var memoryStream = new MemoryStream();
                            await file.OpenReadStream(100 * 1024 * 1024).CopyToAsync(memoryStream);
                            var content = new ByteArrayContent(memoryStream.ToArray());
                            string filextn = Path.GetExtension(file.Name).ToLower();
                            string fileName = Path.GetFileNameWithoutExtension(file.Name) + "_" + DateTime.Now.Ticks + filextn;
                            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                            {
                                Name = "\"file\"",
                                FileName = $"\"{fileName}\""
                            };

                            formData.Add(content);

                            FileSys fileSys = new()
                            {
                                Id = 0,
                                UID = Guid.NewGuid().ToString(),
                                LinkedItemType = linkedItemType,
                                LinkedItemUID = linkedItemUID,
                                FileSysType = fileSysType,
                                IsDirectory = isDirectory,
                                FileName = fileName,
                                DisplayName = file.Name,
                                FileSize = file.Size,
                                RelativePath = relativePath,
                                TempPath = tempPath,
                                Latitude = 0.ToString(),
                                Longitude = 0.ToString(),
                                CreatedBy = _appUser.Emp.UID,
                                ModifiedBy = _appUser.Emp.UID,
                                CreatedTime = DateTime.Now,
                                ModifiedTime = DateTime.Now,
                                ServerAddTime = DateTime.Now,
                                ServerModifiedTime = DateTime.Now,
                            };
                            if (filextn == ".pdf")
                            {
                                fileSys.FileType = FileTypeConstants.Pdf;
                                fileSys.FileSysFileType = Winit.Modules.FileSys.Model.Classes.FileType.Pdf;
                            }
                            else if (filextn == ".jpg" || filextn == ".jpeg" || filextn == ".png")
                            {
                                fileSys.FileType = FileTypeConstants.Image;
                                fileSys.FileSysFileType = Winit.Modules.FileSys.Model.Classes.FileType.Image;
                            }
                            else if (filextn == ".doc" || filextn == ".docx")
                            {
                                fileSys.FileType = FileTypeConstants.Doc;
                                fileSys.FileSysFileType = Winit.Modules.FileSys.Model.Classes.FileType.Doc;
                            }
                            else if (filextn == ".mp4")
                            {
                                fileSys.FileType = FileTypeConstants.Vedio;
                                fileSys.FileSysFileType = Winit.Modules.FileSys.Model.Classes.FileType.Video;
                            }
                            files.Add(fileSys);
                        }

                        formData.Add(new StringContent(tempPath), "folderPath");
                        var response = await _apiService.UploadFileAsync($"{_appConfig.ApiBaseUrl}FileUpload/UploadFile", formData);
                        if (response.IsSuccess)
                        {
                            AllImages.AddRange(files);
                            Files.Clear();
                            return true;
                        }
                        else
                        {
                            await _alertService.ShowErrorAlert("Error", response!.ErrorMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exception
                    //return ex.Message;
                }



                //Console.WriteLine(response); // Handle the response as needed
            }
            else
            {
                Console.WriteLine("No file selected.");
            }
            return false;
        }

        //public async Task<string> UploadFileAsync(string LinkedItemType, string LinkedItemUID, string FileType, string FileSysType, string RelativePath)
        //{
        //    try
        //    {
        //        using var formData = new MultipartFormDataContent();

        //        foreach (var file in Files)
        //        {
        //            using var memoryStream = new MemoryStream();
        //            await file.OpenReadStream().CopyToAsync(memoryStream);
        //            var content = new ByteArrayContent(memoryStream.ToArray());
        //            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        //            {
        //                Name = "\"file\"",
        //                FileName = $"\"{file.Name}\""
        //            };

        //            formData.Add(content);

        //            AllImages.Add(new FileSys()
        //            {
        //                Id = 0,
        //                UID = Guid.NewGuid().ToString(),
        //                LinkedItemType = _LinkedItemType,
        //                LinkedItemUID = _LinkedItemUID,
        //                FileSysType = _FileSysType,
        //                FileType = _FileType,
        //                IsDirectory = false,
        //                FileName = file.Name,
        //                DisplayName = file.Name,
        //                FileSize = file.Size,
        //                RelativePath = FilePath,
        //                TempPath = this.TempPath,
        //                Latitude = 0.ToString(),
        //                Longitude = 0.ToString(),
        //                CreatedBy = _appUser.Emp.UID,
        //                ModifiedBy = _appUser.Emp.UID,
        //                CreatedTime = DateTime.Now,
        //                ModifiedTime = DateTime.Now,
        //                ServerAddTime = DateTime.Now,
        //                ServerModifiedTime = DateTime.Now,
        //            });
        //        }

        //        if (!string.IsNullOrEmpty(TempPath))
        //        {
        //            formData.Add(new StringContent(TempPath), "folderPath");
        //        }

        //        using var httpClient = new HttpClient();

        //        var response = await _apiService.UploadFileAsync($"{_appConfig.ApiBaseUrl}FileUpload/UploadFile", formData);


        //        return response.Data;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exception
        //        return ex.Message;
        //    }
        //}
        protected Winit.Modules.FileSys.Model.Classes.FileType IsImageOrPDF(string fileName, string filextn = null)
        {
            Winit.Modules.FileSys.Model.Classes.FileType fileType;
            if (!string.IsNullOrEmpty(fileName))
            {
                if (filextn == null)
                {
                    filextn = Path.GetExtension(fileName);
                }
                if (filextn == ".pdf")
                {
                    return Winit.Modules.FileSys.Model.Classes.FileType.Pdf;
                }
                else if (filextn == ".jpg" || filextn == ".jpeg" || filextn == ".png")
                {
                    return Winit.Modules.FileSys.Model.Classes.FileType.Image;
                }
            }
            return Winit.Modules.FileSys.Model.Classes.FileType.None;

        }
        //public async Task<string> UploadFileAsync1(string LinkedItemType, string LinkedItemUID, string FileType, string FileSysType, string RelativePath)
        //{
        //    try
        //    {
        //        using var formData = new MultipartFormDataContent();
        //        foreach (var file in Files)
        //        {
        //            using var memoryStream = new MemoryStream();
        //            await file.OpenReadStream().CopyToAsync(memoryStream);
        //            var content = new ByteArrayContent(memoryStream.ToArray());
        //            string filextn = Path.GetExtension(file.Name).ToLower();
        //            string fileName = Path.GetFileNameWithoutExtension(file.Name) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + filextn;
        //            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        //            {
        //                Name = "\"file\"",
        //                FileName = $"\"{fileName}\""
        //            };

        //            formData.Add(content);

        //            FileSys fileSys = new()
        //            {
        //                Id = 0,
        //                UID = Guid.NewGuid().ToString(),
        //                LinkedItemType = LinkedItemType,
        //                LinkedItemUID = LinkedItemUID,
        //                FileSysType = FileSysType,
        //                IsDirectory = false,
        //                FileName = fileName,
        //                DisplayName = file.Name,
        //                FileSize = file.Size,
        //                RelativePath = FilePath,
        //                TempPath = this.TempPath,
        //                Latitude = 0.ToString(),
        //                Longitude = 0.ToString(),
        //                CreatedBy = _appUser.Emp.UID,
        //                ModifiedBy = _appUser.Emp.UID,
        //                CreatedTime = DateTime.Now,
        //                ModifiedTime = DateTime.Now,
        //                ServerAddTime = DateTime.Now,
        //                ServerModifiedTime = DateTime.Now,
        //            };
        //            if (filextn == ".pdf")
        //            {
        //                fileSys.FileType = FileTypeConstants.Pdf;
        //                fileSys.FileSysFileType = Winit.Modules.FileSys.Model.Classes.FileType.Pdf;
        //            }
        //            else if (filextn == ".jpg" || filextn == ".jpeg" || filextn == ".png")
        //            {
        //                fileSys.FileType = FileTypeConstants.Image;
        //                fileSys.FileSysFileType = Winit.Modules.FileSys.Model.Classes.FileType.Image;
        //            }
        //            else if (filextn == ".doc")
        //            {
        //                fileSys.FileType = FileTypeConstants.Doc;
        //            }
        //            AllImages.Add(fileSys);
        //        }

        //        if (!string.IsNullOrEmpty(TempPath))
        //        {
        //            formData.Add(new StringContent(TempPath), "folderPath");
        //        }

        //        using var httpClient = new HttpClient();

        //        var response = await _apiService.UploadFileAsync($"{_appConfig.ApiBaseUrl}FileUpload/UploadFile", formData);


        //        return response.Data;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exception
        //        return ex.Message;
        //    }
        //}


        public async Task<bool> DeleteFile(Modules.FileSys.Model.Classes.FileSys fileSys)
        {
            bool isYes = await _alertService.ShowConfirmationReturnType("Alert", "Are you sure you want to delete!");
            if (isYes)
            {
                string imagePath = @$"{fileSys.RelativePath}/{fileSys.FileName}";
                if (fileSys.Id == 0)
                {
                    imagePath = @$"{fileSys.TempPath}/{fileSys.FileName}";
                }
                var content = new StringContent(imagePath, Encoding.UTF8, "application/json");
                var response = await _apiService.UploadFileAsync($"{_appConfig.ApiBaseUrl}FileUpload/DeleteFile?imagePath={imagePath}", content);
                if (response != null && response.IsSuccess)
                {
                    if (fileSys.Id != 0)
                    {
                        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfig.ApiBaseUrl}FileSys/DeleteFileSys?UID={fileSys.UID}", HttpMethod.Delete);
                        if (apiResponse != null)
                        {
                            if (apiResponse.IsSuccess)
                            {
                                AllImages.Remove(fileSys);
                                await _alertService.ShowSuccessAlert("Success", "Deleted Successfully");
                                return apiResponse.IsSuccess;
                            }
                            else
                            {
                                await _alertService.ShowErrorAlert("Error", $"{apiResponse.ErrorMessage}");
                            }
                        }
                    }
                    else
                    {
                        AllImages.Remove(fileSys);
                        await _alertService.ShowSuccessAlert("Success", "Deleted Successfully");
                        return true;
                    }

                }
                else
                {

                    await _alertService.ShowErrorAlert("Error", $"{response.ErrorMessage}");
                }
            }
            return false;
        }

        public async Task<ApiResponse<string>> MoveFiles(string relativePath)
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfig.ApiBaseUrl}FileUpload/MoveFiles?SourcePath={GetTempPath(relativePath)}&DestinationPath={relativePath}", HttpMethod.Put);
            if (apiResponse != null && apiResponse.IsSuccess)
            {
                foreach (FileSys fileSys in AllImages)
                {
                    if (fileSys.Id == 0)
                    {
                        fileSys.Id = -1;
                    }
                }
            }
            return apiResponse;
        }
        protected string GetTempPath(string relativePath)
        {
            return relativePath.Replace("Data", "Data/TempData");
        }
    }
}
