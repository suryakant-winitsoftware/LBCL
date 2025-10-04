using Newtonsoft.Json;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Modules.Route.BL.Interfaces;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;

namespace Winit.Modules.Route.BL.Classes;

public class RouteManagementBaseViewModel : IRouteManagement
{
    // variables
    public bool IsInitialized { get; set; }
    public string SearchString { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; }
    public int TotalRouteChangeLogItemsCount { get; set; }
    private readonly IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private readonly IListHelper _listHelper;
    private readonly IAppUser _appUser;
    private readonly IAppConfig _appConfigs;
    private readonly ApiService _apiService;

    // required lists and required instances
    public string RouteUID { get; set; }
    public Winit.Modules.Route.Model.Interfaces.IRoute Route { get; set; }
    public Winit.Modules.Route.Model.Interfaces.IRouteChangeLog RouteChangeLog { get; set; }
    public Winit.Modules.Route.Model.Interfaces.IRouteMasterView RouteMasterView { get; set; }
    public Winit.Modules.Route.Model.Interfaces.IRouteSchedule RouteSchedule { get; set; }
    public List<Winit.Modules.Route.Model.Interfaces.IRouteScheduleConfig> RouteScheduleConfigs { get; set; }
    public List<Winit.Modules.Route.Model.Interfaces.IRouteScheduleCustomerMapping> RouteScheduleCustomerMappings { get; set; }
    public List<Winit.Modules.Route.Model.Interfaces.IRouteCustomer> RouteCustomersList { get; set; }
    public List<Winit.Modules.Store.Model.Interfaces.IStoreCustomer> StoreCustomers { get; set; }
    public List<Winit.Modules.Route.Model.Interfaces.IRouteCustomerItemView> RouteCustomerItemViews { get; set; }
    public List<Winit.Modules.Route.Model.Interfaces.IRouteCustomerItemView> FilteredRouteCustomerItemViews { get; set; }
    public List<Winit.Modules.Route.Model.Interfaces.IRouteCustomerItemView> DisplayRouteCustomerItemViews { get; set; }
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
    public string ValidationMessage { get; private set; }
    private readonly List<string> _propertiesToSearch = new();
    public List<FilterCriteria> FilterCriterias { get; set; }
    public List<SortCriteria> SortCriterias { get; set; }
    private TimeOnly DayStarts;
    private TimeOnly DayEnds;
    private int Duration;
    private int TravelTime;
    //Constructor
    public RouteManagementBaseViewModel(IServiceProvider serviceProvider, IFilterHelper filter, ISortHelper sorter,
        IListHelper listHelper, IAppUser appUser, IAppConfig appConfigs, ApiService apiService
        )
    {
        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _listHelper = listHelper;
        _appUser = appUser;
        _appConfigs = appConfigs;
        _apiService = apiService;
        // Initialize common properties or perform other common setup
        RouteChangeLogs = new List<Model.Interfaces.IRouteChangeLog>();
        _propertiesToSearch = new List<string>
        {
            "StoreCode",
            "StoreLabel"
        };
        Route = _serviceProvider.CreateInstance<Winit.Modules.Route.Model.Interfaces.IRoute>();
        RouteChangeLog = _serviceProvider.CreateInstance<Winit.Modules.Route.Model.Interfaces.IRouteChangeLog>();
        RouteMasterView = _serviceProvider.CreateInstance<Winit.Modules.Route.Model.Interfaces.IRouteMasterView>();
        RouteSchedule = _serviceProvider.CreateInstance<Winit.Modules.Route.Model.Interfaces.IRouteSchedule>();
        RouteScheduleConfigs = new List<Winit.Modules.Route.Model.Interfaces.IRouteScheduleConfig>();
        RouteScheduleCustomerMappings = new List<Winit.Modules.Route.Model.Interfaces.IRouteScheduleCustomerMapping>();
        RouteCustomersList = new List<Winit.Modules.Route.Model.Interfaces.IRouteCustomer>();
        RouteCustomerItemViews = new List<IRouteCustomerItemView>();
        FilteredRouteCustomerItemViews = new List<IRouteCustomerItemView>();
        DisplayRouteCustomerItemViews = new List<IRouteCustomerItemView>();
        WareHouseSelectionItems = new List<Winit.Shared.Models.Common.ISelectionItem>();
        VehicleSelectionItems = new List<Winit.Shared.Models.Common.ISelectionItem>();
        UserSelectionItems = new List<Winit.Shared.Models.Common.ISelectionItem>();
        OtherUserSelectionItems = new List<Winit.Shared.Models.Common.ISelectionItem>();
        RoleSelectionItems = new List<Winit.Shared.Models.Common.ISelectionItem>();
        Roles = new List<IRole>();
        RouteUsers = new List<IRouteUser>();
        RouteChangeLog.ValidFrom = DateTime.Now;
        RouteChangeLog.ValidUpto = new DateTime(2099, 12, 31);
        FilterCriterias = new List<FilterCriteria>();
        SortCriterias = new List<SortCriteria>();
    }

    public async Task PopulateRouteChangeLogGridData()
    {
        RouteChangeLogs.Clear();
        RouteChangeLogs.AddRange(await GetRouteChangeLogDataFromAPIAsync());
    }

    public async Task PopulateViewModel(string routeUID, bool isEditMode)
    {
        RouteUID = routeUID;
        RouteChangeLog.UID = routeUID;
        RouteChangeLog.OrgUID = _appUser.SelectedJobPosition.OrgUID;
        var roles = await GetRoles(_appUser.SelectedJobPosition.OrgUID);
        if (roles != null)
        {
            Roles.Clear();
            Roles.AddRange(roles);
            RoleSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IRole>
                (roles, new List<string> { "UID", "Code", "RoleNameEn" }));
        }
        _ = await LoadAllStoreCustomerList();
        if (isEditMode)
        {
            //_ = await GetVehiclesdata(_appUser.SelectedJobPosition.OrgUID);
            //_ = await GetWareHouses(CommonConstant.FRWH, _appUser.SelectedJobPosition.OrgUID);
            //_ = await GetUser(_appUser.SelectedJobPosition.OrgUID);
            _ = await GetRouteDataByUID(RouteUID);
            _ = await GetRouteDetailsByUID(RouteUID);
            var selectedRoleSelectionItem = RoleSelectionItems.Find(e => e.UID == RouteChangeLog.RoleUID);
            if (selectedRoleSelectionItem != null)
            {
                selectedRoleSelectionItem.IsSelected = true;
                await OnRoleSelect(selectedRoleSelectionItem, true);
                var selectedUser = UserSelectionItems.Find(e => e.UID == RouteChangeLog.JobPositionUID);
                if (selectedUser != null)
                {
                    selectedUser.IsSelected = true;
                    OtherUserSelectionItems.RemoveAll(e => e.UID == selectedUser.UID);
                }
                foreach (var routeUser in RouteUsers)
                {
                    var otherUser = OtherUserSelectionItems.Find(e => e.UID == routeUser.JobPositionUID);
                    if (otherUser != null) otherUser.IsSelected = true;
                }
                if (SelectedRole.HaveVehicle)
                {
                    var selectedVehicle = VehicleSelectionItems.Find(e => e.UID == RouteChangeLog.VehicleUID);
                    if (selectedVehicle != null) selectedVehicle.IsSelected = true;
                }
                if (SelectedRole.HaveWarehouse)
                {
                    var selectedWarehouse = WareHouseSelectionItems.Find(e => e.UID == RouteChangeLog.WHOrgUID);
                    if (selectedWarehouse != null) selectedWarehouse.IsSelected = true;
                }
            }
            RouteCustomerItemViews.Clear();
            FilteredRouteCustomerItemViews.Clear();
            DisplayRouteCustomerItemViews.Clear();
            foreach (IRouteCustomer item in RouteCustomersList)
            {
                IRouteCustomerItemView routeCustomerItemView = ConvertRouteCustomerToRouteCustomerItemView(item);
                routeCustomerItemView.ActionType = Shared.Models.Enums.ActionType.Update;
                RouteCustomerItemViews.Add(routeCustomerItemView);
                FilteredRouteCustomerItemViews.Add(routeCustomerItemView);
                DisplayRouteCustomerItemViews.Add(routeCustomerItemView);
            }
        }
    }
    public async Task OnRoleSelect(ISelectionItem item, bool iseditPage = false)
    {
        SelectedRole = Roles.Find(e => item.UID == e.UID)!;
        if (!iseditPage)
        {
            RouteChangeLog.RoleUID = item.UID;
            RouteChangeLog.WHOrgUID = null;
            RouteChangeLog.JobPositionUID = null;
            RouteChangeLog.VehicleUID = null;
        }
        RouteUsers.ForEach(e => e.ActionType = ActionType.Delete);
        if (SelectedRole.HaveVehicle && !VehicleSelectionItems.Any())
        {
            VehicleSelectionItems.Clear();
            VehicleSelectionItems.AddRange(await GetVehiclesdata(_appUser.SelectedJobPosition.OrgUID));
        }
        if (SelectedRole.HaveWarehouse && !WareHouseSelectionItems.Any())
        {
            WareHouseSelectionItems.Clear();
            WareHouseSelectionItems.AddRange(await GetWareHouses(CommonConstant.FRWH, _appUser.SelectedJobPosition.OrgUID));
        }
        await GetUser(_appUser.SelectedJobPosition.OrgUID, SelectedRole.UID);
    }
    public async Task<bool> ConfirmDelete(List<string> selectedRouteCustomerUIDs)
    {
        try
        {
            // Call the API to delete the selected customers
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}RouteCustomer/DeleteRouteCustomerDetails",
                HttpMethod.Delete, selectedRouteCustomerUIDs);

            // Check if the API call was successful
            if (apiResponse.IsSuccess)
            {
                return true; // Indicate success
            }
            else
            {
                // Handle other cases where the response doesn't contain a string
                // You can add your error logic or notification here
                return false; // Indicate failure
            }
        }
        catch (Exception)
        {
            // Handle unexpected exceptions
            // You can log or display an error message here
            return false; // Indicate failure
        }
    }

    public async Task PageIndexChanged(int pageNumber)
    {
        PageNumber = pageNumber;
        await PopulateRouteChangeLogGridData();
    }
    public async Task ApplySearch(string searchString)
    {
        DisplayRouteCustomerItemViews.Clear();
        DisplayRouteCustomerItemViews.AddRange(await _filter.ApplySearch<Winit.Modules.Route.Model.Interfaces.IRouteCustomerItemView>(
            FilteredRouteCustomerItemViews, searchString, _propertiesToSearch)
            );
    }

    #region handeling methods
    public async Task BindAllDropdownsForEditpage()
    {
        if (RouteChangeLog != null)
        {
            //bind selected warehouse
            ISelectionItem? selectedWareHouse = WareHouseSelectionItems.Find(e => e.UID == RouteChangeLog.WHOrgUID);
            if (selectedWareHouse != null)
            {
                selectedWareHouse.IsSelected = true;
            }

            //bind selected user
            ISelectionItem? selectedUser = UserSelectionItems?.Find(e => e.UID == RouteChangeLog?.JobPositionUID);
            if (selectedUser != null)
            {
                selectedUser.IsSelected = true;
            }

            //bind selected other user 
            //ISelectionItem selectedOtherUser = OtherUserSelectionItems?.Find(e => e.UID == RouteChangeLog?.oth);
            //if(selectedOtherUser != null) selectedUser.IsSelected = true;

            //bind selected vehicle
            ISelectionItem? selectedVehicle = VehicleSelectionItems?.Find(e => e.UID == RouteChangeLog?.VehicleUID);
            if (selectedVehicle != null)
            {
                selectedVehicle.IsSelected = true;
            }

            //bind Other User
            List<string>? selectedRouteUsers = RouteUsers.Select(e => e.JobPositionUID)?.ToList();
            if (selectedRouteUsers != null && selectedRouteUsers.Any())
            {
                OtherUserSelectionItems?.ForEach(user =>
                {
                    if (selectedRouteUsers.Contains(user.UID))
                    {
                        user.IsSelected = true;
                    }
                });
            }
        }
        await Task.CompletedTask;
    }
    public async Task OnOtherUserSelection(DropDownEvent dropDownEvent)
    {
        RouteUsers.ForEach(e => e.ActionType = Winit.Shared.Models.Enums.ActionType.Delete);
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
        {
            List<string>? selectedUsers = dropDownEvent.SelectionItems.Select(e => e.UID)?.ToList();
            if (selectedUsers != null)
            {
                List<ISelectionItem> item = dropDownEvent.SelectionItems;
                // Remove the selected user from the list of users (first dropdown)
                UserSelectionItems = UserSelectionItems.Where(user => !selectedUsers.Contains(user.UID)).ToList();
                dropDownEvent.SelectionItems.ForEach(e =>
                {
                    IRouteUser? routeUser = RouteUsers.Find(user => user.JobPositionUID == e.UID);
                    if (routeUser != null)
                    {
                        routeUser.ActionType = Shared.Models.Enums.ActionType.Update;
                    }
                    else
                    {
                        RouteUsers.Add(new RouteUser
                        {
                            UID = Guid.NewGuid().ToString(),
                            ModifiedBy = _appUser?.Emp?.UID ?? "WINIT",
                            CreatedBy = _appUser?.Emp?.UID ?? "WINIT",
                            ModifiedTime = DateTime.Now,
                            CreatedTime = DateTime.Now,
                            JobPositionUID = e.UID,
                            RouteUID = RouteUID,
                            IsActive = true
                        });
                    }
                });
            }
        }
        await Task.CompletedTask;
    }
    public void HandleSelectedCustomers(List<IStoreCustomer> selectedCustomers)
    {
        foreach (IStoreCustomer storeCustomer in selectedCustomers)
        {
            IRouteCustomerItemView? routeCustomerItemView = RouteCustomerItemViews.Find(e => e.StoreUID == storeCustomer.UID);
            if (routeCustomerItemView == null)
            {
                routeCustomerItemView = ConvertStoreCustomerToRouteCustomerItemView(storeCustomer, RouteCustomerItemViews.Count + 1);
                routeCustomerItemView.ActionType = Shared.Models.Enums.ActionType.Add;
                RouteCustomerItemViews.Add(routeCustomerItemView);
                FilteredRouteCustomerItemViews.Add(routeCustomerItemView);
                DisplayRouteCustomerItemViews.Add(routeCustomerItemView);
            }
        }
        UpdateTotalCustomersCount();
        CalculateTimes();
    }
    private void UpdateTotalCustomersCount()
    {
        if (RouteChangeLog != null)
        {
            RouteChangeLog.TotalCustomers = RouteCustomerItemViews?.Count ?? 0;
        }
    }
    public void ApplyTimes(TimeOnly startTime, TimeOnly endTime)
    {
        DayStarts = new TimeOnly(startTime.Hour, startTime.Minute);
        DayEnds = new TimeOnly(endTime.Hour, endTime.Minute);
        RouteChangeLog.VisitTime = new TimeSpan(hours: startTime.Hour, minutes: startTime.Minute, seconds: 0).ToString();
        RouteChangeLog.EndTime = new TimeSpan(hours: endTime.Hour, minutes: endTime.Minute, seconds: 0).ToString();
        CalculateTimes();
    }
    public void ApplyDuratinonAndTravelTime(int duration, int travelTime)
    {
        Duration = duration;
        TravelTime = travelTime;
        RouteChangeLog.VisitDuration = duration;
        RouteChangeLog.TravelTime = travelTime;
        SetDurationAndTravelTime();
        CalculateTimes();
    }
    private void SetDurationAndTravelTime()
    {
        RouteCustomerItemViews.ForEach(e => { e.VisitDuration = Duration; e.TravelTime = TravelTime; });
    }
    public void CalculateTimes()
    {
        //if(DayStarts!=default && DayEnds!=default && Duration!=default && TravelTime!= default)
        //{
        //TimeOnly timeOnly = new TimeOnly(DayStarts.Hour,DayStarts.Minute);
        //timeOnly = timeOnly.AddMinutes((TravelTime + Duration) * RouteCustomerItemViews.Count);
        //if (timeOnly > DayEnds)
        //{
        //    throw new Exception("Time Exceeds the days end Time");
        //}
        //else
        //{
        TimeOnly timeOnlySample = new(DayStarts.Hour, DayEnds.Minute);
        foreach (IRouteCustomerItemView item in RouteCustomerItemViews)
        {
            item.VisitTime = new TimeSpan(hours: timeOnlySample.Hour, minutes: timeOnlySample.Minute, seconds: 0).ToString(@"hh\:mm\:ss");
            timeOnlySample = timeOnlySample.AddMinutes(item.VisitDuration);
            item.EndTime = new TimeSpan(hours: timeOnlySample.Hour, minutes: timeOnlySample.Minute, seconds: 0).ToString(@"hh\:mm\:ss");
            timeOnlySample = timeOnlySample.AddMinutes(item.TravelTime);

        }
        //}
        //}
    }
    public Task<bool> ValidateCustomerVistTimes()
    {
        // return Task.FromResult(DayEnds >= TimeOnly.FromTimeSpan(RouteCustomerItemViews.Last().EndTime));

        string lastEndTimeString = RouteCustomerItemViews.Last().EndTime;
        if (TimeOnly.TryParse(lastEndTimeString, out TimeOnly lastEndTime))
        {
            return Task.FromResult(DayEnds >= lastEndTime);
        }
        return Task.FromResult(false);
    }
    #endregion
    #region conversion methods
    private IRouteCustomerItemView ConvertStoreCustomerToRouteCustomerItemView(IStoreCustomer storeCustomer, int seqNo)
    {
        IRouteCustomerItemView routeCustomerItemView = new RouteCustomerItemView
        {
            StoreUID = storeCustomer.UID,
            RouteUID = RouteUID,
            StoreCode = storeCustomer.Code,
            StoreLabel = storeCustomer.Label,
            Address = storeCustomer.Address,
            SeqNo = seqNo,
        };
        if (RouteChangeLog.IsCustomerWithTime)
        {
            routeCustomerItemView.VisitDuration = Duration;
            routeCustomerItemView.TravelTime = TravelTime;
            ApplyDuratinonAndTravelTime(Duration, TravelTime);
        }

        AddCreateFields(routeCustomerItemView, true);
        return routeCustomerItemView;
    }
    private IRouteCustomerItemView ConvertRouteCustomerToRouteCustomerItemView(IRouteCustomer routeCustomer)
    {
        IRouteCustomerItemView routeCustomerItemView = new RouteCustomerItemView
        {
            UID = routeCustomer.UID,
            StoreUID = routeCustomer.StoreUID,//storeCustomer.UID,
            RouteUID = routeCustomer.RouteUID,
            //StoreCode = "",//storeCustomer.Code,
            //StoreLabel = "",//storeCustomer.Label,
            CreatedBy = routeCustomer.CreatedBy,
            ModifiedBy = _appUser?.Emp?.UID ?? "WINIT",
            ModifiedTime = System.DateTime.Now,
            CreatedTime = routeCustomer.CreatedTime,
            VisitDuration = routeCustomer.VisitDuration,
            VisitTime = routeCustomer.VisitTime,
            EndTime = routeCustomer.EndTime,
            TravelTime = routeCustomer.TravelTime,
            ServerAddTime = routeCustomer.ServerAddTime,
            ServerModifiedTime = routeCustomer.ServerModifiedTime,
            IsDeleted = routeCustomer.IsDeleted,
            //Address = "",
            SeqNo = routeCustomer.SeqNo,
        };
        MapRouteCustomersFromStoreCustomers(routeCustomerItemView);
        AddUpdateFields(routeCustomerItemView);
        return routeCustomerItemView;
    }
    private void MapRouteCustomersFromStoreCustomers(IRouteCustomerItemView routeCustomer)
    {
        IStoreCustomer? storeCustomer = StoreCustomers.Find(e => e.UID == routeCustomer.StoreUID);
        if (storeCustomer != null)
        {
            routeCustomer.StoreLabel = storeCustomer.Label;
            routeCustomer.Address = storeCustomer.Address;
            routeCustomer.StoreCode = storeCustomer.Code;
        }
    }
    //private IRouteCustomer ConvertRouteCustomerItemViewToRouteCustomer(IRouteCustomerItemView routeCustomerItemView)
    //{
    //    IRouteCustomer routeCustomer = new RouteCustomer
    //    {
    //        UID = routeCustomerItemView.UID,
    //        CreatedTime = routeCustomerItemView.CreatedTime,
    //        ModifiedTime = routeCustomerItemView.ModifiedTime,
    //        CreatedBy = ro
    //    }
    //}
    private void AddCreateFields(IBaseModel baseModel, bool IsUIDRequired)
    {
        baseModel.CreatedBy = _appUser?.Emp?.UID ?? "WINIT";//_appUser.Emp.UID;
        baseModel.ModifiedBy = _appUser?.Emp?.UID ?? "WINIT";//_appUser.Emp.UID;
        baseModel.CreatedTime = DateTime.Now;
        baseModel.ModifiedTime = DateTime.Now;
        if (IsUIDRequired)
        {
            baseModel.UID = Guid.NewGuid().ToString();
        }
    }
    private void AddUpdateFields(IBaseModel baseModel)
    {
        baseModel.ModifiedBy = _appUser?.Emp?.UID ?? "WINIT"; //_appUser.Emp.UID;
        baseModel.ModifiedTime = DateTime.Now;
    }

    public async Task<bool> Update()
    {
        try
        {
            RouteChangeLog.AutoFreezeRunTime = AutoFreezRunTime.ToString();
            AddUpdateFields(RouteChangeLog);
            AddUpdateFields(RouteSchedule);
            RouteMasterView = _serviceProvider.CreateInstance<Winit.Modules.Route.Model.Interfaces.IRouteMasterView>();
            {
                RouteMasterView.Route = RouteChangeLog;
                RouteMasterView.RouteSchedule = RouteSchedule;
                RouteMasterView.RouteCustomersList = RouteCustomerItemViews.ToList<IRouteCustomer>();
                RouteMasterView.RouteUserList = RouteUsers;
            };

            // Set up new scheduling system
            if (RouteScheduleConfigs != null && RouteScheduleConfigs.Any())
            {
                foreach (var config in RouteScheduleConfigs)
                {
                    AddUpdateFields(config);
                }
                RouteMasterView.RouteScheduleConfigs = RouteScheduleConfigs;
            }
            
            if (RouteScheduleCustomerMappings != null && RouteScheduleCustomerMappings.Any())
            {
                foreach (var mapping in RouteScheduleCustomerMappings)
                {
                    AddUpdateFields(mapping);
                }
                RouteMasterView.RouteScheduleCustomerMappings = RouteScheduleCustomerMappings;
            }

            string jsonData = JsonConvert.SerializeObject(RouteMasterView);
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Route/UpdateRouteMaster",
                HttpMethod.Put, RouteMasterView);

            // Check if the API call was successful
            if (apiResponse.IsSuccess)
            {
                return true; // Indicate success
            }
            else
            {
                return false; // Indicate failure
            }
        }
        catch (Exception)
        {
            return false; // Indicate failure
        }
    }

    public async Task<bool> Save()
    {
        try
        {
            RouteChangeLog.AutoFreezeRunTime = AutoFreezRunTime.ToString();
            AddCreateFields(RouteChangeLog, false);
            RouteChangeLog.UID = $"{_appUser.SelectedJobPosition.OrgUID}_{RouteChangeLog.Code}";
            AddCreateFields(RouteSchedule, true);
            RouteSchedule.RouteUID = RouteChangeLog.UID;

            RouteMasterView = _serviceProvider.CreateInstance<Winit.Modules.Route.Model.Interfaces.IRouteMasterView>();
            {
                //RouteChangeLog.JobPositionUID = _appUser?.SelectedJobPosition?.UID;
                RouteMasterView.Route = RouteChangeLog;
                RouteMasterView.RouteSchedule = RouteSchedule;
                foreach (var item in RouteCustomerItemViews)
                {
                    item.RouteUID = RouteChangeLog.UID;
                }
                RouteMasterView.RouteCustomersList = RouteCustomerItemViews.ToList<IRouteCustomer>();
                foreach (var item in RouteUsers)
                {
                    item.RouteUID = RouteChangeLog.UID;
                }
                RouteMasterView.RouteUserList = RouteUsers;
            };

            // Set up new scheduling system for create
            if (RouteScheduleConfigs != null && RouteScheduleConfigs.Any())
            {
                foreach (var config in RouteScheduleConfigs)
                {
                    AddCreateFields(config, true);
                }
                RouteMasterView.RouteScheduleConfigs = RouteScheduleConfigs;
            }
            
            if (RouteScheduleCustomerMappings != null && RouteScheduleCustomerMappings.Any())
            {
                foreach (var mapping in RouteScheduleCustomerMappings)
                {
                    AddCreateFields(mapping, true);
                    mapping.RouteScheduleUID = RouteSchedule.UID;
                }
                RouteMasterView.RouteScheduleCustomerMappings = RouteScheduleCustomerMappings;
            }
            //string jsonData = JsonConvert.SerializeObject(RouteMasterView);

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Route/CreateRouteMaster",
                HttpMethod.Post, RouteMasterView);

            // Check if the API call was successful
            if (apiResponse.IsSuccess)
            {
                // Return true to indicate success
                return true;
            }
            else
            {
                // Return false to indicate failure
                return false;
            }
        }
        catch (Exception)
        {
            // Log the exception or handle it as needed
            // Return false to indicate failure
            return false;
        }
    }
    #endregion
    #region api calling methods
    private async Task<List<Winit.Modules.Route.Model.Interfaces.IRouteChangeLog>> GetRouteChangeLogDataFromAPIAsync()
    {
        try
        {
            PagingRequest pagingRequest = new()
            {
                PageSize = PageSize,
                PageNumber = PageNumber,
                IsCountRequired = true,
                FilterCriterias = FilterCriterias,
                SortCriterias = SortCriterias
            };
            ApiResponse<PagedResponse<Winit.Modules.Route.Model.Classes.RouteChangeLog>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Route.Model.Classes.RouteChangeLog>>(
                $"{_appConfigs.ApiBaseUrl}Route/SelectRouteChangeLogAllDetails",
                HttpMethod.Post, pagingRequest);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                //string data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
                //PagedResponse<Winit.Modules.Route.Model.Classes.RouteChangeLog> pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Route.Model.Classes.RouteChangeLog>>(data);

                if (apiResponse.Data.PagedData is not null and not null)
                {
                    for (int i = 0; i < apiResponse.Data.PagedData.Count(); i++)
                    {
                        apiResponse.Data.PagedData.ToList()[i].serialNumber = ((PageNumber - 1) * PageSize) + i + 1;
                    }
                    if (apiResponse.Data.TotalCount >= 0)
                    {
                        TotalRouteChangeLogItemsCount = apiResponse.Data.TotalCount;
                    }
                    return apiResponse.Data.PagedData.ToList<Winit.Modules.Route.Model.Interfaces.IRouteChangeLog>();
                }
            }
            else
            {
                TotalRouteChangeLogItemsCount = 0;
            }
            return new List<Model.Interfaces.IRouteChangeLog>();
        }
        catch (Exception)
        {
            TotalRouteChangeLogItemsCount = 0;
            throw;
        }
    }
    private async Task<bool> GetRouteDataByUID(string routeUid)
    {
        try
        {
            if (!string.IsNullOrEmpty(routeUid))
            {
                // Fetch route data by UID
                ApiResponse<Winit.Modules.Route.Model.Classes.Route> apiResponse = await _apiService.FetchDataAsync<Winit.Modules.Route.Model.Classes.Route>(
                    $"{_appConfigs.ApiBaseUrl}Route/SelectRouteDetailByUID?UID={routeUid}",
                    HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    Route = apiResponse.Data;
                }
            }
            else
            {
                Route = _serviceProvider.CreateInstance<Winit.Modules.Route.Model.Interfaces.IRoute>();
                return true; // Indicate success
            }
        }
        catch (Exception)
        {
            // Handle unexpected exceptions
            // Log or display a generic error message here if needed
        }

        return false; // Indicate failure
    }
    private async Task<List<ISelectionItem>> GetWareHouses(string frwh, string orgUid)
    {
        try
        {
            ApiResponse<List<SelectionItem>> apiResponse = await _apiService.FetchDataAsync<List<SelectionItem>>(
                $"{_appConfigs.ApiBaseUrl}Route/GetWareHouseDDL?OrgTypeUID={frwh}&&ParentUID={orgUid}",
                HttpMethod.Get);

            if (apiResponse != null && apiResponse.Data != null && apiResponse.Data.Any())
            {

                return apiResponse.Data.ToList<ISelectionItem>();
            }
        }
        catch (Exception)
        {
        }
        return new(); // Indicate failure
    }
    private async Task<List<ISelectionItem>> GetVehiclesdata(string orgUid)
    {
        try
        {
            ApiResponse<List<SelectionItem>> apiResponse = await _apiService.FetchDataAsync<List<SelectionItem>>(
                $"{_appConfigs.ApiBaseUrl}Route/GetVehicleDDL?orgUID={orgUid}",
                HttpMethod.Get);

            if (apiResponse != null && apiResponse.Data != null && apiResponse.Data.Any())
            {
                return apiResponse.Data.ToList<ISelectionItem>();
            }
        }
        catch (Exception)
        {
            // Handle exceptions
        }

        return new(); // Indicate failure
    }
    private async Task<List<IRole>?> GetRoles(string orgUid)
    {
        try
        {
            ApiResponse<List<Winit.Modules.Role.Model.Classes.Role>> apiResponse =
                await _apiService.FetchDataAsync<List<Winit.Modules.Role.Model.Classes.Role>>(
                $"{_appConfigs.ApiBaseUrl}Role/SelectRolesByOrgUID?orgUID={orgUid}&IsAppUser=true",
                HttpMethod.Get);
            if (apiResponse != null && apiResponse.Data != null)
            {
                return apiResponse.Data.ToList<IRole>();
            }
            return default;
        }
        catch (Exception)
        {
            throw;
        }
    }
    private async Task<bool> GetUser(string orgUid, string roleUID)
    {
        try
        {
            ApiResponse<List<SelectionItem>> apiResponse =
                await _apiService.FetchDataAsync<List<SelectionItem>>(
                $"{_appConfigs.ApiBaseUrl}Emp/GetEmployeesSelectionItemByRoleUID?orgUID={orgUid}&roleUID={roleUID}",
                HttpMethod.Get);

            if (apiResponse != null && apiResponse.Data != null && apiResponse.Data.Any())
            {
                List<ISelectionItem> otherUSerItems = new();
                apiResponse.Data.ForEach(e => { otherUSerItems.Add((e as SelectionItem).DeepCopy()!); });
                UserSelectionItems.Clear();
                UserSelectionItems.AddRange(apiResponse.Data);
                OtherUserSelectionItems.Clear();
                OtherUserSelectionItems.AddRange(otherUSerItems);
                return true; // Indicate success
            }
        }
        catch (Exception)
        {
            // Handle exceptions
        }

        return false; // Indicate failure
    }
    public async Task<bool> GetRouteDetailsByUID(string routeUid)
    {
        try
        {
            if (!string.IsNullOrEmpty(routeUid))
            {
                ApiResponse<Winit.Modules.Route.Model.Classes.RouteMaster> apiResponse = await _apiService.FetchDataAsync<Winit.Modules.Route.Model.Classes.RouteMaster>(
                    $"{_appConfigs.ApiBaseUrl}Route/SelectRouteMasterViewByUID?UID={routeUid}",
                    HttpMethod.Get);

                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    RouteChangeLog = apiResponse.Data.Route ?? _serviceProvider.CreateInstance<Winit.Modules.Route.Model.Interfaces.IRouteChangeLog>();
                    RouteSchedule = apiResponse.Data.RouteSchedule ?? _serviceProvider.CreateInstance<Winit.Modules.Route.Model.Interfaces.IRouteSchedule>();
                    // Load new scheduling data
                    RouteScheduleConfigs = apiResponse.Data.RouteScheduleConfigs?.ToList<Winit.Modules.Route.Model.Interfaces.IRouteScheduleConfig>() ?? new List<Winit.Modules.Route.Model.Interfaces.IRouteScheduleConfig>();
                    RouteScheduleCustomerMappings = apiResponse.Data.RouteScheduleCustomerMappings?.ToList<Winit.Modules.Route.Model.Interfaces.IRouteScheduleCustomerMapping>() ?? new List<Winit.Modules.Route.Model.Interfaces.IRouteScheduleCustomerMapping>();
                    RouteCustomersList = apiResponse.Data.RouteCustomersList == null
                        ? new List<Winit.Modules.Route.Model.Interfaces.IRouteCustomer>()
                        : apiResponse.Data.RouteCustomersList.ToList<Winit.Modules.Route.Model.Interfaces.IRouteCustomer>()
                            .Select(i => { i.ActionType = Winit.Shared.Models.Enums.ActionType.Update; return i; })
                            .ToList();
                    RouteUsers = apiResponse.Data.RouteUserList?.ToList<IRouteUser>() ?? new List<IRouteUser>();
                    RouteUsers.ForEach(e => e.ActionType = Shared.Models.Enums.ActionType.Update);
                    if (RouteChangeLog.AutoFreezeRunTime != null)
                    {
                        AutoFreezRunTime = TimeOnly.Parse(RouteChangeLog.AutoFreezeRunTime);
                    }
                    // Code for buttons - assuming these are properties in your ViewModel
                    // selectedWRHTxt = RouteChangeLog.WHOrgUID;
                    // selectedVehicleTxt = RouteChangeLog.VehicleUID;
                    // selectedUserTxt = RouteChangeLog.JobPositionUID;
                    RouteCustomersList = RouteCustomersList.OrderBy(e => e.SeqNo).ToList();
                    return true; // Operation succeeded
                }
            }
            else
            {
                // Initialize a new route model for adding
                RouteChangeLog = _serviceProvider.CreateInstance<Winit.Modules.Route.Model.Interfaces.IRouteChangeLog>();
                RouteSchedule = _serviceProvider.CreateInstance<Winit.Modules.Route.Model.Interfaces.IRouteSchedule>();
                RouteScheduleConfigs = new List<Winit.Modules.Route.Model.Interfaces.IRouteScheduleConfig>();
                RouteScheduleCustomerMappings = new List<Winit.Modules.Route.Model.Interfaces.IRouteScheduleCustomerMapping>();
                RouteCustomersList = new List<Winit.Modules.Route.Model.Interfaces.IRouteCustomer>();

                return true; // Operation succeeded
            }
        }
        catch (Exception)
        {
            // Handle unexpected exceptions
            // Log or display an error message as needed
        }

        return false; // Operation failed
    }
    public async Task<bool> LoadAllStoreCustomerList()
    {
        try
        {
            PagingRequest pagingRequest = new();
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Store/GetAllStoreItems?OrgUID={_appUser.SelectedJobPosition.OrgUID}",
                HttpMethod.Post, pagingRequest);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                PagedResponse<Winit.Modules.Store.Model.Classes.StoreCustomer>? pagedResponse = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Store.Model.Classes.StoreCustomer>>(data);
                if (pagedResponse != null)
                {
                    IEnumerable<IStoreCustomer> listofCustomers = pagedResponse.PagedData;
                    StoreCustomers = listofCustomers.ToList();
                    return true; // Indicate success
                }
            }
            else
            {
            }
            return false; // Indicate failure
        }
        catch (Exception)
        {
            // Handle unexpected exceptions
            // Log or display a generic error message here if needed
            return false; // Indicate failure
        }
    }
    public async Task ApplyFilter(Dictionary<string, string> keyValuePairs)
    {
        FilterCriterias.Clear();
        foreach (KeyValuePair<string, string> item in keyValuePairs)
        {
            if (!string.IsNullOrEmpty(item.Value))
            {
                FilterCriterias.Add(new FilterCriteria(item.Key, item.Value, FilterType.Equal));
            }
        }
        await PopulateRouteChangeLogGridData();
    }
    public async Task ApplySort(SortCriteria sortCriteria)
    {
        SortCriterias.Clear();
        SortCriterias.Add(sortCriteria);
        await PopulateRouteChangeLogGridData();
    }
    #endregion
}