using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Constants;

namespace WINITMobile.Pages
{
    partial class Settings
    {
        private bool IsChangePassWordPopUpOpen { get; set; }
        private string ConfirmPassword { get; set; }
        private string ErrorMsg { get; set; }
        private Winit.Modules.Auth.Model.Interfaces.IChangePassword ChangePassword { get; set; }

        private async Task OnChangePasswordClick()
        {
            if (string.IsNullOrEmpty(ChangePassword.OldPassword))
            {
                ErrorMsg = @Localizer["current_password_is_required"];
                return;
            }
            if (string.IsNullOrEmpty(ChangePassword.NewPassword))
            {
                ErrorMsg = @Localizer["please_enter_new_password"];
                return;
            }
            if (ChangePassword.NewPassword != ConfirmPassword)
            {
                ErrorMsg = @Localizer["password_miss_match"];
                return;
            }
            try
            {
                ShowLoader();
                ChangePassword.ChallengeCode = _sHACommonFunctions.GenerateChallengeCode();
                ChangePassword.OldPassword = _sHACommonFunctions.EncryptPasswordWithChallenge(ChangePassword.OldPassword?.Trim(), ChangePassword.ChallengeCode);
                ChangePassword.UserId = _appUser?.Emp?.LoginId;
                ChangePassword.NewPassword = ChangePassword.NewPassword;
                string response = await _loginViewModel.UpdateExistingPasswordWithNewPassword(ChangePassword);
                IsChangePassWordPopUpOpen = false;
                HideLoader();
                ErrorMsg = null;
                await _alertService.ShowSuccessAlert(@Localizer["success"], response);
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
            }
        }
        protected override async void OnInitialized()
        {
            LoadResources(null, _languageService.SelectedCulture);
            _backbuttonhandler.ClearCurrentPage();
        }
        private async Task OnSyncDataClick()
        {
            try
            {
                // Caller controls loader lifecycle for consistency
                ShowLoader("Syncing data...");
                
                // Use interactive sync for Settings page - shows success confirmation
                await ProcessDatabaseSyncInteractive();
                // Base method will update loader text with progress
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert(@Localizer["error"], ex.Message);
            }
            finally
            {
                // Caller responsibility to hide loader for consistency
                HideLoader();
            }
        }
        
        /// <summary>
        /// Handles the upload data click event with full UI feedback.
        /// Uses the reusable HandleCurrentUserDataUpload method.
        /// Caller manages loader lifecycle as expected by base method.
        /// </summary>
        private async Task OnUploadDataClick()
        {
            try
            {
                // Caller controls loader lifecycle - base method expects this
                ShowLoader("Uploading data...");
                
                await HandleCurrentUserDataUpload(
                    baseMessage: "Uploading data",
                    showAlerts: true
                );
                // Base method will update loader text with progress
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert(@Localizer["error"], "Upload failed. Please try again.");
                Console.WriteLine($"OnUploadDataClick error: {ex.Message}");
            }
            finally
            {
                // Caller responsibility to hide loader (as expected by base method)
                HideLoader();
            }
        }

        private async Task UploadDatabase()
        {
            _loadingService.ShowLoading("Uploading logs");
            try
            {
                bool ClearDataAfterUpload = false;
                bool result = await _databaseUploadService.UploadDatabaseToServer(ClearDataAfterUpload, "Settings");

                if (result)
                {

                    await _alertService.ShowErrorAlert("Upload Success", "Database logs uploaded successfully!");
                    //if (ClearDataAfterUpload)
                    //{
                    //    StatusMessage += " Data has been cleared.";
                    //}
                }
                else
                {

                    await _alertService.ShowErrorAlert("Upload Failed", "Failed to upload database to server.");
                }
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

        private async Task UploadImagesToServer()
        {
            _loadingService.ShowLoading("Uploading Files");
            try
            {
                bool ClearDataAfterUpload = false;
                bool result = await _imageUploadService.PostPendingImagesToServer();

                if (result)
                {

                    await _alertService.ShowErrorAlert("Upload Success", "Files  uploaded successfully!");
                    //if (ClearDataAfterUpload)
                    //{
                    //    StatusMessage += " Data has been cleared.";
                    //}
                }
                else
                {

                    await _alertService.ShowErrorAlert("Upload Failed", "Failed to upload Files to server.");
                }
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
