using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Constants;

namespace WINITMobile.Pages.CFD;

public partial class CloseOfDay
{
    [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }

    protected override void OnInitialized()
    {
        if (_viewmodel != null)
        {
            _viewmodel.SelectedBeatHistory = _appUser.SelectedBeatHistory;
            _viewmodel.UserJourney = _appUser.UserJourney;
            _viewmodel.SelectedRoute = _appUser.SelectedRoute;
            if (_viewmodel.UserJourney != null)
            {
                _viewmodel.UserJourneyUID = _viewmodel.UserJourney.UID;
                _viewmodel.StartReading = _viewmodel.UserJourney.StartOdometerReading;
                _viewmodel.EndReading = _viewmodel.UserJourney.EndOdometerReading;
                _viewmodel.EOTStatus = _viewmodel.UserJourney.EOTStatus;
                _viewmodel.JourneyStartTime = _viewmodel.UserJourney.JourneyStartTime;
                _viewmodel.JourneyEndTime = _viewmodel.UserJourney.JourneyEndTime;
                _viewmodel.HasAuditCompleted = _viewmodel.UserJourney.HasAuditCompleted;
            }

        }
        LoadResources(null, _languageService.SelectedCulture);

    }
    protected override async Task OnInitializedAsync()
    {
        if (_appUser?.UserJourney == null)
        {
            // Show the error alert
            await _alertService.ShowErrorAlert("Error", "There is no UserJourney . Please start the day to continue.");

            // Navigate to the dashboard page
            _navigationManager.NavigateTo("/DashBoard");

            return;
        }
        // call populate view model
        await _viewmodel.PopulateViewModel();
        StateHasChanged();
        //await SetTopBar();

    }

    async Task SetTopBar()
    {
        WINITMobile.Models.TopBar.MainButtons buttons = new WINITMobile.Models.TopBar.MainButtons()
        {
            TopLabel = @Localizer["close_of_day(cfd)"],

            UIButton1 = new Models.TopBar.Buttons
            {
                Action = async () => await CheckAndSubmit(),
                ButtonText = @Localizer["submit"],
                ButtonType = Models.TopBar.ButtonType.Text,
                IsVisible = true
            }
        };
        await Btnname.InvokeAsync(buttons);
    }

    protected async Task CheckAndSubmit()
    {
        int NotVisitedCustomers = _viewmodel.NonVisited;
        if (NotVisitedCustomers > 0)
        {
            // if(await _alertService.ShowConfirmationReturnType("CFD", string.Format("Still {0} Customers not visited. Are you sure you want to do CFD?", NotVisitedCustomers), "Yes", "No"))
            if (await _alertService.ShowConfirmationReturnType("Day End", string.Format($"{@Localizer["still"]} {NotVisitedCustomers} {@Localizer["customers_not_visited"]} {@Localizer["are_you_sure_you_want_to_do_cfd?"]}"), @Localizer["yes"], @Localizer["no"]))
            {
                await PerformCfd();
            }

            else
            {
                // implement later

            }
        }
        else
        {
            // here what should i do
            await PerformCfd();
        }
    }

    //protected async Task PerformCfd()
    //{
    //    if (_viewmodel.UploadStatus == "Completed")
    //    {
    //        // Proceed with CFD confirmation
    //        if (await _alertService.ShowConfirmationReturnType(
    //            @Localizer["cfd"],
    //            $"{@Localizer["are_you_at_the_depot?"]} {@Localizer["are_you_sure_you_want_to_do_close_for_day?"]} {@Localizer["after_you_close_for_day_you_will_not_be_able_to_do_any_further_transactions"]}",
    //            @Localizer["yes"],
    //            @Localizer["no"]
    //        ))
    //        {
    //            if (await _viewmodel.UpdateBeatHistoryAndUserJourney())
    //            {
    //                _appUser.SelectedBeatHistory.CFDTime = DateTime.Now;
    //                _appUser.UserJourney = _viewmodel.UserJourney;

    //                await _alertService.ShowSuccessAlert(@Localizer["success"], @Localizer["cfd_submitted_successfully"]);
    //                NavigateTo("DashBoard");
    //            }
    //            else
    //            {
    //                await _alertService.ShowErrorAlert("Error", "Update error");
    //            }
    //        }
    //    }
    //    else
    //    {
    //        // Ask user to upload first
    //        bool uploadNow = await _alertService.ShowConfirmationReturnType(
    //            "Pending Upload",
    //            $"{@Localizer["there_are_some_unposted_data_that_need_to_be_posted_before_doing_cfd"]} {"Do you want to upload now"}",
    //            "upload Now",
    //            "No"
    //        );

    //        if (uploadNow)
    //        {

    //            if (await _viewmodel.UpdateBeatHistoryAndUserJourney())
    //            {
    //                _appUser.SelectedBeatHistory.CFDTime = DateTime.Now;
    //                _appUser.UserJourney = _viewmodel.UserJourney;
    //                await _alertService.ShowSuccessAlert(@Localizer["success"], @Localizer["cfd_submitted_successfully"]);
    //                NavigateTo("DashBoard");
    //            }
    //            else
    //            {
    //                await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["update_error"]);
    //            }
    //            await OnUploadDataClick();
    //            StateHasChanged();
    //        }
    //        else
    //        {
    //            // NavigateTo("Settings");
    //            return;
    //        }
    //    }
    //}
    protected async Task<bool> IsEotCompletedAsync()
    {
        return _viewmodel?.UserJourney?.EOTStatus == "Complete";
    }
    //protected async Task PerformCfd()
    //{
    //    if (await IsEotCompletedAsync())
    //    {
    //        bool confirm = await _alertService.ShowConfirmationReturnType("Alert", "You already submitted EOT. Do you want to upload again?");
    //        if (!confirm)
    //        {
    //            return;
    //        }
    //    }
    //    // Ask user to upload first
    //    bool uploadNow = await _alertService.ShowConfirmationReturnType(
    //            "Pending Upload",
    //            $"{@Localizer["there_are_some_unposted_data_that_need_to_be_posted_before_doing_cfd"]} {"Do you want to upload now"}",
    //            "upload Now",
    //            "No"
    //        );

    //        if (uploadNow)
    //        {

    //            if (await _viewmodel.UpdateBeatHistoryAndUserJourney())
    //            {
    //                _appUser.SelectedBeatHistory.CFDTime = DateTime.Now;
    //                _appUser.UserJourney = _viewmodel.UserJourney;
    //                await _alertService.ShowSuccessAlert(@Localizer["success"], @Localizer["cfd_submitted_successfully"]);
    //                NavigateTo("DashBoard");
    //            }
    //            else
    //            {
    //                await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["update_error"]);
    //            }
    //            await OnUploadDataClick();
    //            StateHasChanged();
    //        }
    //        else
    //        {
    //            // NavigateTo("Settings");
    //            return;
    //        }

    //}
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
    protected async Task PerformCfd()
    {
        if (await IsEotCompletedAsync())
        {
            bool confirm = await _alertService.ShowConfirmationReturnType("Alert", "You already submitted Day End.", "Ok", "");
            return;
        }

        if (_viewmodel.IsSyncPushPending)
        {
            bool uploadNow = await _alertService.ShowConfirmationReturnType(
                "Pending Upload",
                $"{"There are some unposted data that need to be posted before doing Day End. Do you want to upload now"}",
                "upload Now",
                "No");
            if (uploadNow)
            {
                await OnUploadDataClick();
                await _viewmodel.UpdateUploadStatus();
            }
        }

        if (_viewmodel.IsSyncPushPending)
        {
            return;
        }

        // Refresh the UI to show updated status
        StateHasChanged();

        // Now update the local status (BeatHistory and UserJourney)
        if (await _viewmodel.UpdateBeatHistoryAndUserJourney())
        {
            if(_appUser.SelectedBeatHistory != null)
            {
                _appUser.SelectedBeatHistory.CFDTime = DateTime.Now;
            }
            
            _appUser.UserJourney = _viewmodel.UserJourney;

            // Update the viewmodel status properties to reflect completion
            _viewmodel.EOTStatus = "Complete";
            _viewmodel.UploadStatus = "Completed";
            // First, perform the upload
            await OnUploadDataClick();
            // Final UI refresh
            StateHasChanged();

            await _alertService.ShowSuccessAlert(@Localizer["success"], "Day End submitted successfully.");
            NavigateTo("DashBoard");
        }
        else
        {
            await _alertService.ShowErrorAlert(@Localizer["error"], "Error while doing Day End. Please contact support team.");
        }

    }
    protected async Task DeleteUerJourney()
    {
        if (_viewmodel != null)
        {
            await _viewmodel.DeleteUserJourneyColumns();

        }
    }

    protected async Task UpdateLocalStatus()
    {
        if (await _viewmodel.UpdateBeatHistoryAndUserJourney())
        {
            if (_appUser.SelectedBeatHistory != null)
            {
                _appUser.SelectedBeatHistory.CFDTime = DateTime.Now;
            }
            _appUser.UserJourney = _viewmodel.UserJourney;
            await _alertService.ShowSuccessAlert(@Localizer["success"], "Day End submitted successfully.");
            NavigateTo("DashBoard");
        }
        else
        {
            await _alertService.ShowErrorAlert("Error", "Update error");
        }
        await Task.CompletedTask;
    }
    private async Task OnUploadDataClick_old()
    {
        _loadingService.ShowLoading("Data uploading");
        try
        {
            await Task.Run(async () =>
            {

                await _mobileDataSyncBL.SyncDataFromSQLiteToServer(_appUser.SelectedJobPosition.OrgUID, DbTableGroup.Master, _appUser.Emp.UID, _appUser.SelectedJobPosition.UID);
                await _mobileDataSyncBL.SyncDataFromSQLiteToServer(_appUser.SelectedJobPosition.OrgUID, DbTableGroup.SurveyResponse, _appUser.Emp.UID, _appUser.SelectedJobPosition.UID);
                await _mobileDataSyncBL.SyncDataFromSQLiteToServer(_appUser.SelectedJobPosition.OrgUID, DbTableGroup.FileSys, _appUser.Emp.UID, _appUser.SelectedJobPosition.UID);
                //await _mobileDataSyncBL.SyncDataFromSQLiteToServer(_appUser.SelectedJobPosition.OrgUID, DbTableGroup.Address, _appUser.Emp.UID, _appUser.SelectedJobPosition.UID);


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
    protected async Task PrintSalesSummary()
    {
        if (await CheckPrinterAvailableOrNot())
        {
        }
    }
    protected async Task PrintCollectionSummary()
    {
        if (await CheckPrinterAvailableOrNot())
        {
        }
    }
    private async Task<bool> CheckPrinterAvailableOrNot()
    {
        string printerTypeString = _storageHelper.GetStringFromPreferences("PrinterTypeOrBrand");
        string printerSizeString = _storageHelper.GetStringFromPreferences("PrinterPaperSize");
        string printerMacAddressString = _storageHelper.GetStringFromPreferences("MacAddress");
        StringBuilder validationMessage = new StringBuilder();

        if (string.IsNullOrEmpty(printerTypeString))
        {
            validationMessage.Append(@Localizer["printer_type_or_brand_is_empty_or_null"]);
        }
        if (string.IsNullOrEmpty(printerSizeString))
        {
            validationMessage.Append(@Localizer["printer_paper_size_is_empty_or_null"]);
        }
        if (string.IsNullOrEmpty(printerMacAddressString))
        {
            validationMessage.Append(@Localizer["printer_mac_address_is_empty_or_null"]);
        }

        if (validationMessage.Length > 0)
        {
            await _alertService.ShowErrorAlert(@Localizer["no_printer"], @Localizer["no_printer_selected_at_printer_service_for_printing"] + validationMessage.ToString());
            return false;
        }
        return true;
    }




}
