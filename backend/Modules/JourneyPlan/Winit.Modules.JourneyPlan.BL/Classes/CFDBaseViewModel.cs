using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.UIComponents.Common.Language;
using Microsoft.Extensions.Localization;
using Winit.UIComponents.Common.LanguageResources.Web;
using System.Globalization;
using System.Resources;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.Common.BL.Classes;

namespace Winit.Modules.JourneyPlan.BL.Classes
{
    public class CFDBaseViewModel : ICFDViewModel
    {
        public IServiceProvider _serviceProvider;
        public IFilterHelper _filter;
        public ISortHelper _sorter;
        public IListHelper _listHelper;
        public IAppUser _appUser;
        public IAppConfig _appConfigs;
        public ApiService _apiService;
        public readonly ILanguageService _languageService;
        private IStringLocalizer<LanguageKeys> _localizer;
        private Winit.Modules.Route.BL.Interfaces.IRouteBL _routeBL;
        public Winit.Modules.Store.BL.Interfaces.IStoreBL _storeBL { get; set; }
        public IBeatHistoryBL _BeatHistoryBL { get; set; }


        public CFDBaseViewModel(IServiceProvider serviceProvider, IFilterHelper filter, ISortHelper sorter, IListHelper listHelper, IAppUser appUser, IAppConfig appConfigs, ApiService apiService, IBeatHistoryBL BeatHistoryBL, IStoreBL storeBL, ILanguageService Languageservice, IStringLocalizer<LanguageKeys> Localizer)
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
            _languageService = Languageservice;
            _localizer = Localizer;
        }
        public IUserJourney UserJourney { get; set; }
        public IRoute SelectedRoute { get; set; }
        public string StartDayStatus { get; set; }
        public IBeatHistory SelectedBeatHistory { get; set; }
        // variables
        public bool IsOnline { get; set; }
        public string UserJourneyUID { get; set; }
        public int StartReading { get; set; }
        public int EndReading { get; set; }
        public string UploadStatus { get; set; }
        public string EOTStatus { get; set; }
        public string AuditStatus { get; set; }
        public DateTime? JourneyStartTime { get; set; }
        public DateTime? JourneyEndTime { get; set; }
        public bool HasAuditCompleted { get; set; }
        public int NonVisited { get; set; }
        public bool IsStockAuditCompleted { get; set; }
        public bool IsSyncPushPending { get; set; }
        // public WinIT.mSFA.Shared.sFAModel.VehicleStatus OldSelectedVehicleStatus { get; set; }
        public string WHStockRequestUID { get; set; } = string.Empty;
        public bool IsOldRouteOpen { get; set; } = false;
        public IBeatHistory beatHistoryToClose { get; set; }
        public List<Winit.Modules.Store.Model.Interfaces.IStandardListSource> StandardListSourceList { get; set; }
        //methodes
        public async Task PopulateViewModel()
        {
            LoadResources(null, _languageService.SelectedCulture);
            await UpdateUploadStatus();
            if (JourneyStartTime != null)
            {
                await GetNotVisitedCustomersFortheDay((DateTime)JourneyStartTime, SelectedRoute?.UID);
            }
        }
        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
            _localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }

        public async Task UpdateUploadStatus()
        {
            var res = await IsDataPendingToPush();
            IsSyncPushPending = res.IsPending;
            UploadStatus = !IsSyncPushPending ? @_localizer["completed"] : @_localizer["pending"];
        }
        public async Task<(bool IsPending, string ErrorMessage)> IsDataPendingToPush()
        {
            List<Winit.Modules.Store.Model.Interfaces.IStandardListSource> standardListSources = await GetPendingPushData();
            if (standardListSources != null && standardListSources.Count > 0)
            {
                string pendingTransactionString = string.Join("\n", standardListSources
                    .Select(e => e.SourceType + " : " + e.SourceLabel));
                //return (true, "There are some unposted data that need to be posted before clearing data from system. System will sync data then try again.");
                return (true, @_localizer["there_are_some_unposted_data_of_previous_logged_in_user,_please_sync_the_data_to_continue."]);
            }
            return (false, "");
        }
        public async Task<List<Winit.Modules.Store.Model.Interfaces.IStandardListSource>> GetPendingPushData()
        {

            IEnumerable<Winit.Modules.Store.Model.Interfaces.IStandardListSource> StandardListSourceList = await _BeatHistoryBL.GetPendingPushData();
            return StandardListSourceList.ToList();
        }

        public async Task GetNotVisitedCustomersFortheDay(DateTime JourneyStartTime, string RouteUid)
        {
            NonVisited = await _BeatHistoryBL.GetNotVisitedCustomersFortheDay(JourneyStartTime, RouteUid);
        }

        public async Task DeleteUserJourneyColumns()
        {
            var a = _BeatHistoryBL.DeleteUserJourney();
        }

        //public async Task<bool> UpdateBeatHistoryAndUserJourney()
        //{
        //    try
        //    {
        //        // Update SelectedBeatHistory and set CFDTime to the current time
        //        SelectedBeatHistory.CFDTime = DateTime.Now;
        //        SelectedBeatHistory.ModifiedTime = DateTime.Now;
        //        SelectedBeatHistory.ModifiedBy = _appUser?.Emp?.UID ?? "";
        //        int beatHistoryUpdateStatus = await _BeatHistoryBL.UpdateBeatHistoryjourneyEndTime(SelectedBeatHistory);

        //        // Check if the update was successful
        //        if (beatHistoryUpdateStatus <= 0)
        //        {
        //            return false; // Return false if update failed
        //        }

        //        // Update UserJourney properties
        //        UserJourney.SS = 2;
        //        UserJourney.EOTStatus = "Complete";
        //        UserJourney.JourneyEndTime = DateTime.Now;

        //        // Calculate JourneyTime as the difference between JourneyEndTime and JourneyStartTime
        //        UserJourney.JourneyTime = (UserJourney.JourneyEndTime - UserJourney.JourneyStartTime).ToString();

        //        // Update UserJourney in the database
        //        int userJourneyUpdateStatus = await _BeatHistoryBL.UpdateUserJourney(UserJourney);

        //        // Check if the update was successful
        //        if (userJourneyUpdateStatus <= 0)
        //        {
        //            return false; // Return false if update failed
        //        }

        //        // Update the UI with the latest data
        //        //_appuser.SelectedBeatHistory = SelectedBeatHistory;
        //       // _appuser.UserJourney = UserJourney;

        //        // Return true indicating success
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle the error (log it or notify the user, if needed)
        //        Console.WriteLine($"Error: {ex.Message}");
        //        return false; // Return false if an error occurred
        //    }
        //}


        //new (26/05/2025) - NIRANJAN - 
        public async Task<bool> UpdateBeatHistoryAndUserJourney()
        {
            try
            {
                if (SelectedBeatHistory != null)
                {
                    // Update SelectedBeatHistory and set CFDTime to the current time
                    SelectedBeatHistory.CFDTime = DateTime.Now;
                    SelectedBeatHistory.ModifiedTime = DateTime.Now;
                    SelectedBeatHistory.ModifiedBy = _appUser?.Emp?.UID ?? "";
                    int beatHistoryUpdateStatus = await _BeatHistoryBL.UpdateBeatHistoryjourneyEndTime(SelectedBeatHistory);

                    // Check if the update was successful
                    if (beatHistoryUpdateStatus <= 0)
                    {
                        return false; // Return false if update failed
                    }
                }

                // Update UserJourney properties
                UserJourney.SS = 2;
                UserJourney.EOTStatus = "Complete";
                UserJourney.JourneyEndTime = DateTime.Now;

                // Calculate JourneyTime as the difference between JourneyEndTime and JourneyStartTime
                UserJourney.JourneyTime = (UserJourney.JourneyEndTime - UserJourney.JourneyStartTime).ToString();

                // Update UserJourney in the database
                int userJourneyUpdateStatus = await _BeatHistoryBL.UpdateUserJourney(UserJourney);

                // Check if the update was successful
                if (userJourneyUpdateStatus <= 0)
                {
                    return false; // Return false if update failed
                }

                // Update the ViewModel properties to reflect the changes
                this.EOTStatus = "Complete";
                this.JourneyEndTime = UserJourney.JourneyEndTime;

                // Update upload status after successful local updates
                await UpdateUploadStatus();

                return true;
            }
            catch (Exception ex)
            {
                // Handle the error (log it or notify the user, if needed)
                Console.WriteLine($"Error: {ex.Message}");
                return false; // Return false if an error occurred
            }
        }
    }
}
