using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Enums;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.JourneyPlan.BL.Interfaces
{
    public interface IJourneyPlanViewModel
    {
        public bool IsInitialized { get; set; }
        public long Id { get; set; }
        public string CustomerItemViewUID { get; set; }
        public List<IBeatHistory> BeatHistoryList { get; set; }
        public List<IStoreItemView> CustomerItemViews { get; set; }
        public List<IStoreItemView> FilteredCustomerItemViews { get; set; }
        public List<IStoreItemView> DisplayedCustomerItemViews { get; set; }
        public List<IStoreItemView> CustomerItemViewsToStore { get; set; }
        public List<IStoreItemView> DisplayedCustomerItemViewws_Preview { get; set; }
        public List<IStoreMaster> StoreMasterDataForCustomers { get; set; }
        public Winit.Modules.Store.Model.Interfaces.IStoreCredit SelectedStoreCredit { get; set; }
        public List<Winit.Modules.Org.Model.Interfaces.IOrgHeirarchy> ApplicableOrgHeirarchy { get; set; }
        // for journey plan 
        public IUserJourney UserJourney { get; set; }
        public IRoute SelectedRoute { get; set; }
        public string StartDayStatus { get; set; }
        public IBeatHistory SelectedBeatHistory { get; set; }
        // here you create variable SelectedTab
        public DateTime SelectedJPDate { get; set; }
        // this for  based on the this variable ui will be displayed like hide tabs 
        public bool IsCurrentDayJP { set; get; }
        public string UserJourneyUID { get; set; }
        public Winit.Modules.Store.Model.Interfaces.IStoreItemView SelectedCustomer { get; set; }
        //public Winit.Modules.Address.Model.Interfaces.IAddress DefaultAddress { get; set; }
        public List<FilterCriteria> FilterCriteriaList { get; set; }
        public List<SortCriteria> SortCriteriaList { get; set; }
        public string SelectedCheckInReason { get; set; }
        // for skip customer
        public string SKIP_REASON { get; set; }
        public string Zero_Sales_Reason { get; set; }
        public string ScreenType { get; set; }
        public bool SkippingCustomersAgreed { get; set; }
        public bool JpSkipCustomerReason { get; set; }
        // check-in and check-out time 
        Task<int> UpdateStatusInStoreHistory(string StoreHistoryUID = "", string Status = "");
        Task<bool> PrepareDBForCheckIn(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeItemView);
        Task<bool> SetExceptionLog(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeItemView);
        Task<bool> SetStoreHistoryStats(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeItemView, int totalTimeInMin, bool isForceCheckIn, string UID, string empUID);
        Task<bool> UpdateBeathistory(string beatHistoryUID);
        Task<bool> PrepareDBForCheckout(IStoreItemView storeItemView);
        Task PopulateViewModel();
        Task GettheCustomersAndBindToProperty(string beatHistoryUID, string routeUID = null);
        Task SetOrgHierarchy();
        void Dispose();
        Task ApplyFilter(List<FilterCriteria> filterCriterias, FilterMode filterMode);
        Task ApplySearch(string searchString);
        Task ApplySort(List<SortCriteria> sortCriterias);
        Task<List<string>> GetAllowedSKUByStoreUID(string storeUID);
    }
}
