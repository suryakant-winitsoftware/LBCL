using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JourneyPlan.DL.Interfaces;

public interface IBeatHistoryDL
{
    Task<PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>> SelectAllBeatHistoryDetails
        (List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias,
        bool isCountRequired);
    Task<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> GetBeatHistoryByUID(string UID);
    Task<int> CreateBeatHistory(Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory beatHistory);
    Task<int> UpdateBeatHistory(Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory beatHistory);
    Task<int> DeleteBeatHistory(string UID);
    Task<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> GetSelectedBeatHistoryByRouteUID(string RouteUID);
    Task<IEnumerable<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>> GetAllBeatHistoriesByRouteUID(string RouteUID);
    Task<PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>> GetAllBeatHistoriesByRouteUID(string RouteUID, int pageNumber, int pageSize);
    // LATER IT MOVE TO ANOTHER Module
    Task<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStoreItemView>> GetCustomersByBeatHistoryUID
        (string BeatHistoryUID);
    Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrgHeirarchy>?> LoadOrgHeirarchy
        (Winit.Modules.Store.Model.Interfaces.IStoreItemView selectedcustomer);

    Task<int> UpdateStoreHistoryStatus(string StoreHistoryUID, string Status);
    Task<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStoreHistory>> GetStoreHistoriesByUserJourneyUID(string userJourneyUID);

    Task<int> CreateStoreHistoryStats(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeItemView,
        int totalTimeInMin, bool isForceCheckIn, string UID, string empUID);
    Task<int> CreateExceptionLog(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeItemView);

    Task<int> OnCheckout(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeItemView);

    Task<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> GetBeatSummaryFromStoreHistory(string BeatHistoryUID);

    Task<int> UpdateBeathistory_Checkout(Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory beatHistoryTarget,
        string BeatHistoryUID);

    Task<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStoreItemView>> GetCustomersForSelectedDate(DateTime dateTime);
    // this for mycustomer page 
    Task<int> CheckCustomerExistsInJP(string StoreUID, string BeatHistoryUID);
    Task<int> AddCustomerInJP(string UID, string StoreUID, string BeatHistoryUID);
    // this is for Cfd Module later it moves into cfd module

    Task<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStandardListSource>> GetPendingPushData();

    Task<int> GetNotVisitedCustomersFortheDay(DateTime JourneyStartTime, string RouteUid);

    Task<int> DeleteUserJourney();
    Task<int> UpdateUserJourney(IUserJourney userJourney);
    Task<int> UpdateBeatHistoryjourneyEndTime(Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory beatHistory);

    // this is for Start-day saving Userdata later i will move to Start-day Module

    Task<int> InsertUserJourney(IUserJourney userJourney);
    Task<List<IJPBeatHistory>> GetActiveOrTodayBeatHistory();
    Task<int> StartBeatHistory(IJPBeatHistory beatHistory);
    Task<int> OpenBeatHistory(IJPBeatHistory beatHistory);
    Task<int> UpdateUserJourneyUIDForBeatHistory(IJPBeatHistory beatHistory);
    Task<int> GetAlreadyCollectedLoadRequestCountForRoute();
    Task<int> GetAlreadyVisitedCustomerCountForRoute();
    Task<int> UpdateStockAuditAndStopBeatHistory(string stockAuditUID, IBeatHistory beatHistory, DateTime stopTime);
    Task<int> UpdateBeatHistoryUIDInUserJourney(string beatHistoryUID, string userJourneyUID);
    Task<bool> InsertMasterRabbitMQueue(MasterDTO masterDTO);
    Task<List<string>> GetAllowedSKUByStoreUID(string storeUID);
}