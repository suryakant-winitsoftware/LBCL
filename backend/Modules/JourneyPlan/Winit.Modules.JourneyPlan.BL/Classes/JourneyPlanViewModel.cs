using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.JourneyPlan.DL.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.JourneyPlan.BL.Classes
{
    public class JourneyPlanViewModel : JourneyPlanAppViewModel
    {
        public JourneyPlanViewModel(IServiceProvider serviceProvider, IFilterHelper filter,
            ISortHelper sorter, IListHelper listHelper, IAppUser appUser, IAppConfig appConfigs, ApiService apiService,
            IBeatHistoryBL BeatHistoryBL, IStoreBL storeBL) : base(serviceProvider, filter, sorter, listHelper, appUser,
                appConfigs, apiService, BeatHistoryBL, storeBL)
        { }
        protected override async Task<IEnumerable<IStoreItemView>> GetCustomersForViewModel(string beatHistoryUID, string routeUID = null)
        {
            // Specific implementation for JourneyPlanViewModel
            return await _BeatHistoryBL.GetCustomersByBeatHistoryUID(beatHistoryUID);
        }

        public async Task<IEnumerable<IStoreItemView>> GetCustomersForJourneyPlan(string BeatHistoryUID)
        {
            return await _BeatHistoryBL.GetCustomersByBeatHistoryUID(BeatHistoryUID);
        }

        public async Task<IEnumerable<IStoreItemView>> GetCustomersForSelectedDate(DateTime dateTime)
        {
            return await _BeatHistoryBL.GetCustomersForSelectedDate(dateTime);
        }

        public async Task<IEnumerable<IStoreItemView>> GetCustomersUnplanned(string RouteUID ,string BeatHistoryUID , bool notInJP)
        {

            return await _storeBL.GetStoreByRouteUID(RouteUID, BeatHistoryUID, notInJP);
        }

        public override async Task<int> CheckCustomerExistsInJourneyPlan(string StoreUID, string BeatHistoryUID)
        {
            return await _BeatHistoryBL.CheckCustomerExistsInJP(StoreUID, BeatHistoryUID);
        }
        public override async Task<int> AddCustomerInJourneyPlan(string UID, string StoreUID, string BeatHistoryUID)
        {

            return await _BeatHistoryBL.AddCustomerInJP(UID, StoreUID, BeatHistoryUID);
        }
        public async Task<List<string>> GetAllowedSKUByStoreUID(string storeUID)
        {
            return await _BeatHistoryBL.GetAllowedSKUByStoreUID(storeUID);
        }
    }
}
