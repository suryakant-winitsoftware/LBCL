using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.Model.Constants;
using Winit.Modules.DashBoard.BL.Interfaces;
using Winit.Modules.JobPosition.Model.Classes;
using Winit.Modules.JobPosition.Model.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;

namespace Winit.Modules.DashBoard.BL;

public class DashboardViewModel : IDashBoardViewModel
{
    public Winit.Modules.Common.BL.Interfaces.IAppUser _appUser { get; set; }
    public DateTime JourneyStartDate { get; set; }
    public Winit.Modules.Common.Model.Interfaces.IMyRole SelectedRole { get; set; }
    public List<Winit.Modules.Common.Model.Interfaces.IMyRole> Roles { get; set; }
    public Winit.Modules.Route.Model.Interfaces.IRoute SelectedRoute { get; set; }
    public List<Winit.Modules.Route.Model.Interfaces.IRoute> RouteList { get; set; }
    public List<Winit.Modules.Org.Model.Interfaces.IOrg> MyOrgs { get; set; }
    public Winit.Modules.Org.Model.Interfaces.IOrg SelectedOrg { get; set; }
    public Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory SelectedBeatHistory { get; set; }
    public List<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> BeatHistoryList { get; set; }
    public Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance JobPositionAttendance { get; set; }
    public bool IsAllRouteJPRCompleted { get; set; }
    public bool DisableRouteDD { get; set; }
    public string StartDayBtnText { get; set; }
    private NavigationManager _navigationManager { get; set; }
    protected Winit.UIComponents.Common.IAlertService _alertService { get; set; }

    protected Winit.Modules.JobPosition.BL.Interfaces.IJobPositionBL _jobPositionBL { get; set; }

    //public bool IsVehicleInspectionNeeded { get; set; }
    //public string startDayStatus { get; set; }
    //public bool IsKPISelection { get; set; } 
    //public bool IsSendMsg { get; set; }
    //public bool IsDisplayMsg { get; set; }
    //public List<MenuItems> MenuItemsList = new List<MenuItems>();

    //public Dictionary<string, List<string>> MenuItemsListDictionary = new Dictionary<string, List<string>>();
    //public List<Shared.sFAModel.JPBeatHistory> BeatHistoryList { get; set; } = new List<Shared.sFAModel.JPBeatHistory>();
    //public bool IsPreviousBeatOpen { get; set; }
    //public bool IsBeatStartedFirstTime { get; set; } = false;
    //public int AlreadyCollectedLoadReqeustCountForRoute { get; set; }
    //public int AlreadyVisitedCustomerCountForRoute { get; set; }
    //public List<string> FOCLoadRequesstUIDsForRouteToCollect { get; set; }
    //public bool IsAllRouteJPRCompleted = false;
    //public string WHStockRequestUID { get; set; }
    protected readonly IStringLocalizer<LanguageKeys> Localizer;
    protected readonly ISKUBL _sKUBL;

    public DashboardViewModel(Winit.Modules.Common.BL.Interfaces.IAppUser appUser,
        NavigationManager navigationManager, Winit.UIComponents.Common.IAlertService alertService, 
        Winit.Modules.JobPosition.BL.Interfaces.IJobPositionBL jobPositionBL, 
        IStringLocalizer<LanguageKeys> localizer,
        ISKUBL sKUBL)
    {
        _appUser = appUser;
        _navigationManager = navigationManager;
        _alertService = alertService;
        _jobPositionBL = jobPositionBL;
        Localizer= localizer;
        _sKUBL = sKUBL;
    }

    private string Prevaildate()
    {
        Winit.Modules.Common.BL.Interfaces.IAppUser appUser = _appUser;
        if (appUser == null)
        {
            return @Localizer["unable_to_download_user_master_data_completely,_please_relogin_to_continue."];
            //HandleDeveloperError(_viewmodel, "Unable to download user master data completely, please relogin to continue.");
        }
        else if (appUser.SelectedRole == null)
        {
            return @Localizer["user_role_not_mapped"];
        }
        //else if (appUser.JpRoute == null)
        else if (/*WinIT.mSFA._appUser.DELIVERY_ACTIVITIES_PERM
            &&*/ (RouteList == null || RouteList.Count == 0))
        {
            return @Localizer["route_not_mapped"];
        }
        else if (/*_appUser.DELIVERY_ACTIVITIES_PERM &&*/ appUser.Vehicle == null)
        {
            return @Localizer["vehicle_not_mapped"];
        }
        else if (MyOrgs == null || MyOrgs.Count == 0)
        {
            return @Localizer["user_org_not_mapped"];
        }
        else if (appUser.OrgSeqCode == null)
        {
            return @Localizer["org_seq_code_not_mapped"];
        }
        else if (appUser.DefaultOrgCurrency == null)
        {
            return @Localizer["currency_not_mapped"];
        }
        else
        {
            return string.Empty;
        }
    }
    private async Task<string> ValidateBeatCount()
    {
        IsAllRouteJPRCompleted = false;
        if (SelectedRoute == null)
        {
            if (JourneyStartDate != default &&
                Winit.Shared.CommonUtilities.Common.CommonFunctions.GetTimeDifferenceInDay(JourneyStartDate, DateTime.Now.Date) == 0 &&
                (this.BeatHistoryList == null || this.BeatHistoryList.Count == 0))
            {
                return @Localizer["no_journey_plan_available_for_the_day"];
            }
            else
            {
                List<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> jPBeatHistoriesOpenToUse = this.BeatHistoryList
                    .Where(e => !e.HasAuditCompleted).ToList();
                if (jPBeatHistoriesOpenToUse == null || jPBeatHistoriesOpenToUse.Count == 0)
                {
                    IsAllRouteJPRCompleted = true;
                }
                else
                {
                    if (this.BeatHistoryList.Count > 1)
                    {

                    }
                    else
                    {

                    }
                }
            }
            return @Localizer["error"];
        }
        return string.Empty;
    }
    protected async Task OnStartDayConfirmed()
    {
        _navigationManager.NavigateTo("StartdayPreRequisties");
    }
    public async void StartDayInitiate()
    {
        if (StartDayBtnText.Equals(StartDayStatus.START_DAY))
        {
            await _alertService.ShowConfirmationAlert("Confirmation", "Have you marked your Razorpay attendance?", (param) => OnStartDayConfirmed(), "Yes", "No");
        }
        else if (StartDayBtnText.Equals(StartDayStatus.CONTINUE))
        {
            MoveToJP();
        }
        else if (StartDayBtnText.Equals(StartDayStatus.EOT_DONE))
        {
            await _alertService.ShowErrorAlert("", "Day End has been completed for the day. You can't perform any transaction Today.");
        }
        else if (StartDayBtnText.Equals(StartDayStatus.EOT))
        {
            _navigationManager.NavigateTo("/CustomerList");
        }
        else if (StartDayBtnText.Equals(StartDayStatus.BLANK))
        {
            await _alertService.ShowErrorAlert("", "You have already started future date and now you can't start back date.");
        }
        else
        {
            
            MoveToJP();
        }
    }
    public void MoveToJP()
    {
        if (_appUser.StartDayStatus.Equals(StartDayStatus.START_DAY))
        {
            _alertService.ShowErrorAlert("", @Localizer["please_start_your_day_first_to_continue_to_journey_plan."]);
        }
        else if (_appUser.StartDayStatus.Equals(StartDayStatus.EOT_DONE))
        {
            _alertService.ShowErrorAlert("", "Day End has been completed for the day. You can't perform any transaction Today.");
        }
        else if (_appUser.StartDayStatus.Equals(StartDayStatus.EOT))
        {
            //Intent intentCFD = new Intent(ApplicationContext, typeof(Activities.CFDActivity));
            //StartActivity(intentCFD);
            _navigationManager.NavigateTo("/CloseOfTheDay");
        }
        else if (_appUser.StartDayStatus.Equals(StartDayStatus.BLANK))
        {
            _alertService.ShowErrorAlert("", @Localizer["you_have_already_started_future_date_and_now_you_can't_start_back_date."]);
        }
        else if (_appUser.SelectedRoute == null)
        {
            _alertService.ShowErrorAlert("", @Localizer["please_select_route_to_continue."]);
        }
        else
        {
            //Intent intentJP = new Intent(ApplicationContext, typeof(Activities.JourneyPlan.JourneyPlanActivity));
            //intentJP.SetFlags(ActivityFlags.ClearTop);
            //intentJP.PutExtra("EXIT", true);
            //StartActivity(intentJP);
            _navigationManager.NavigateTo("/CustomerList");
        }
    }
    
    //private Task<string> StartDayInitiate()
    //{
    //    if (StartDayBtnText.Equals(StartDayStatus.START_DAY))
    //    {
    //        //ShowLoader("Starting Day");
    //        new Thread(new ThreadStart(() =>
    //        {
    //            try
    //            {
    //                //StartUnloadProcess(_viewmodel);
    //            }
    //            catch (System.Exception ex)
    //            {
    //                //HideLoader();
    //            }
    //        })).Start();
    //        //new Thread(new ThreadStart(() => NextDashboardFlow(new WinIT.mSFA.Dashboard.BLayer.InitStartDay(_viewmodel)))).Start();
    //    }
    //    else if (StartDayBtnText.Equals(StartDayStatus.CONTINUE))
    //    {
    //        //MoveToJourneyPlan();
    //        FetchPendingLoadRequestCount(TargetScreenType.JourneyPlan);
    //    }
    //    else if (StartDayBtnText.Equals(StartDayStatus.EOT_DONE))
    //    {
    //        return Task.FromResult("CFD has been completed for the day. You can't perform any transaction Today.");
    //    }
    //    else if (StartDayBtnText.Equals(StartDayStatus.EOT))
    //    {
    //        //Intent intent = new Intent(_activity, typeof(CFDActivity));
    //        //_activity.StartActivity(intent);
    //    }
    //    else if (StartDayBtnText.Equals(StartDayStatus.BLANK))
    //    {
    //        return Task.FromResult("You have already started future date and now you can't start back date.");
    //    }
    //    else
    //    {
    //        //MoveToJourneyPlan();
    //        FetchPendingLoadRequestCount(TargetScreenType.JourneyPlan);
    //    }
    //}

    //public string FetchPendingLoadRequestCount(string targetScreen, bool isBackPressed = false)
    //{
    //    if (_appUser.StartDayStatus.Equals(StartDayStatus.EOT))
    //    {
    //        return "CFD need to be done. You can't perform any transaction Today.";
    //    }
    //    else if (_appUser.StartDayStatus.Equals(StartDayStatus.EOT_DONE))
    //    {
    //        return "CFD has been completed for the day. You can't perform any transaction Today.";
    //    }
    //    else if (_appUser.SelectedRoute == null)
    //    {
    //        return "Please select route to continue.";
    //    }
    //    this.TargetScreen = targetScreen;
    //    if (_appUser.Emp.CanHandleStock == true)
    //    {
    //        if (_stockCollectionViewModel == null)
    //        {
    //            _stockCollectionViewModel = new mSFA.Stock.Model.StockCollectionViewModel();
    //        }
    //        //_stockCollectionViewModel.JourneyStartDate = GetJourneyStartDate();
    //        _stockCollectionViewModel.IsBackPressed = isBackPressed;
    //        NextCheckOutFlow(new WinIT.mSFA.Stock.BLayer.PopupatePendingMoveOrderCount(_stockCollectionViewModel));
    //    }
    //    else
    //    {
    //        MoveToTargetScreen();
    //    }
    //}

    public async Task GetUserAttendence(string jobPositionUID, string empUID)
    {
        try
        {
            JobPositionAttendance = await _jobPositionBL.GetJobPositionAttendanceByEmpUID(jobPositionUID, empUID);

        }
        catch (Exception ex)
        {

            await _alertService.ShowErrorAlert(@Localizer["error"], ex.Message);

        }
    }
    public async Task<bool> UpdateJobPositionAttendance(Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance jobPositionAttendance)
    {
        try
        {
            int res = await _jobPositionBL.UpdateJobPositionAttendance(jobPositionAttendance);
            return res > 0;
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert(@Localizer["error"], ex.Message);
            return false;
        }
    }

    public async Task PopulateCacheData()
    {
        var skumasters = await _sKUBL.PrepareSKUMaster(_appUser.OrgUIDs, null, null, null);
        await _sKUBL.CRUDWinitCache("skumaster", JsonConvert.SerializeObject(skumasters));
    }
    public async Task<IJobPositionAttendance> GetTotalAssignedAndVisitedStores()
    {
        return await _jobPositionBL.GetTotalAssignedAndVisitedStores(_appUser.SelectedJobPosition.UID);
    }
}
