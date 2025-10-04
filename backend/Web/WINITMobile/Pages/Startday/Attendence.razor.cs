using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Media;
using Microsoft.Maui.Controls;
using SkiaSharp;
using System.Security.Claims;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.Models.Constants;

namespace WINITMobile.Pages.Startday
{
    partial class Attendence
    {
        private string CapturedImage { get; set; }
        private string CurrentDate { get; set; }
        private string CurrentTime { get; set; }
        private string CurrentAddress { get; set; }
        private double NewLatitude { get; set; }
        private double NewLongitude { get; set; }
        private string Status { get; set; } = "Present";
        private string FolderPath { get; set; } = string.Empty;
        private string FileName { get; set; } = string.Empty;
        public List<WINITMobile.Data.FileSys> Files { get; set; } = new List<WINITMobile.Data.FileSys>();
        private bool IsImageCaptured { get; set; } = false;
        protected override async Task OnInitializedAsync()
        {
            await CheckGPSANDGetlatitude();
            CurrentDate = DateTime.Now.ToString("dd/MM/yyyy");
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");

            if (_viewmodel?.UserJourney != null)
            {
                _viewmodel.UserJourney.AttendanceLatitude = NewLatitude.ToString();
                _viewmodel.UserJourney.AttendanceLongitude = NewLongitude.ToString();
            }
            LoadResources(null, _languageService.SelectedCulture);
        }
        // Update your existing CaptureFile method to be more user-friendly
        public async Task CaptureFile()
        {
            ShowLoader();
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                    if (status != PermissionStatus.Granted)
                    {
                        await Application.Current.MainPage.DisplayAlert("Permission Denied", "Camera permission is required to capture photos.", "OK");
                        return;
                    }
                }

                // Show instruction dialog BEFORE opening camera
                //await Application.Current.MainPage.DisplayAlert(
                //    "📸 Take Your Selfie",
                //    "The camera will open now.\n\n🔄 Please switch to FRONT camera for your attendance selfie.\n\nLook for the camera switch button (usually 🔄 or 📷) in the camera app.",
                //    "Got it!");

                if (MediaPicker.Default.IsCaptureSupported)
                {
                    var options = new MediaPickerOptions
                    {
                        Title = "📸 Switch to Front Camera for Selfie"
                    };

                    FileResult photo = await MediaPicker.Default.CapturePhotoAsync(options);

                    if (photo != null)
                    {
                        CapturedImage = await ConvertToBase64(photo);
                        string fileName = $"{Guid.NewGuid()}.jpg";
                        //string filePath = Path.Combine(FileSystem.AppDataDirectory, "CapturedImages");

                        //FileName = $@"{FileName}_{photo.FileName}";
                        FileName = fileName;
                       // FolderPath = Path.Combine(filePath, fileName);
                        FolderPath = Path.Combine(FileSystem.AppDataDirectory, "Images", "Attendence");
                        

                        using Stream sourceStream = await photo.OpenReadAsync();
                        using MemoryStream memoryStream = new MemoryStream();
                        await sourceStream.CopyToAsync(memoryStream);

                        string base64String = Convert.ToBase64String(memoryStream.ToArray());
                        await Winit.Shared.CommonUtilities.Common.CommonFunctions.SaveBase64StringToFile(base64String,
                            FolderPath, fileName);

                        Files.Add(new WINITMobile.Data.FileSys
                        {
                            FileName = fileName,
                            FileData = base64String
                        });

                        string filePath;

                        filePath = Path.Combine(FolderPath, fileName);

                        if (!File.Exists(filePath))
                        {
                            Console.WriteLine($"File not found at path: {filePath}");
                        }

                        IsImageCaptured = true;
                        StateHasChanged();

                        // Verify it's a selfie by asking user
                        //var isSelfie = await Application.Current.MainPage.DisplayAlert(
                        //    "Verify Photo",
                        //    "Did you take this photo using the front camera (selfie)?\n\nFor attendance verification, we need a front-facing photo.",
                        //    "Yes, it's a selfie",
                        //    "No, let me retake");

                        //if (!isSelfie)
                        //{
                        //    await RetakePhoto();
                        //}
                    }
                    else
                    {
                        //await Application.Current.MainPage.DisplayAlert("Error", "Failed to capture photo: Photo result is null.", "OK");
                        IsImageCaptured = false;
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Camera capture is not supported on this device.", "OK");
                    IsImageCaptured = false;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to capture photo: {ex.Message}", "OK");
                IsImageCaptured = false;
                Console.WriteLine($"Exception: {ex}");
            }
            finally
            {
                HideLoader();
                StateHasChanged();
            }
        }
        //public async Task CaptureFile()
        //{
        //    try
        //    {
        //        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        //        if (status != PermissionStatus.Granted)
        //        {
        //            status = await Permissions.RequestAsync<Permissions.Camera>();
        //            if (status != PermissionStatus.Granted)
        //            {
        //                await Application.Current.MainPage.DisplayAlert("Permission Denied", "Camera permission is required to capture photos.", "OK");
        //                return;
        //            }
        //        }

        //        if (MediaPicker.Default.IsCaptureSupported)
        //        {
        //            FileResult photo = await MediaPicker.Default.CapturePhotoAsync();
        //            if (photo != null)
        //            {
        //                CapturedImage = await ConvertToBase64(photo);
        //                //string base64String = await ConvertToBase64(photo);
        //                string fileName = $"{Guid.NewGuid()}.jpg";
        //                string filePath = Path.Combine(FileSystem.AppDataDirectory, "CapturedImages");
        //                // Store the image in the cache directory
        //                FileName = $@"{FileName}_{photo.FileName}";
        //                FolderPath = Path.Combine(filePath, fileName);

        //                // Save the captured photo to the cache file
        //                using Stream sourceStream = await photo.OpenReadAsync();
        //                //using FileStream localFileStream = File.OpenWrite(localFilePath);
        //                using MemoryStream memoryStream = new MemoryStream();
        //                await sourceStream.CopyToAsync(memoryStream);

        //                // Add the file information to your data model
        //                string base64String = Convert.ToBase64String(memoryStream.ToArray());
        //                await Winit.Shared.CommonUtilities.Common.CommonFunctions.SaveBase64StringToFile(base64String,
        //                    FolderPath, fileName);
        //                Files.Add(
        //                    new WINITMobile.Data.FileSys
        //                    {
        //                        FileName = fileName,
        //                        FileData = base64String
        //                    });
        //                IsImageCaptured = true;
        //                StateHasChanged();
        //            }
        //            else
        //            {
        //                await Application.Current.MainPage.DisplayAlert("Error", "Failed to capture photo: Photo result is null.", "OK");
        //                IsImageCaptured = false;
        //            }
        //        }
        //        else
        //        {
        //            await Application.Current.MainPage.DisplayAlert("Error", "Camera capture is not supported on this device.", "OK");
        //            IsImageCaptured = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await Application.Current.MainPage.DisplayAlert("Error", $"Failed to capture photo: {ex.Message}", "OK");
        //        IsImageCaptured = false;
        //        // Log the exception for further analysis
        //        Console.WriteLine($"Exception: {ex}");
        //    }
        //}

        private async Task<string> ConvertToBase64(FileResult photo)
        {
            try
            {
                using (var stream = await photo.OpenReadAsync())
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();
                    return $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}";
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(@Localizer["error"], $"{@Localizer["failed_to_convert_photo"]}: {ex.Message}", "OK");
                return string.Empty;
            }
        }
        private async Task CheckGPSANDGetlatitude()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (status != PermissionStatus.Granted)
                    {
                        await Application.Current.MainPage.DisplayAlert(@Localizer["permission_denied"], @Localizer["location_permission_is_required_to_use_this_feature"], "OK");
                        return;
                    }
                }

                var location = await Geolocation.GetLocationAsync();
                if (location != null)
                {
                    NewLatitude = location.Latitude;
                    NewLongitude = location.Longitude;
                    CurrentAddress = await GetAddress(location);
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(@Localizer["gps"], @Localizer["gps_is_not_enabled._do_you_want_to_enable_it?"], "Yes", "No");
                }
            }
            catch (FeatureNotSupportedException ex)
            {
                await Application.Current.MainPage.DisplayAlert(@Localizer["gps"], @Localizer["gps_is_not_supported_on_this_device"], @Localizer["ok"]);
            }
            catch (PermissionException ex)
            {
                await Application.Current.MainPage.DisplayAlert(@Localizer["gps"], @Localizer["location_permission_is_not_granted"], @Localizer["ok"]);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(@Localizer["gps"], $"{@Localizer["error"]}: {ex.Message}", @Localizer["ok"]);
            }
        }

        private async Task<string> GetAddress(Location location)
        {
            try
            {
                var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var placemark = placemarks?.FirstOrDefault();
                if (placemark != null)
                {
                    return $"{placemark.SubLocality}, {placemark.Locality}, {placemark.SubAdminArea}, {placemark.AdminArea}, {placemark.CountryName}";
                }
                else
                {
                    return "Address not found";
                }
            }
            catch (FeatureNotSupportedException ex)
            {
                return "Geocoding is not supported on this device.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private async Task RetakePhoto()
        {
            CapturedImage = null;
            await CaptureFile();
        }
        private async Task AttendenceImage()
        {
            try
            {
                if (FileName == string.Empty && FolderPath == string.Empty)
                {
                    return;
                }
                IFileSys fileSys = ConvertFileSys("user_journey", _appUser.Emp?.UID, "Attendence", "Image",
                    FileName, _appUser.Emp?.Name, FolderPath);

                fileSys.SS = -1;
                _viewmodel.ImageFileSysList.Add(fileSys);
                _viewmodel.FolderName = FolderPath;
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {

            }

        }
        private async Task SaveImage()
        {
            if(Status == "Present")
            {
                if(!IsImageCaptured)
                {
                    await _alertService.ShowErrorAlert("Attendance Verification", "Please capture your attendance photo to continue.");
                    return;
                }
            }
            if (_viewmodel?.UserJourney != null)
            {
                _viewmodel.UserJourney.AttendanceStatus = Status;
            }
            await AttendenceImage();
            // later handle based on the role actually there changed roles for audit app --->Sales Officer
            //if (_appUser?.Role?.RoleNameEn == "Pre Sales User" || _appUser?.Role?.RoleNameEn == "Audit")
            if (false)
            {
                await PrepareUserJourneyAndSave();
            }
            else
            {
#if ANDROID
            if (!IsImageCaptured)
            {
                await _alertService.ShowErrorAlert("Attendance Verification", "Please capture your attendance photo to continue.");
                return;
            }
#endif
                if (_viewmodel?.UserJourney != null)
                {
                    _viewmodel.UserJourney.AttendanceStatus = Status;
                }
                _navigationManager.NavigateTo("OdometerReading");
            }
        }

       

        protected async Task PrepareUserJourneyAndSave()
        {
#if ANDROID
            if (!IsImageCaptured)
            {
                await _alertService.ShowErrorAlert("Attendance Verification", "Please capture your attendance photo to continue.");
                return;
            }
#endif
            if (_viewmodel != null)
            {
                if (_viewmodel.UserJourney != null)
                {
                    string GUID = Guid.NewGuid().ToString();
                    _viewmodel.UserJourney.UID = GUID;
                    _viewmodel.UserJourney.JobPositionUID = _appUser.SelectedJobPosition.UID;
                    _viewmodel.UserJourney.EmpUID = _appUser.Emp.UID;
                    _viewmodel.UserJourney.JourneyStartTime = DateTime.Now;
                    _viewmodel.UserJourney.JourneyEndTime = null;
                    // _viewmodel.UserJourney.StartOdometerReading = null;
                    //_viewmodel.UserJourney.EndOdometerReading = null;
                    _viewmodel.UserJourney.JourneyTime = null;
                    //  _viewmodel.UserJourney.VehicleUID = _appUser.Vehicle.UID;
                    _viewmodel.UserJourney.EOTStatus = "Pending";
                    _viewmodel.UserJourney.AttendanceStatus = Status;
                    _viewmodel.UserJourney.ReOpenedBy = null;
                    _viewmodel.UserJourney.HasAuditCompleted = false;
                    _viewmodel.UserJourney.BeatHistoryUID = _appUser?.SelectedBeatHistory?.UID ?? null;
                    _viewmodel.UserJourney.WHStockRequestUID = null;
                    _viewmodel.UserJourney.SS = 1;
                    _viewmodel.UserJourney.CreatedBy = _appUser.Emp.UID;
                    _viewmodel.UserJourney.ModifiedBy = _appUser.Emp.UID;
                }
            }
            await SetUserJourney();
        }
        protected async Task SetUserJourney()
        {
            // Check and update job_position_attendance
            await _dashBoardViewModel.GetUserAttendence(_appUser.SelectedJobPosition.UID, _appUser.Emp.UID);
            if (_dashBoardViewModel.JobPositionAttendance != null && _dashBoardViewModel.JobPositionAttendance.LastUpdateDate.Date != DateTime.Now.Date)
            {
                _dashBoardViewModel.JobPositionAttendance.DaysPresent++;
                _dashBoardViewModel.JobPositionAttendance.LastUpdateDate = DateTime.Now;
                // The AttendancePercentage will be automatically recalculated because it is a computed property
                bool Result = await _dashBoardViewModel.UpdateJobPositionAttendance(_dashBoardViewModel.JobPositionAttendance);
            }
            int res = await _viewmodel.UploadStartDayUserJourney();
            if (res >= 1)
            {
                _appUser.UserJourney = _viewmodel.UserJourney;
                await _appUser.SetStartDayAction();

                await _beatHistoryViewModel.Load();
                //NavigateTo("MessageOfTheDay");
                await OnUploadDataClick();
                if (Status == "Leave" || Status == "Weekoff")
                {
                    await _alertService.ShowErrorAlert("CFD Required", "Since you were on Leave/Weekoff, you still need to complete CFD.");
                    _navigationManager.NavigateTo("CloseOfTheDay");
                    return;
                }
                _navigationManager.NavigateTo("MessageOfTheDay");
            }
            else
            {
                await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["there_is_some_unposted_userdata"]);
            }
        }
        private async Task OnUploadDataClick()
        {
            _loadingService.ShowLoading("Data uploading");
            try
            {
                await Task.Run(async () =>
                {
                    await _mobileDataSyncBL.SyncDataFromSQLiteToServer(_appUser.SelectedJobPosition.OrgUID, DbTableGroup.Master, _appUser.Emp.UID, _appUser.SelectedJobPosition.UID);
                    //await _mobileDataSyncBL.SyncDataFromSQLiteToServer(_appUser.SelectedJobPosition.OrgUID, DbTableGroup.SurveyResponse, _appUser.Emp.UID, _appUser.SelectedJobPosition.UID);
                    //await _mobileDataSyncBL.SyncDataFromSQLiteToServer(_appUser.SelectedJobPosition.OrgUID, DbTableGroup.FileSys, _appUser.Emp.UID, _appUser.SelectedJobPosition.UID);

                    await InvokeAsync(async () =>
                    {
                        _loadingService.HideLoading();
                        await _alertService.ShowErrorAlert(@Localizer["success"], @Localizer["upload_completed_successfully"]);
                    });
                });
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert(@Localizer["error"], ex.Message);
            }
            finally
            {
                _loadingService.HideLoading();
            }
        }
    }
}