using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;

namespace Winit.Modules.JourneyPlan.BL.Classes;

public abstract class BeatHistoryBaseViewModel : IBeatHistoryViewModel
{
    private readonly IAppUser _appUser;
    private readonly IBeatHistoryBL _beatHistoryBL;
    private readonly ILanguageService _languageService;
    private IStringLocalizer<LanguageKeys> _localizer;
    protected BeatHistoryBaseViewModel(IServiceProvider serviceProvider,
        IConfiguration config, IAppUser appUser, IBeatHistoryBL beatHistoryBL, IStringLocalizer<LanguageKeys> Localizer,
            ILanguageService languageService)
    {
        _appUser = appUser;
        _beatHistoryBL = beatHistoryBL;
        _localizer = Localizer;
        _languageService = languageService;
    }
    protected void LoadResources(object sender, string culture)
    {
        CultureInfo cultureInfo = new CultureInfo(culture);
        ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys).Assembly);
        _localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
    }
    public async Task Load()
    {
        LoadResources(null, _languageService.SelectedCulture);
        if (_appUser.UserJourney == null)
        {
            return;
        }
        //Reset();
        _appUser.BeatHistories = await _beatHistoryBL.GetActiveOrTodayBeatHistory();
        if (_appUser.BeatHistories != null && _appUser.BeatHistories.Any())
        {
            foreach (IJPBeatHistory jPBeatHistory in _appUser.BeatHistories)
            {
                if (_appUser.UserJourney?.JourneyStartTime != null) jPBeatHistory.JourneyStartDate = _appUser.UserJourney.JourneyStartTime.Value.Date;
                jPBeatHistory.UpdatedMyStatus();
            }
            var openlst = _appUser.BeatHistories
                .Where(e => Shared.CommonUtilities.Common.CommonFunctions.GetTimeDifferenceInDay(e.VisitDate, _appUser.JourneyStartDate) > 0 && Shared.CommonUtilities.Common.CommonFunctions.IsDateNull(e.EndTime))
                .ToList<IJPBeatHistory>();
            if (openlst != null && openlst.Count() > 0)
            {
                _appUser.IsPreviousBeatOpen = true;
            }
            else if (false/*_appUser.StartDayStatus.Equals(StartDayStatus.EOT_DONE)
            || _appUser.StartDayStatus.Equals(StartDayStatus.START_DAY)
            || _appUser.StartDayStatus.Equals(StartDayStatus.BLANK)
            */
                )
            {
                //CFD Done so no need to load route
            }
            else
            {
                IJPBeatHistory? alreadyStartedBeatHistory = _appUser.BeatHistories
                        .Where(e => Shared.CommonUtilities.Common.CommonFunctions.GetTimeDifferenceInDay(e.VisitDate,_appUser.JourneyStartDate) == 0 /*vk DateTime.Now.Date*/ && CommonFunctions.IsDateNull(e.EndTime)
                        && !Shared.CommonUtilities.Common.CommonFunctions.IsDateNull(e.StartTime))
                        .FirstOrDefault<IJPBeatHistory>();

                if (alreadyStartedBeatHistory == null)
                {
                    IJPBeatHistory? toStartBeatHistory = _appUser.BeatHistories
                        .Where(e => Shared.CommonUtilities.Common.CommonFunctions.GetTimeDifferenceInDay(e.VisitDate,_appUser.JourneyStartDate) == 0 /*DateTime.Now.Date*/ && CommonFunctions.IsDateNull(e.EndTime)
                        && Shared.CommonUtilities.Common.CommonFunctions.IsDateNull(e.StartTime))
                        .FirstOrDefault<IJPBeatHistory>();

                    if (toStartBeatHistory != null && _appUser.BeatHistories.Count == 1)
                    {
                        await StartBeatHistory(toStartBeatHistory);
                    }
                }
                else
                {
                    _appUser.SelectedBeatHistory = alreadyStartedBeatHistory; // Added by vishal on 13th Jul 2020
                    if (_appUser.SelectedBeatHistory.UserJourneyUID != _appUser.UserJourney.UID ||
                    _appUser.SelectedBeatHistory.JobPositionUID != _appUser.SelectedJobPosition.UID)
                    {
                        await _beatHistoryBL.UpdateUserJourneyUIDForBeatHistory(alreadyStartedBeatHistory);
                    }
                }
            }
            if (_appUser.SelectedBeatHistory != null)
            {
                SetIfLastRoute();
                SetSelectedRoute(_appUser.SelectedBeatHistory.RouteUID);
                if (_appUser.EmpInfo.CanHandleStock == true)
                {
                    _appUser.AlreadyCollectedLoadReqeustCountForRoute = await _beatHistoryBL.GetAlreadyCollectedLoadRequestCountForRoute();
                }
                _appUser.AlreadyVisitedCustomerCountForRoute = await _beatHistoryBL.GetAlreadyVisitedCustomerCountForRoute();
            }
            //_appUser.TodaysBeatCount = (_appUser.BeatHistories.FindAll(t => t.VisitDate.Date == DateTime.Now.Date) ?? new List<IJPBeatHistory>()).Count;
            List<string> jpRouteUIDList = _appUser.BeatHistories.Select(e => e.RouteUID).Distinct().ToList();
            _appUser.JPRoutes = _appUser.Routes.Where(e => jpRouteUIDList.Contains(e.UID)).ToList();
        }
    }
    public async Task OnRouteDD_Select(DropDownEvent dropDownEvent)
    {

        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            ISelectionItem beatHistorySelectionItem = dropDownEvent.SelectionItems.First();
            IJPBeatHistory beatHistory = _appUser.BeatHistories.Where(e1 => e1.RouteUID == beatHistorySelectionItem.UID).First();

            if (beatHistory != null)
            {
                if (beatHistory != null && beatHistory.ActionButtonText.Equals(CommonConstant.BLANK))
                {
                    throw new CustomException(ExceptionStatus.Failed, @_localizer["the_selected_route_aleady_closed_for_the_day."]);
                }
                else if (beatHistory != null && beatHistory.ActionButtonText.Equals(CommonConstant.STOP))
                {
                    throw new CustomException(ExceptionStatus.Failed, @_localizer["the_selected_route_already_active."]);
                }
                else if (beatHistory != null && beatHistory.ActionButtonText.Equals(CommonConstant.START))
                {
                    if (_appUser.AlreadyVisitedCustomerCountForRoute > 0)
                    {
                        throw new CustomException(ExceptionStatus.Failed, @_localizer["you_must_complete_the_current_route_before_you_start_a_new_route._journey_plan_reconciliation_may_also_be_required."]);
                    }
                    else if (_appUser.AlreadyCollectedLoadReqeustCountForRoute > 0)
                    {
                        throw new CustomException(ExceptionStatus.Failed, @_localizer["you_must_complete_the_current_route_before_you_start_a_new_route._journey_plan_reconciliation_may_also_be_required."]);
                    }
                    else
                    {
                        IJPBeatHistory? beatHistoryAlreadyStartedExceptCurrent = _appUser.BeatHistories
                        .Where(e1 => e1.UID != beatHistorySelectionItem.UID
                        && e1.ActionButtonText.Equals(CommonConstant.STOP, StringComparison.InvariantCultureIgnoreCase))
                        .FirstOrDefault<IJPBeatHistory>();

                        // If for already started beat any customer visited/skipped then alert will come that will correspond to stopping old beat and start new beat
                        // If not visited/skipped any customer then it will change by reversing old beat
                        if (beatHistoryAlreadyStartedExceptCurrent != null)
                        {
                            if (await _beatHistoryBL.OpenBeatHistory(beatHistoryAlreadyStartedExceptCurrent) > 0)
                            {
                                beatHistoryAlreadyStartedExceptCurrent.UpdatedMyStatus();
                            }
                        }
                        await StartBeatHistory(beatHistory);
                    }
                }
            }
        }
    }
    private void SetIfLastRoute()
    {
        List<IJPBeatHistory> notStartedBeatHistory = _appUser.BeatHistories
                .Where(e => e.ActionButtonTextLabel.Equals(CommonConstant.NOT_STARTED))
                .ToList<IJPBeatHistory>();
        if (notStartedBeatHistory != null && notStartedBeatHistory.Count == 0)
        {
            _appUser.SelectedBeatHistory.IsLastRoute = true;
        }
        else
        {
            _appUser.SelectedBeatHistory.IsLastRoute = false;
        }
    }
    private void SetSelectedRoute(string routeUID)
    {
        if (_appUser.Routes != null)
        {
            _appUser.SelectedRoute = _appUser.Routes
                .Where(e => e.UID == routeUID)
                .FirstOrDefault();
        }
    }
    private async Task StartBeatHistory(IJPBeatHistory beatHistory)
    {
        // Auto Start of Beat if only one beat
        if (await _beatHistoryBL.StartBeatHistory(beatHistory) > 0)
        {
            beatHistory.UpdatedMyStatus();// add this
            _appUser.IsBeatStartedFirstTime = true;
            // If only one route available it means this is last route so stock unload mandatory
            beatHistory.IsLastRoute = true;
            //_viewmodel.SelectedBeat = toStartBeatHistory;
            _appUser.SelectedBeatHistory = beatHistory; // Added by Vishal on 3rd Oct 2020
            _appUser.SelectedRoute = _appUser.Routes.Find(e => e.UID == beatHistory.RouteUID);
        }
    }

    public async Task StopBeatHistory(IBeatHistory beatHistory, string stockAuditUID)
    {
        DateTime stopTime = DateTime.Now;
        int returnValue = await _beatHistoryBL.UpdateStockAuditAndStopBeatHistory(stockAuditUID,beatHistory, stopTime);
        if (returnValue > 0)
        {
            _appUser.SelectedBeatHistory.HasAuditCompleted = true;
            _appUser.SelectedBeatHistory.WHStockAuditUID = stockAuditUID;
            _appUser.SelectedBeatHistory.EndTime = stopTime;
            _appUser.SelectedBeatHistory.UserJourneyVehicleUID = _appUser.UserJourneyVehicleUID;

            IUserJourney userJourney = _appUser.UserJourney;
            if (userJourney != null)
            {
                int returnValue2 = await _beatHistoryBL.UpdateBeatHistoryUIDInUserJourney(_appUser.SelectedBeatHistory.UID, userJourney.UID);
            }

            _appUser.SelectedBeatHistory.UpdatedMyStatus();
        }
    }
}
