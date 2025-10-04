using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Shared.Models.Constants;

namespace WINITMobile.Pages.Startday
{
    partial class Odometer
    {
        public int OdometerReading { get; set; }
        public string VehicleNumber { get; set; }

        protected override void OnInitialized()
        {
            //VehicleNumber = _appUser.Vehicle.VehicleNo;
            VehicleNumber = "Ap323";
            LoadResources(null, _languageService.SelectedCulture);
            SaveReading();
        }

        protected async Task SaveReading()
        {
            if (await _alertService.ShowConfirmationReturnType(@Localizer["confirm"], @Localizer["do_you_want_to_start_the_journey?"], @Localizer["proceed"], @Localizer["cancel"]))
            {
                // SaveToDatabase(OdometerReading);
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
                        _viewmodel.UserJourney.StartOdometerReading = OdometerReading;
                        _viewmodel.UserJourney.EndOdometerReading = OdometerReading;
                        _viewmodel.UserJourney.JourneyTime = null;
                        _viewmodel.UserJourney.VehicleUID = _appUser?.Vehicle?.UID ?? "";
                        _viewmodel.UserJourney.EOTStatus = "Pending";
                      
                        _viewmodel.UserJourney.ReOpenedBy = null;
                        _viewmodel.UserJourney.HasAuditCompleted = false;
                        _viewmodel.UserJourney.BeatHistoryUID = null;
                        _viewmodel.UserJourney.WHStockRequestUID = null;
                        _viewmodel.UserJourney.SS = 1;
                        _viewmodel.UserJourney.CreatedBy = _appUser.Emp.UID;
                        _viewmodel.UserJourney.ModifiedBy = _appUser.Emp.UID;
                    }
                }
                await SetUserJourney();
            }
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
                //await OnUploadDataClick(); // Vishal commented on 28th Jun 2025 temporirly
                if (_viewmodel?.UserJourney?.AttendanceStatus == "Leave" || _viewmodel?.UserJourney?.AttendanceStatus == "Weekoff")
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