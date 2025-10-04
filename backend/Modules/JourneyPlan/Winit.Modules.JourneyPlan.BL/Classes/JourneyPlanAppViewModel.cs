using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.JourneyPlan.BL.Classes
{
    public  class JourneyPlanAppViewModel : JourneyPlanBaseViewModel
    {
        public JourneyPlanAppViewModel(IServiceProvider serviceProvider, IFilterHelper filter, 
            ISortHelper sorter, IListHelper listHelper, IAppUser appUser, IAppConfig appConfigs, ApiService apiService, 
            IBeatHistoryBL BeatHistoryBL , IStoreBL storeBL) : base(serviceProvider, filter, sorter, listHelper, appUser, 
                appConfigs, apiService,BeatHistoryBL, storeBL) { }

        protected override async Task<IEnumerable<IStoreItemView>> GetCustomersForViewModel(string beatHistoryUID, string routeUID = null)
        {
            // Specific implementation for JourneyPlanAppViewModel
            //return await _BeatHistoryBL.GetCustomersByBeatHistoryUID(beatHistoryUID);
            throw new NotImplementedException();
        }
        public virtual async Task<int> CheckCustomerExistsInJourneyPlan(string StoreUID, string BeatHistoryUID)
        {
            // Provide a default implementation here if needed
            throw new NotImplementedException();
        }

        public virtual async Task<int> AddCustomerInJourneyPlan(string UID, string StoreUID, string BeatHistoryUID)
        {
            // Provide a default implementation here if needed
            throw new NotImplementedException();
        }

        public override async Task<bool> PrepareDBForCheckIn(Winit.Modules.Store.Model.Interfaces.IStoreItemView SelectedCustomer)
        {
            int result = await UpdateStatusInStoreHistory(SelectedCustomer.StoreHistoryUID, Winit.Shared.Models.Constants.StoryHistoryStatus.VISITED);
            if (result >= 1)
            {
                if (true) // Later change to proper condition
                {
                    isForceCheckIn = true;
                    string GUIDForStoreHistoryStats = Guid.NewGuid().ToString();
                    int totalTimeInMin = await GetTimeDifferenceInMinute(SelectedCustomer.CheckInTime, SelectedCustomer.CheckOutTime);
                    if (await SetStoreHistoryStats(SelectedCustomer, totalTimeInMin, isForceCheckIn, GUIDForStoreHistoryStats, _appUser.Emp.UID))
                    {
                        SelectedCustomer.StoreHistoryStatsUID = GUIDForStoreHistoryStats;
                        if (SelectedCheckInReason != null)
                        {
                            SelectedCustomer.ExceptionType = "Force_Check_in";
                            SelectedCustomer.ExceptionReason = SelectedCheckInReason;
                            return await SetExceptionLog(SelectedCustomer);
                        }
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        public override async Task<int> UpdateStatusInStoreHistory(string StoreHistoryUID = "", string Status = "")
        {
            if (!string.IsNullOrEmpty(UserJourneyUID))
            {
                int result = await _BeatHistoryBL.UpdateStoreHistoryStatus(StoreHistoryUID, Status);
                return result;
            }

            return 0;
        }

        public override async Task<int> GetTimeDifferenceInMinute(DateTime startDate, DateTime? endDate)
        {
            if (endDate == null || startDate.Equals(DateTime.MinValue) || endDate.Equals(DateTime.MinValue))
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(((endDate.Value.Subtract(startDate)).TotalMinutes));
            }
        }

        public override async Task<bool> SetStoreHistoryStats(IStoreItemView storeItemView, int totalTimeInMin, bool isForceCheckIn, string UID, string empUID)
        {
            int result = await _BeatHistoryBL.CreateStoreHistoryStats(storeItemView, totalTimeInMin, isForceCheckIn, UID, empUID);
            return result >= 1;
        }

        public override async Task<bool> SetExceptionLog(IStoreItemView storeItemView)
        {
            int result = await _BeatHistoryBL.CreateExceptionLog(storeItemView);
            return result >= 1;
        }

        public override async Task<bool> UpdateBeathistory(string beatHistoryUID)
        {
            IBeatHistory beatHistoryTarget = await _BeatHistoryBL.GetBeatSummaryFromStoreHistory(beatHistoryUID);

            if (beatHistoryTarget != null)
            {
                int result = await _BeatHistoryBL.UpdateBeathistory_Checkout(beatHistoryTarget, beatHistoryUID);
                return result >= 1;
            }
            else
            {
                return false;
            }
        }

        public override async Task<bool> PrepareDBForCheckout(IStoreItemView storeItemView)
        {
            int result = await _BeatHistoryBL.OnCheckout(storeItemView);

            return result >= 1;
        }

        public override async Task SetOrgHierarchy()
        {
            if (_BeatHistoryBL == null)
            {
                return;
            }
            if (SelectedCustomer == null)
            {
                return;
            }
            IEnumerable<Winit.Modules.Org.Model.Interfaces.IOrgHeirarchy> OrgHeirarchy = await _BeatHistoryBL.LoadOrgHeirarchy(SelectedCustomer);
            ApplicableOrgHeirarchy = OrgHeirarchy.ToList();
        }
        public async Task<List<string>> GetAllowedSKUByStoreUID(string storeUID)
        {
            return await _BeatHistoryBL.GetAllowedSKUByStoreUID(storeUID);
        }
    }


}
