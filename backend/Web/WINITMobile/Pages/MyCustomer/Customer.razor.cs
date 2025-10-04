using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.Model.Constants;
using Winit.Modules.JourneyPlan.BL.Classes;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.CustomControls;
using Winit.UIComponents.Common.Services;
namespace WINITMobile.Pages.MyCustomer;

public partial class Customer
{
    [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }

    public List<Winit.Modules.Store.Model.Interfaces.IStoreItemView> Customerlist { get; set; }
    public List<Winit.Modules.Store.Model.Interfaces.IStoreItemView> CustomerlisttoShow { get; set; }

    public Winit.Modules.JourneyPlan.BL.Interfaces.IJourneyPlanViewModel _journeyplanviewmodel { get; set; }

    public List<WINITMobile.State.CustomerLatlng> CustomersWithLatLon { get; set; } = new List<WINITMobile.State.CustomerLatlng>();
    public IStoreItemView SelectedStoreItemView { get; set; }

    public IStoreMaster storeMaster { get; set; }
    public Winit.Modules.Route.Model.Interfaces.IRoute SelectedRouteMyCustomer { get; set; }
    private bool SalesOrganizationPop { get; set; }
    private bool IsVisible { get; set; }
    private bool IsOpnedScoreCredit { get; set; }
    private bool ShowMap { get; set; }

    private async Task ShowMapModal()
    {

        ShowMap = true;
        await Task.CompletedTask;
    }
    private void HideMapModal()
    {
        ShowMap = false;
    }
    private WinitTextBox wtbSearch;

    private async void OpenDialPad()
    {
        try
        {
            // Check and request permissions
            var status = await Permissions.CheckStatusAsync<Permissions.Phone>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Phone>();
            }

            // If permission is granted, open the dial pad
            if (status == PermissionStatus.Granted)
            {
                // Additional check to ensure that the feature is available on the current platform
                if (PhoneDialer.Default.IsSupported)
                {
                    PhoneDialer.Default.Open("9436742566");
                }
                else
                {
                    await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["phone_dialer_is_not_supported_on_this_device"]);
                }
            }
            else
            {

                await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["permission_not_granted_to_use_the_phone_dialer"]);
            }
        }
        catch (FeatureNotSupportedException ex)
        {
            await _alertService.ShowErrorAlert(@Localizer["phone_dialer_not_supported"], ex.Message);

        }
        catch (PermissionException ex)
        {
            await _alertService.ShowErrorAlert(@Localizer["permission_not_granted"], ex.Message);
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert(@Localizer["an_error_occurred"], ex.Message);
        }
    }

    protected override async void OnInitialized()
    {
        try
        {
            if (_viewmodel != null)
            {
                _journeyplanviewmodel = _viewmodel.CreateJourneyPlanViewModel(Winit.Shared.Models.Constants.ScreenType.MyCustomer);
            }

            // Initialize Customerlist here
            //await SetJourneyPlanViewModel(Winit.Shared.Models.Constants.ScreenType.MyCustomer);

            if (_journeyplanviewmodel.CustomerItemViewsToStore != null)
            {
                Customerlist = _journeyplanviewmodel.CustomerItemViewsToStore;
                CustomerlisttoShow = Customerlist;
                _journeyplanviewmodel.ScreenType = Winit.Shared.Models.Constants.ScreenType.MyCustomer;
                _journeyplanviewmodel.UserJourney = _appUser.UserJourney;
                _journeyplanviewmodel.SelectedRoute = _appUser.SelectedRoute;
                _journeyplanviewmodel.SelectedBeatHistory = _appUser.SelectedBeatHistory;
            }
            if (_appUser?.SelectedBeatHistory != null)
            {
                _journeyplanviewmodel.SelectedBeatHistory = _appUser.SelectedBeatHistory;
            }
            if (_appUser?.UserJourney != null)
            {
                _journeyplanviewmodel.UserJourney = _appUser.UserJourney;
            }
            if (_appUser?.SelectedRoute != null)
            {
                _journeyplanviewmodel.SelectedRoute = _appUser.SelectedRoute;
            }

            // this for map 
            // if (CustomersWithLatLon.Count == 0) { CustomersWithLatLon = await GetCustomersLatlng(Customerlist); }
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert(@Localizer["an_error_occurred"], ex.Message);
        }
        LoadResources(null, _languageService.SelectedCulture);

    }


    protected override async Task OnInitializedAsync()
    {

        _backbuttonhandler.SetCurrentPage(this);
        if (_appUser.SelectedRoute == null)
        {
            // Show the error alert
            await _alertService.ShowErrorAlert("Error", "There is no selected route. Please select the route to continue.");

            // Navigate to the dashboard page
            _navigationManager.NavigateTo("/DashBoard");

            return;
        }
        if (_appUser.UserJourney == null)
        {
            // Show the error alert
            await _alertService.ShowErrorAlert("Error", "There is no UserJourney . Please start the day to continue.");

            // Navigate to the dashboard page
            _navigationManager.NavigateTo("/DashBoard");

            return;
        }
        if (_appUser?.StartDayStatus != null)
        {
            if (_appUser.StartDayStatus.Equals(StartDayStatus.EOT_DONE))
            {
                await _alertService.ShowErrorAlert("", "EOD has been Completed for the day. you can't perform any transaction today.");
                _navigationManager.NavigateTo("/DashBoard");

                return;
            }
        }
        if (!_journeyplanviewmodel.IsInitialized)
        {
            _loadingService.ShowLoading(@Localizer["loading_journey_plan"]);
            await InvokeAsync(async () =>
            {
                try
                {
                    if (_appUser.SelectedBeatHistory != null && _appUser.SelectedBeatHistory.UID != null)
                    {
                        await LoadCustomers(_appUser.SelectedRoute.UID, _appUser.SelectedBeatHistory.UID);
                        await _journeyplanviewmodel.PopulateViewModel();
                        Customerlist = _journeyplanviewmodel.CustomerItemViewsToStore;
                        // if (CustomersWithLatLon.Count == 0 && Customerlist.Count >0) { CustomersWithLatLon = await GetCustomersLatlng(Customerlist); }
                        // IEnumerable<IStoreItemView> SelectedCustomers = await GetCustomersForViewModel(UID);
                    }
                    await InvokeAsync(async () =>
                    {
                        await SetTopBar();
                        _loadingService.HideLoading();
                        StateHasChanged();
                    });
                }
                catch (Exception ex)
                {
                    await _alertService.ShowErrorAlert(@Localizer["an_error_occurred"], ex.Message);
                }
            });

        }
        if (_journeyplanviewmodel?.CustomerItemViewsToStore == null || _journeyplanviewmodel?.CustomerItemViewsToStore.Count == 0)
        {
            await ChecktheCustomerAndTrySync();
        }
        await Task.CompletedTask;
    }

    protected async Task ChecktheCustomerAndTrySync()
    {
        if (await CheckTodayBeatHistoryExistsAsync())
        {
            if (await _alertService.ShowConfirmationReturnType("alert ", "Today's customer list has changed. Please log in again to refresh your data."))
            {
                _navigationManager.NavigateTo("/");
            }
            else
            {
                return;
            }
        }
        else
        {
            // Show an alert or take appropriate action
            await _alertService.ShowErrorAlert(
            "No Customer Data",
            "No beat information is available for today. Please check your schedule or contact your administrator."
        );
        }
    }
    public async Task<bool> CheckTodayBeatHistoryExistsAsync()
    {
        var todayDate = DateTime.Today;

        // Step 1: Check in local SQLite database
        bool exists = await LocalDatabaseHasBeatHistoryForTodayAsync(todayDate);
        if (exists)
        {
            return true;
        }

        // Step 2: If not exists, call Sync method
        await SyncBeatHistoryAsync();

        // Step 3: After sync, again check in local database
        exists = await LocalDatabaseHasBeatHistoryForTodayAsync(todayDate);
        return exists;
    }

    // Helper method to check if record exists in local DB
    private async Task<bool> LocalDatabaseHasBeatHistoryForTodayAsync(DateTime todayDate)
    {
        // Sample pseudo query, replace with your database service
        string query = "SELECT COUNT(1) FROM beat_history WHERE visit_date = @TodayDate";

        var parameters = new { TodayDate = todayDate };

        int count = await _sqlite.ExecuteScalarAsync<int>(query, parameters);
        return count > 0;
    }

    // Your existing Sync method
    private async Task SyncBeatHistoryAsync()
    {
        _loadingService.ShowLoading("Syncing");
        try
        {
            await Task.Run(async () =>
            {
                await _mobileDataSyncBL.SyncDataForTableGroup("", "", _appUser.Emp.UID, _appUser.SelectedJobPosition.UID,
                    _appUser.SelectedJobPosition.UserRoleUID, _appUser?.Vehicle?.UID, _appUser.SelectedJobPosition.OrgUID, _appUser.Emp.Code);

                await InvokeAsync(async () =>
                {
                    _loadingService.HideLoading();
                    await _alertService.ShowErrorAlert(@Localizer["success"], @Localizer["syncing_completed_successfully"]);
                });
            });
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert(@Localizer["error"], ex.Message);
        }
        finally
        {
            _loadingService.HideLoading();
        }
    }

    private async Task UpdateVisitDate()
    {
        // Get the current date with the time set to 00:00:00
        string currentDate = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";

        // SQL query to update the visit_date
        string updateQuery = $"UPDATE beat_history SET visit_date = '{currentDate}'";

        try
        {
            // Execute the query using SQLite
            await _sqlite.ExecuteNonQueryAsync(updateQuery);

            // Optionally, log success or notify the user
            Console.WriteLine("Visit date updated successfully.");
        }
        catch (Exception ex)
        {
            // Handle exceptions
            Console.WriteLine($"Error updating visit date: {ex.Message}");
        }
    }

    private async Task LoadCustomers(string routeUID, string beatHistoryUID)
    {
        await _journeyplanviewmodel.GettheCustomersAndBindToProperty(routeUID, beatHistoryUID);
        Customerlist = _journeyplanviewmodel.CustomerItemViewsToStore;
        CustomerlisttoShow = Customerlist;
        //if (CustomersWithLatLon.Count == 0 && Customerlist.Count > 0) { CustomersWithLatLon = await GetCustomersLatlng(Customerlist); }
    }

    async Task SetTopBar()
    {
        WINITMobile.Models.TopBar.MainButtons buttons = new WINITMobile.Models.TopBar.MainButtons()
        {

            TopLabel = @Localizer["my_customers"],

        };
        await Btnname.InvokeAsync(buttons);
        await Task.CompletedTask;
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_journeyplanviewmodel.IsInitialized)
        {
            _journeyplanviewmodel.IsInitialized = true;
            // Load data if needed
        }

        if (firstRender)
        {
            try
            {
                //if (CustomersWithLatLon.Count>0)
                //{
                //string serializedCustomers = JsonConvert.SerializeObject(CustomersWithLatLon);
                //await _jSRuntime.InvokeVoidAsync("initialize", serializedCustomers);

                //}
            }
            catch (Exception ex)
            {

            }
        }
        await Task.CompletedTask;
    }
    //user can Search using name,code and Address
    private async Task OnSearching(string searching)
    {
        if (!string.IsNullOrEmpty(searching))
        {
            searching = searching.ToLower();

            CustomerlisttoShow = Customerlist.Where(c =>
                (c.Name?.ToLower() ?? "").Contains(searching) ||
                (c.Code?.ToLower() ?? "").Contains(searching) ||
                (c.Address?.ToLower() ?? "").Contains(searching)
            ).ToList();
        }
        else
        {
            CustomerlisttoShow = Customerlist;
        }

        StateHasChanged();
        await Task.CompletedTask;
    }


    protected async Task<List<WINITMobile.State.CustomerLatlng>> GetCustomersLatlng(List<Winit.Modules.Store.Model.Interfaces.IStoreItemView> Customerlist)
    {
        return Customerlist
            .Where(customer => !string.IsNullOrEmpty(customer.Latitude) && !string.IsNullOrEmpty(customer.Longitude))
            .Select(customer =>
            {
                bool isLatValid = decimal.TryParse(customer.Latitude, out decimal latitude);
                bool isLonValid = decimal.TryParse(customer.Longitude, out decimal longitude);

                if (isLatValid && isLonValid)
                {
                    return new WINITMobile.State.CustomerLatlng
                    {
                        Code = customer.Code,
                        Name = customer.Name,
                        Description = customer.Address,
                        Latitude = latitude,
                        Longitude = longitude
                    };
                }
                return null;
            })
            .Where(customerLatlng => customerLatlng != null)
            .ToList();
    }
    private async Task ValidadateCustomer(IStoreItemView storeItemView)
    {
        // Check IsActive
        //if (!storeItemView.IsActive)
        //{
        //    await _alertService.ShowErrorAlert("", "The selected customer is not active.");
        //    return;
        //}

        // Check IsBlocked
        if (storeItemView.IsBlocked)
        {
            await _alertService.ShowErrorAlert("", $"{@Localizer["the_selected_customer_is_blocked_with_comment"]}: {storeItemView.BlockedReasonDescription}.");
            return;
        }
        await Task.CompletedTask;
    }

    private async Task CheckCustomerAndAddToJp()
    {
        if (_journeyplanviewmodel != null)
        {
            if (_journeyplanviewmodel is Winit.Modules.JourneyPlan.BL.Classes.MyCustomerViewModel myCustomerViewModel)
            {
                // Now you can access the CheckCustomerExistsInJourneyPlan method
                if (_journeyplanviewmodel.SelectedCustomer != null && _journeyplanviewmodel.SelectedBeatHistory != null)
                {
                    int res = await myCustomerViewModel.CheckCustomerExistsInJourneyPlan(_journeyplanviewmodel.SelectedCustomer.StoreUID, _journeyplanviewmodel.SelectedBeatHistory.UID);
                    if (res == 0)
                    {
                        string GUIDForStoreHistory = Guid.NewGuid().ToString();
                        int result = await myCustomerViewModel.AddCustomerInJourneyPlan(GUIDForStoreHistory, _journeyplanviewmodel.SelectedCustomer.StoreUID, _journeyplanviewmodel.SelectedBeatHistory.UID);
                        if (result >= 1)
                        {
                            _journeyplanviewmodel.SelectedCustomer.StoreHistoryUID = GUIDForStoreHistory;
                            _journeyplanviewmodel.SelectedCustomer.IsAddedInJP = true;
                            _journeyplanviewmodel.SelectedCustomer.SerialNo = 9999;
                            _journeyplanviewmodel.SelectedCustomer.IsPlanned = false;

                        }

                    }
                }
                else
                {
                    // Handle the case where SelectedCustomer or SelectedBeatHistory is null
                }
            }
            else
            {
                // Handle the case where _journeyplanviewmodel is not an instance of MyCustomerViewModel
            }
        }
        else
        {
            // Handle the case where _journeyplanviewmodel is null
        }
        await Task.CompletedTask;
    }
    //public async Task CheckAndBindOrgUID(IStoreItemView storeItemView)
    //{

    //    // for validation
    //    await ValidadateCustomer(storeItemView);


    //    storeMaster = _journeyplanviewmodel.StoreMasterDataForCustomers.Find(e => e.Store != null && e.Store.UID == storeItemView.StoreUID);

    //    //_journeyplanviewmodel.DefaultAddress = (Winit.Modules.Address.Model.Interfaces.IAddress)storeMaster.Addresses.Select(e => e.IsDefault == true);

    //    // _journeyplanviewmodel.DefaultAddress = storeMaster.Addresses.FirstOrDefault(e => e.IsDefault == true);

    //    if (storeMaster.storeCredits != null && storeMaster.storeCredits.Any())
    //    {
    //        int count = storeMaster.storeCredits.Where(e => e.IsActive).Count();
    //        if (count == 1)
    //        {
    //            // storeItemView.SelectedOrgUID = storeMaster.storeCredits.FirstOrDefault().OrgUID;

    //            // here set to  SetSelectedCustomer to view model
    //            _journeyplanviewmodel.SelectedCustomer = storeItemView;


    //            // here set to viewmodel SelectedStoreCredit to Selected Customer
    //            _journeyplanviewmodel.SelectedCustomer.SelectedStoreCredit = storeMaster.storeCredits.FirstOrDefault();

    //            // _journeyplanviewmodel.SelectedCustomer.AvailableCreditLimit = storeMaster.storeCredits.FirstOrDefault().TemporaryCredit;
    //            // Here we set the OrgHierarchy
    //            await _journeyplanviewmodel.SetOrgHierarchy();
    //            //_dataManager.SetData("SelectedStoreViewModel", storeItemView);

    //            //_navigationManager.NavigateTo("/Customertemview");
    //            await CheckCustomerAndAddToJp();
    //            NavigateTo("Customertemview");

    //        }
    //        else if (count > 1)
    //        {
    //            _journeyplanviewmodel.SelectedCustomer = storeItemView;

    //            //List<Winit.Modules.Store.Model.Interfaces.IStoreCredit> storeCreditLIST =

    //            List<ISelectionItem> storeCreditSelectionItems = ConvertToSelectionItems<Winit.Modules.Store.Model.Interfaces.IStoreCredit>(storeMaster.storeCredits);
    //            await _dropdownService.ShowMobilePopUpDropDown(new DropDownOptions
    //            {
    //                DataSource = storeCreditSelectionItems,
    //                OnSelect = async (eventArgs) =>
    //                {
    //                    await OnDDOptionSelect(eventArgs);
    //                },
    //                OkBtnTxt = "Select",
    //                Title = "Orgs"
    //            });

    //        }
    //        else
    //        {
    //            // impletement later
    //        }
    //    }
    //    await Task.CompletedTask;
    //}


    public async Task CheckAndBindOrgUID(IStoreItemView storeItemView)
    {
        _loadingService.ShowLoading(@Localizer["loading...."]);
        await Task.Run(async () =>
        {
            try
            {
                // for validation
                await ValidadateCustomer(storeItemView);


                storeMaster = _journeyplanviewmodel.StoreMasterDataForCustomers.Find(e => e.Store != null && e.Store.UID == storeItemView.StoreUID);

                //_journeyplanviewmodel.DefaultAddress = (Winit.Modules.Address.Model.Interfaces.IAddress)storeMaster.Addresses.Select(e => e.IsDefault == true);

                // _journeyplanviewmodel.DefaultAddress = storeMaster.Addresses.FirstOrDefault(e => e.IsDefault == true);
                _journeyplanviewmodel.SelectedCustomer = storeItemView;
                if (storeMaster.storeCredits != null && storeMaster.storeCredits.Any())
                {
                    int count = storeMaster.storeCredits.Where(e => e.IsActive).Count();
                    if (count == 1)
                    {
                        // storeItemView.SelectedOrgUID = storeMaster.storeCredits.FirstOrDefault().OrgUID;

                        // here set to  SetSelectedCustomer to view model


                        // here set to viewmodel SelectedStoreCredit to Selected Customer
                        _journeyplanviewmodel.SelectedCustomer.SelectedStoreCredit = storeMaster.storeCredits.FirstOrDefault();
                        _journeyplanviewmodel.SelectedCustomer.SelectedOrgUID = _journeyplanviewmodel.SelectedCustomer.SelectedStoreCredit.OrgUID;
                        _journeyplanviewmodel.SelectedCustomer.SelectedDistributionChannelUID = _journeyplanviewmodel.SelectedCustomer.SelectedStoreCredit.DistributionChannelUID;
                        Winit.Modules.Store.Model.Interfaces.IStoreAttributes storeChannel = storeMaster.storeAttributes?.Where(e => e.Name == "Channel").FirstOrDefault();
                        if(storeChannel != null)
                        {
                            _journeyplanviewmodel.SelectedCustomer.ChannelCode = storeChannel.Code;
                            _journeyplanviewmodel.SelectedCustomer.ChannelName = storeChannel.Value;
                        }

                        //_journeyplanviewmodel.SelectedCustomer.AvailableCreditLimit = storeMaster.storeCredits.FirstOrDefault().TemporaryCredit;
                        // Here we set the OrgHierarchy
                        await _journeyplanviewmodel.SetOrgHierarchy();
                        //_dataManager.SetData("SelectedStoreViewModel", storeItemView);
                        //_journeyplanviewmodel.SelectedCustomer.AllowedSKUs = await _journeyplanviewmodel.GetAllowedSKUByStoreUID(storeItemView.StoreUID);

                        Dictionary<string, List<string>> storeLinkedItemUIDs = await _selectionMapCriteriaBL.GetLinkedItemUIDByStore(LinkedItemType.SKUClassGroup, new List<string>() { _journeyplanviewmodel.SelectedCustomer.StoreUID });
                        if (storeLinkedItemUIDs != null && storeLinkedItemUIDs.Count >= 0)
                        {
                            string skuClassGroupUID = storeLinkedItemUIDs.Values.FirstOrDefault()?.FirstOrDefault();
                            _journeyplanviewmodel.SelectedCustomer.AllowedSKUs = await _sKUClassGroupItemsBL.GetApplicableAllowedSKUGBySKUClassGroupUID(skuClassGroupUID);
                            
                        }
                        
                        _dataManager.SetData(nameof(Winit.Modules.Store.Model.Interfaces.IStoreItemView), _journeyplanviewmodel.SelectedCustomer);
                        await CheckCustomerAndAddToJp();
                        _navigationManager.NavigateTo("/Customertemview");
                        // NavigateTo("Customertemview");

                    }
                    else if (count > 1)
                    {
                        _journeyplanviewmodel.SelectedCustomer.StoreCredits = storeMaster.storeCredits;
                        List<Winit.Modules.Store.Model.Interfaces.IStoreCredit> storeCreditLIST = storeMaster.storeCredits;

                        SalesOrganizationPop = true;
                        IsOpnedScoreCredit = true;

                    }
                    else
                    {
                        // impletement later
                    }
                }
                await InvokeAsync(async () =>
                {
                    _loadingService.HideLoading();
                    StateHasChanged(); // Ensure UI reflects changes
                });
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert(@Localizer["an_error_occurred"], ex.Message);
            }
        });
        await Task.CompletedTask;
    }
    private void HandlePopupVisibilityChange(bool IsOpned)
    {
        IsOpnedScoreCredit = IsOpned;
    }
    protected async Task HandleStoreCreditSelected(Winit.Modules.Store.Model.Interfaces.IStoreCredit StoreCredit)
    {
        _journeyplanviewmodel.SelectedCustomer.SelectedStoreCredit = StoreCredit;

        //_journeyplanviewmodel.SelectedCustomer.AvailableCreditLimit = iStoreCredit.TemporaryCredit;
        _journeyplanviewmodel.SelectedCustomer.SelectedOrgUID = StoreCredit.OrgUID;
        _journeyplanviewmodel.SelectedCustomer.SelectedDistributionChannelUID = StoreCredit.DistributionChannelUID;
        _dataManager.SetData(nameof(Winit.Modules.Store.Model.Interfaces.IStoreItemView), _journeyplanviewmodel.SelectedCustomer);
        _navigationManager.NavigateTo("/Customertemview");
        // NavigateTo("Customertemview");
        await Task.CompletedTask;
    }
    public List<ISelectionItem> ConvertToSelectionItems<T>(List<T>? items) where T : class
    {
        List<ISelectionItem> selectionItems = new List<ISelectionItem>();
        foreach (var item in items)
        {
            ISelectionItem selectionItem = new SelectionItem();
            selectionItem.Code = GetPropertyValue(item, "OrgUID") as string;
            selectionItem.UID = GetPropertyValue(item, "UID") as string;
            selectionItem.Label = GetPropertyValue(item, "DCLabel") as string;
            selectionItems.Add(selectionItem);
        }
        return selectionItems;
    }

    private object? GetPropertyValue(object obj, string propertyName)
    {
        return obj?.GetType().GetProperty(propertyName)?.GetValue(obj, null);
    }

    public async Task OnDDOptionSelect(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        // here set to viewmodel SelectedStoreCredit to Selected Customer
        //_journeyplanviewmodel.SelectedCustomer.SelectedStoreCredit = dropDownEvent.storeCredits.FirstOrDefault();
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null)
        {
            ISelectionItem selectionItem = dropDownEvent.SelectionItems.FirstOrDefault();
            IStoreCredit iStoreCredit = storeMaster.storeCredits.Find(x => x.UID == selectionItem.UID);
            // here set to viewmodel SelectedStoreCredit to Selected Customer
            _journeyplanviewmodel.SelectedCustomer.SelectedStoreCredit = iStoreCredit;

            //_journeyplanviewmodel.SelectedCustomer.AvailableCreditLimit = iStoreCredit.TemporaryCredit;
            _journeyplanviewmodel.SelectedCustomer.SelectedOrgUID = dropDownEvent.SelectionItems.FirstOrDefault().Label;
            //_dataManager.SetData("SelectedStoreViewModel", _journeyplanviewmodel.SelectedCustomer);
            await CheckCustomerAndAddToJp();
            _navigationManager.NavigateTo("/Customertemview");
            //NavigateTo("Customertemview");
        }
        await Task.CompletedTask;
    }

    private async Task OnRouteDDClick()
    {
        return;
        var routeSelectionItems = CommonFunctions.ConvertToSelectionItems<IRoute>(_appUser.JPRoutes, new List<string>
        { "UID","Code","Name"});
        if (_appUser.SelectedRoute != null)
        {
            routeSelectionItems.Find(e => e.UID == _appUser.SelectedRoute.UID).IsSelected = true;
        }
        await _dropdownService.ShowMobilePopUpDropDown(new DropDownOptions
        {
            DataSource = routeSelectionItems,
            OnSelect = async (eventArgs) =>
            {
                await HandelRouteSelect(eventArgs);
            },
            OkBtnTxt = @Localizer["proceed"],
            Title = @Localizer["route_selection"]
        });
    }
    private async Task HandelRouteSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            ISelectionItem beatHistorySelectionItem = dropDownEvent.SelectionItems.First();
            await LoadCustomers(beatHistorySelectionItem.UID, _appUser.SelectedBeatHistory.UID);
            _journeyplanviewmodel.SelectedRoute = _appUser.JPRoutes.Find(e => e.UID == beatHistorySelectionItem.UID);
            _appUser.SelectedRoute = _appUser.JPRoutes.Find(e => e.UID == beatHistorySelectionItem.UID);
            StateHasChanged();
        }
    }
    public override async Task OnBackClick()
    {
        _backbuttonhandler.ClearCurrentPage();
        _navigationManager.NavigateTo("DashBoard");
        await Task.CompletedTask;
    }
}