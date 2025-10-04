using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.Store.BL.Classes;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.DL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.JourneyPlan.BL.Classes
{
    public class MyCustomerViewModel : JourneyPlanAppViewModel
    {
        public MyCustomerViewModel(IServiceProvider serviceProvider, IFilterHelper filter,
            ISortHelper sorter, IListHelper listHelper, IAppUser appUser, IAppConfig appConfigs, ApiService apiService,
            IBeatHistoryBL BeatHistoryBL, IStoreBL storeBL) : base(serviceProvider, filter, sorter, listHelper, appUser,
                appConfigs, apiService, BeatHistoryBL, storeBL)
        { }

        protected override async Task<IEnumerable<IStoreItemView>> GetCustomersForViewModel(string RouteUID,string BeatHistoryUID)
        {

            return await _storeBL.GetStoreByRouteUID(RouteUID, BeatHistoryUID, false);
        }

        public override async Task<int> CheckCustomerExistsInJourneyPlan(string StoreUID, string BeatHistoryUID)
        {
            return await _BeatHistoryBL.CheckCustomerExistsInJP(StoreUID, BeatHistoryUID);
        }
        public override async Task<int> AddCustomerInJourneyPlan(string UID,string StoreUID, string BeatHistoryUID)
        {
            
            return await _BeatHistoryBL.AddCustomerInJP(UID, StoreUID, BeatHistoryUID);
        }
        public async Task<List<string>> GetAllowedSKUByStoreUID(string storeUID)
        {
            return await _BeatHistoryBL.GetAllowedSKUByStoreUID(storeUID);
        }
    }
}
