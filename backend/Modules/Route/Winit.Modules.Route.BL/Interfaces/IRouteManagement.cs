using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;

namespace Winit.Modules.Route.BL.Interfaces
{
    public interface IRouteManagement
    {
        // variables
        public bool IsInitialized { get; set; }
        public string SearchString { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRouteChangeLogItemsCount { get; set; }
        // this for fields
        public string RouteUID { get; set; }
        public Winit.Modules.Route.Model.Interfaces.IRoute Route { get; set; }
        public Winit.Modules.Route.Model.Interfaces.IRouteChangeLog RouteChangeLog { get; set; }
        public Winit.Modules.Route.Model.Interfaces.IRouteMasterView RouteMasterView { get; set; }
        public Winit.Modules.Route.Model.Interfaces.IRouteSchedule RouteSchedule { get; set; }
        public List<Winit.Modules.Route.Model.Interfaces.IRouteScheduleConfig> RouteScheduleConfigs { get; set; }
        public List<Winit.Modules.Route.Model.Interfaces.IRouteScheduleCustomerMapping> RouteScheduleCustomerMappings { get; set; }
        public List<Winit.Modules.Route.Model.Interfaces.IRouteCustomer> RouteCustomersList { get; set; }
        public List<Winit.Modules.Route.Model.Interfaces.IRouteCustomerItemView> RouteCustomerItemViews { get; set; }
        public List<Winit.Modules.Route.Model.Interfaces.IRouteCustomerItemView> FilteredRouteCustomerItemViews { get; set; }
        public List<Winit.Modules.Route.Model.Interfaces.IRouteCustomerItemView> DisplayRouteCustomerItemViews { get; set; }
        public List<Winit.Modules.Store.Model.Interfaces.IStoreCustomer> StoreCustomers { get; set; }
        public List<Winit.Shared.Models.Common.ISelectionItem> WareHouseSelectionItems { get; set; }
        public List<Winit.Shared.Models.Common.ISelectionItem> UserSelectionItems { get; set; }
        public List<Winit.Shared.Models.Common.ISelectionItem> OtherUserSelectionItems { get; set; }
        public List<Winit.Shared.Models.Common.ISelectionItem> VehicleSelectionItems { get; set; }
        public List<Winit.Shared.Models.Common.ISelectionItem> RoleSelectionItems { get; set; }
        public List<Winit.Modules.Role.Model.Interfaces.IRole> Roles { get; set; }
        public Winit.Modules.Role.Model.Interfaces.IRole SelectedRole { get; set; }
        public List<Winit.Modules.Route.Model.Interfaces.IRouteChangeLog> RouteChangeLogs { get; set; }
        public List<Winit.Modules.Route.Model.Interfaces.IRouteUser> RouteUsers { get; set; }
        public TimeOnly AutoFreezRunTime { get; set; }
        public List<FilterCriteria> FilterCriterias { get; set; }
        Task PopulateViewModel(string routeUID, bool isEditMode);
        Task<bool> ConfirmDelete(List<string> selectedRouteCustomerUIDs);
        Task OnRoleSelect(ISelectionItem item,bool isEditPage = false);
        Task BindAllDropdownsForEditpage();
        Task ApplySearch(string searchString);
        Task OnOtherUserSelection(DropDownEvent dropDownEvent);
        void HandleSelectedCustomers(List<IStoreCustomer> selectedCustomers);
        void ApplyTimes(TimeOnly startTime, TimeOnly endTime);
        void ApplyDuratinonAndTravelTime(int duration, int travelTime);
        void CalculateTimes();
        Task<bool> ValidateCustomerVistTimes();
        #region methods for main page
        Task PopulateRouteChangeLogGridData();
        Task PageIndexChanged(int pageNumber);
        Task ApplyFilter(Dictionary<string, string> keyValuePairs);
        Task ApplySort(SortCriteria sortCriteria);
        Task<bool> Update();
        Task<bool> Save();
        #endregion
    }
}
