using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JourneyPlan.BL.Classes;

public class BeatHistoryBL : Winit.Modules.JourneyPlan.BL.Interfaces.IBeatHistoryBL
{
    protected readonly DL.Interfaces.IBeatHistoryDL _beatHistoryDL = null;
    public BeatHistoryBL(DL.Interfaces.IBeatHistoryDL beatHistoryDL)
    {
        _beatHistoryDL = beatHistoryDL;
    }
    public async Task<PagedResponse<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>> SelectAllBeatHistoryDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        return await _beatHistoryDL.SelectAllBeatHistoryDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
    }
    public async Task<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory> GetBeatHistoryByUID(string UID)
    {
        return await _beatHistoryDL.GetBeatHistoryByUID(UID);
    }
    public async Task<int> CreateBeatHistory(Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory beatHistory)
    {
        return await _beatHistoryDL.CreateBeatHistory(beatHistory);
    }
    public async Task<int> UpdateBeatHistory(Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory beatHistory)
    {
        return await _beatHistoryDL.UpdateBeatHistory(beatHistory);
    }
    public async Task<int> DeleteBeatHistory(string UID)
    {
        return await _beatHistoryDL.DeleteBeatHistory(UID);
    }
    public async Task<IBeatHistory> GetSelectedBeatHistoryByRouteUID(string RouteUID)
    {
        return await _beatHistoryDL.GetSelectedBeatHistoryByRouteUID(RouteUID);
    }

    public async Task<IEnumerable<IBeatHistory>> GetAllBeatHistoriesByRouteUID(string RouteUID)
    {
        return await _beatHistoryDL.GetAllBeatHistoriesByRouteUID(RouteUID);
    }

    public async Task<PagedResponse<IBeatHistory>> GetAllBeatHistoriesByRouteUID(string RouteUID, int pageNumber, int pageSize)
    {
        return await _beatHistoryDL.GetAllBeatHistoriesByRouteUID(RouteUID, pageNumber, pageSize);
    }

    public async Task<IEnumerable<IStoreItemView>> GetCustomersByBeatHistoryUID(string BeatHistoryUID)
    {
        return await _beatHistoryDL.GetCustomersByBeatHistoryUID(BeatHistoryUID);
    }

    public async Task<IEnumerable<IOrgHeirarchy>> LoadOrgHeirarchy(IStoreItemView selectedcustomer)
    {
        return await _beatHistoryDL.LoadOrgHeirarchy(selectedcustomer);
    }

    public async Task<int> UpdateStoreHistoryStatus(string StoreHistoryUID, string Status)
    {
        return await _beatHistoryDL.UpdateStoreHistoryStatus(StoreHistoryUID, Status);
    }

    public async Task<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStoreHistory>> GetStoreHistoriesByUserJourneyUID(string userJourneyUID)
    {
        return await _beatHistoryDL.GetStoreHistoriesByUserJourneyUID(userJourneyUID);
    }

    public async Task<int> CreateStoreHistoryStats(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeItemView,
                                                    int totalTimeInMin, bool isForceCheckIn, string UID, string empUID)
    {
        return await _beatHistoryDL.CreateStoreHistoryStats(storeItemView, totalTimeInMin, isForceCheckIn, UID, empUID);
    }

    public async Task<int> CreateExceptionLog(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeItemView)
    {
        return await _beatHistoryDL.CreateExceptionLog(storeItemView);
    }
    public async Task<int> OnCheckout(IStoreItemView storeItemView)
    {
        return await _beatHistoryDL.OnCheckout(storeItemView);
    }

    public async Task<IBeatHistory> GetBeatSummaryFromStoreHistory(string BeatHistoryUID)
    {
        return await _beatHistoryDL.GetBeatSummaryFromStoreHistory(BeatHistoryUID);
    }

    public async Task<int> UpdateBeathistory_Checkout(IBeatHistory beatHistoryTarget, string BeatHistoryUID)
    {
        return await _beatHistoryDL.UpdateBeathistory_Checkout(beatHistoryTarget, BeatHistoryUID);
    }
    public async Task<int> CheckCustomerExistsInJP(string StoreUID, string BeatHistoryUID)
    {
        return await _beatHistoryDL.CheckCustomerExistsInJP(StoreUID, BeatHistoryUID);
    }
    public async Task<int> AddCustomerInJP(string UID, string StoreUID, string BeatHistoryUID)
    {
        return await _beatHistoryDL.AddCustomerInJP(UID, StoreUID, BeatHistoryUID);
    }

    public async Task<IEnumerable<IStoreItemView>> GetCustomersForSelectedDate(DateTime dateTime)
    {
        return await _beatHistoryDL.GetCustomersForSelectedDate(dateTime);
    }

    public async Task<IEnumerable<IStandardListSource>> GetPendingPushData()
    {
        return await _beatHistoryDL.GetPendingPushData();
    }

    public async Task<int> GetNotVisitedCustomersFortheDay(DateTime JourneyStartTime, string RouteUid)
    {
        return await _beatHistoryDL.GetNotVisitedCustomersFortheDay(JourneyStartTime, RouteUid);
    }

    public async Task<int> InsertUserJourney(IUserJourney userJourney)
    {
        return await _beatHistoryDL.InsertUserJourney(userJourney);
    }

    public async Task<List<IJPBeatHistory>> GetActiveOrTodayBeatHistory()
    {
        return await _beatHistoryDL.GetActiveOrTodayBeatHistory();
    }

    public async Task<int> StartBeatHistory(IJPBeatHistory beatHistory)
    {
        return await _beatHistoryDL.StartBeatHistory(beatHistory);
    }

    public async Task<int> OpenBeatHistory(IJPBeatHistory beatHistory)
    {
        return await _beatHistoryDL.OpenBeatHistory(beatHistory);
    }

    public async Task<int> UpdateUserJourneyUIDForBeatHistory(IJPBeatHistory beatHistory)
    {
        return await _beatHistoryDL.UpdateUserJourneyUIDForBeatHistory(beatHistory);
    }

    public async Task<int> GetAlreadyCollectedLoadRequestCountForRoute()
    {
        return await _beatHistoryDL.GetAlreadyCollectedLoadRequestCountForRoute();
    }

    public async Task<int> GetAlreadyVisitedCustomerCountForRoute()
    {
        return await _beatHistoryDL.GetAlreadyVisitedCustomerCountForRoute();
    }
    public async Task<int> UpdateStockAuditAndStopBeatHistory(string stockAuditUID, IBeatHistory beatHistory, DateTime stopTime)
    {
        return await _beatHistoryDL.UpdateStockAuditAndStopBeatHistory(stockAuditUID, beatHistory, stopTime);
    }
    public async Task<int> UpdateBeatHistoryUIDInUserJourney(string beatHistoryUID, string userJourneyUID)
    {
        return await _beatHistoryDL.UpdateBeatHistoryUIDInUserJourney(beatHistoryUID, userJourneyUID);
    }

    public async Task<int> DeleteUserJourney()
    {
        return await _beatHistoryDL.DeleteUserJourney();
    }
    public async Task<bool> InsertMasterRabbitMQueue(MasterDTO masterDTO)
    {
        return await _beatHistoryDL.InsertMasterRabbitMQueue(masterDTO);
    }

    public async Task<int> UpdateUserJourney(IUserJourney userJourney)
    {
        return await _beatHistoryDL.UpdateUserJourney(userJourney);
    }

    public async Task<int> UpdateBeatHistoryjourneyEndTime(IBeatHistory beatHistory)
    {
        return await _beatHistoryDL.UpdateBeatHistoryjourneyEndTime(beatHistory);
    }
    public async Task<List<string>> GetAllowedSKUByStoreUID(string storeUID)
    {
        return await _beatHistoryDL.GetAllowedSKUByStoreUID(storeUID);
    }
}
