using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JourneyPlan.BL.Interfaces;

public interface IBeatHistoryBL
{
    Task<PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>> SelectAllBeatHistoryDetails(
        List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias,
        bool isCountRequired);
    Task<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> GetBeatHistoryByUID(string UID);
    Task<int> CreateBeatHistory(Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory beatHistory);
    Task<int> UpdateBeatHistory(Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory beatHistory);
    Task<int> DeleteBeatHistory(string UID);
    Task<int> UpdateBeatHistoryjourneyEndTime(Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory beatHistory);
    //NIRANJAN
    Task<IBeatHistory> GetSelectedBeatHistoryByRouteUID(string RouteUID);
    Task<IEnumerable<IBeatHistory>> GetAllBeatHistoriesByRouteUID(string RouteUID);
    Task<PagedResponse<IBeatHistory>> GetAllBeatHistoriesByRouteUID(string RouteUID, int pageNumber, int pageSize);
    Task<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStoreItemView>> GetCustomersByBeatHistoryUID(
        string BeatHistoryUID);
    Task<IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrgHeirarchy>> LoadOrgHeirarchy
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
    Task<int> CheckCustomerExistsInJP(string StoreUID, string BeatHistoryUID);
    Task<int> AddCustomerInJP(string UID, string StoreUID, string BeatHistoryUID);
    Task<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStoreItemView>> GetCustomersForSelectedDate(DateTime dateTime);
    // this is for cfd module later move to cfd
    Task<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStandardListSource>> GetPendingPushData();
    Task<int> GetNotVisitedCustomersFortheDay(DateTime JourneyStartTime, string RouteUid);
    // this is for Start-day saving UserJourney
    Task<int> InsertUserJourney(IUserJourney userJourney);
    Task<List<IJPBeatHistory>> GetActiveOrTodayBeatHistory();
    Task<int> StartBeatHistory(IJPBeatHistory beatHistory);
    Task<int> OpenBeatHistory(IJPBeatHistory beatHistory);
    Task<int> UpdateUserJourneyUIDForBeatHistory(IJPBeatHistory beatHistory);
    Task<int> GetAlreadyCollectedLoadRequestCountForRoute();
    Task<int> GetAlreadyVisitedCustomerCountForRoute();
    Task<int> UpdateStockAuditAndStopBeatHistory(string stockAuditUID, IBeatHistory beatHistory, DateTime stopTime);
    Task<int> UpdateBeatHistoryUIDInUserJourney(string beatHistoryUID, string userJourneyUID);
    Task<int> DeleteUserJourney();

    Task<int> UpdateUserJourney(IUserJourney userJourney);
    Task<bool> InsertMasterRabbitMQueue(MasterDTO masterDTO);
    Task<List<string>> GetAllowedSKUByStoreUID(string storeUID);
}
