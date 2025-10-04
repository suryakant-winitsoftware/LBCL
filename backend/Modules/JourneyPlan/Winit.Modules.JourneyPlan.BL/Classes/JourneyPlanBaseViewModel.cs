
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.JourneyPlan.DL.Interfaces;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Route.BL.Classes;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.JourneyPlan.BL.Classes
{
    public abstract class JourneyPlanBaseViewModel : IJourneyPlanViewModel
    {

        public IServiceProvider _serviceProvider;
        public IFilterHelper _filter;
        public ISortHelper _sorter;
        public IListHelper _listHelper;
        public IAppUser _appUser;
        public IAppConfig _appConfigs;
        public ApiService _apiService;
        private Winit.Modules.Route.BL.Interfaces.IRouteBL _routeBL;
        public Winit.Modules.Store.BL.Interfaces.IStoreBL _storeBL { get; set; }
        public IBeatHistoryBL _BeatHistoryBL { get; set; }

        public JourneyPlanBaseViewModel(IServiceProvider serviceProvider, IFilterHelper filter, ISortHelper sorter, IListHelper listHelper, IAppUser appUser, IAppConfig appConfigs, ApiService apiService, IBeatHistoryBL BeatHistoryBL, IStoreBL storeBL)
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

        }
        //SelectedCheckInReason
        public string SelectedCheckInReason { get; set; }
        // for skip customer
        public string SKIP_REASON { get; set; }
        public string Zero_Sales_Reason { get; set; }
        public string ScreenType { get; set; }
        public bool SkippingCustomersAgreed { get; set; }
        public bool JpSkipCustomerReason { get; set; }
        public bool IsInitialized { get; set; } = false;
        public Int64 Id { get; set; }
        public string CustomerItemViewUID { get; set; }
        // for journey plan 
        public IUserJourney UserJourney { get; set; }
        public IRoute SelectedRoute { get; set; }
        public string StartDayStatus { get; set; }
        public bool isForceCheckIn { get; set; } = false; // Set it accordingly
        // here you create variable SelectedTab
        public DateTime SelectedJPDate { get; set; }
        // this for  based on the this variable ui will be displayed like hide tabs and when it true only if SelectedBeatHistory == SelectedJPDate- model.JobPositionUID == model.BeatUID
        public bool IsCurrentDayJP { set; get; } = true;
        public IBeatHistory SelectedBeatHistory { get; set; }
        public string UserJourneyUID { get; set; } = "";
        public List<IBeatHistory> BeatHistoryList { get; set; }
        public List<IStoreItemView> CustomerItemViews { get; set; }
        public List<IStoreItemView> FilteredCustomerItemViews { get; set; }
        public List<IStoreItemView> DisplayedCustomerItemViews { get; set; }
        public List<IStoreItemView> CustomerItemViewsToStore { get; set; }
        public List<IStoreItemView> DisplayedCustomerItemViewws_Preview { get; set; }
        public List<IStoreItemView> SkippingCustomers { get; set; }
        public List<Winit.Modules.Org.Model.Interfaces.IOrgHeirarchy> ApplicableOrgHeirarchy { get; set; } 
        public List<IStoreMaster> StoreMasterDataForCustomers { get; set; }
        public Winit.Modules.Store.Model.Interfaces.IStoreCredit SelectedStoreCredit { get; set; }
        // public IEnumerable<IStoreItemView> SelectedCustomerItemViews { get; set; }
        public Winit.Modules.Store.Model.Interfaces.IStoreItemView SelectedCustomer { get; set; }
        //public Winit.Modules.Address.Model.Interfaces.IAddress DefaultAddress { get; set; }
        public List<FilterCriteria> FilterCriteriaList { get; set; }
        public List<SortCriteria> SortCriteriaList { get; set; }
        public Task ApplyFilter(List<FilterCriteria> filterCriterias, FilterMode filterMode)
        {
            throw new NotImplementedException();
        }
        public Task ApplySearch(string searchString)
        {
            throw new NotImplementedException();
        }
        public Task ApplySort(List<SortCriteria> sortCriterias)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        public async Task PageLoadInstances()
        {

            SelectedCustomer = _serviceProvider.CreateInstance<Winit.Modules.Store.Model.Interfaces.IStoreItemView>();
            ApplicableOrgHeirarchy = new List<Winit.Modules.Org.Model.Interfaces.IOrgHeirarchy>();
            await Task.CompletedTask;
        }
        public async Task GettheCustomersAndBindToProperty(string beatHistoryUID, string routeUID = null)
        {
            try
            {
                var selectedCustomers = await GetCustomersForViewModel(beatHistoryUID, routeUID);
                CustomerItemViewsToStore = selectedCustomers.ToList();
            }
            catch (Exception ex)
            {
                // Handle exception 
                throw ;
            }
        }
        protected virtual async Task<IEnumerable<IStoreItemView>> GetCustomersForViewModel(string beatHistoryUID, string routeUID)
        {
            // Default implementation or throw an exception to force derived classes to implement this method.
            throw new NotImplementedException("This method must be implemented in a derived class.");
           
        }
        public async Task PopulateViewModel()
        {
            if (_BeatHistoryBL == null || _storeBL == null)
            {
                return;
            }
            if(CustomerItemViewsToStore != null) {
                List<IStoreMaster> StoreMasterData = await _storeBL.PrepareStoreMaster(CustomerItemViewsToStore.Select(e => e.StoreUID).ToList<string>());

                StoreMasterDataForCustomers = StoreMasterData;
            }
            IsInitialized = true; // Set IsInitialized to true after populating data

        }
        public abstract Task<bool> PrepareDBForCheckIn(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeItemView);
        public abstract Task<int> UpdateStatusInStoreHistory(string StoreHistoryUID = "", string Status = "");
        public abstract Task<int> GetTimeDifferenceInMinute(DateTime startDate, DateTime? endDate);
        public abstract Task<bool> SetStoreHistoryStats(IStoreItemView storeItemView, int totalTimeInMin, bool isForceCheckIn, string UID, string empUID);
        public abstract Task<bool> SetExceptionLog(IStoreItemView storeItemView);
        public abstract Task<bool> UpdateBeathistory(string beatHistoryUID);
        public abstract Task<bool> PrepareDBForCheckout(IStoreItemView storeItemView);
        public abstract Task SetOrgHierarchy();

        public Task<List<string>> GetAllowedSKUByStoreUID(string storeUID)
        {
            throw new NotImplementedException();
        }
    }
}
