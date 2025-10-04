
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.JourneyPlan.BL.Classes;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.Promotion.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.CustomControles;
using Winit.UIComponents.Common.Services;
using WinIt.Models.Customers;
using WINITMobile.Pages.Base;
using WINITMobile.State;

namespace WINITMobile.Pages.JourneyPlan;

partial class JourneyPlan
{
    [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }
    public List<Winit.Modules.Store.Model.Interfaces.IStoreItemView> Customerlist { get; set; }
    public List<Winit.Modules.Store.Model.Interfaces.IStoreItemView> CustomerlisttoShow { get; set; }
    public List<Winit.Modules.Store.Model.Interfaces.IStoreItemView> UnplanedCustomers { get; set; }
    public Winit.Modules.JourneyPlan.BL.Interfaces.IJourneyPlanViewModel _journeyplanviewmodel { get; set; }
    public List<WINITMobile.State.CustomerLatlng> CustomersWithLatLon { get; set; } = new List<WINITMobile.State.CustomerLatlng>();
    public IStoreMaster storeMaster { get; set; }
    private string searchString { get; set; }
    private Tabs TabsRefference;
    public ISelectionItem SelectedTab { get; set; }
    private bool showDates = false;
    private List<DateTime> DateRange { get; set; } = new List<DateTime>();
    public DateTime SelectedDate { get; set; }
    private bool ShowMap { get; set; }
    public bool IsUnplanned { get; set; }
    private bool ShowMaps { get; set; }
    private bool showCustomerListpopup { get; set; }
    private bool SalesOrganizationPop { get; set; }
    private bool IsVisible { get; set; }
    private bool IsOpnedScoreCredit { get; set; }
    private bool IsCustomersView { get; set; }
    private bool IsComponentRendered { get; set; }
    private double TotalCustomers { get; set; }
    private int VisitedCustomers { get; set; }
    private double VisitPercentage { get; set; }
    private async Task ShowMapModal()
    {
        // Call JavaScript function to initialize the map

        // ShowMap = true;
        IsCustomersView = !IsCustomersView;
        await InitializeMap();
        StateHasChanged();
        //  await _jSRuntime.InvokeVoidAsync("initialize");
        //string serializedCustomers = JsonConvert.SerializeObject(CustomersWithLatLon);
        // await _jSRuntime.InvokeVoidAsync("initialize", serializedCustomers);
    }
    private async Task ShowMapForJp()
    {
        ShowMaps = true;
        await InitializeMap();
        StateHasChanged();
        await Task.CompletedTask;
    }
    private async Task ShowCustomers()
    {
        IsCustomersView = !IsCustomersView;
        StateHasChanged();
        await Task.CompletedTask;
    }
    private void HideMapModal()
    {
        ShowMap = false;
    }
    private void HideMap()
    {
        ShowMaps = false;
    }
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

    public List<ISelectionItem> tabItems = new List<ISelectionItem>
    {
        new SelectionItemTab {  Label="All", Code="All", UID="1", IsSelected=true },
        new SelectionItemTab {  Label="Pending", Code="", UID="2"},
        new SelectionItemTab {  Label="Visited", Code="Visited", UID="3" },
        new SelectionItemTab {  Label="Skipped", Code="Skipped", UID="4"},
        new SelectionItemTab {  Label="Zero Sales", Code="Zero Sales", UID="6"},
        new SelectionItemTab {  Label="Planned", Code="Planned", UID="7"},
        new SelectionItemTab {  Label="UnPlanned", Code="UnPlanned", UID="8"},
    };

    protected override async void OnInitialized()
    {
        try
        {
            IsCustomersView = true;
            _journeyplanviewmodel = _viewmodel.CreateJourneyPlanViewModel(Winit.Shared.Models.Constants.ScreenType.JourneyPlan);
            // _journeyplanviewmodel.SelectedJobPosition = _appUser.SelectedJobPosition;
            //await SetJourneyPlanViewModel(Winit.Shared.Models.Constants.ScreenType.JourneyPlan);
            _journeyplanviewmodel.UserJourney = _appUser.UserJourney;
            _journeyplanviewmodel.SelectedRoute = _appUser.SelectedRoute;
            _journeyplanviewmodel.SelectedBeatHistory = _appUser.SelectedBeatHistory;
            _journeyplanviewmodel.ScreenType = Winit.Shared.Models.Constants.ScreenType.JourneyPlan;

            await SetTopBar();

            if (_journeyplanviewmodel.CustomerItemViewsToStore != null)
            {
                Customerlist = _journeyplanviewmodel.CustomerItemViewsToStore;

                SelectedTab = tabItems.FirstOrDefault(item => item.IsSelected);
                await TabFilter();
                //CustomerlisttoShow = Customerlist;
                //Winit.Shared.Models.Constants.StoryHistoryStatus.SKIPPED

                // this for map 
                if (CustomersWithLatLon.Count == 0) { CustomersWithLatLon = await GetCustomersLatlng(Customerlist); }
            }
            // this for date 
            if (_journeyplanviewmodel?.SelectedBeatHistory?.VisitDate != null)
            {
                SelectedDate = _journeyplanviewmodel.SelectedBeatHistory.VisitDate;
                DateRange.Add(SelectedDate);
                DateRange = Enumerable.Range(0, 6).Select(offset => SelectedDate.AddDays(offset)).ToList();
            }
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
        if (_journeyplanviewmodel?.SelectedRoute == null)
        {
            // Show the error alert
            await _alertService.ShowErrorAlert("Error", "There is no selected route. Please select the route to continue.");

            // Navigate to the dashboard page
            _navigationManager.NavigateTo("/DashBoard");

            return;
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
                        await _journeyplanviewmodel.GettheCustomersAndBindToProperty(_appUser.SelectedBeatHistory.UID);
                        await _journeyplanviewmodel.PopulateViewModel();
                        // IEnumerable<IStoreItemView> SelectedCustomers = await GetCustomersForViewModel(UID);
                        Customerlist = _journeyplanviewmodel.CustomerItemViewsToStore;
                        if (CustomersWithLatLon.Count == 0) { CustomersWithLatLon = await GetCustomersLatlng(Customerlist); }

                        SelectedTab = tabItems.FirstOrDefault(item => item.IsSelected);
                        await TabFilter();
                        await GetVisitPercentageData();
                        //CustomerlisttoShow = Customerlist;
                    }
                    //await GetUnplannedCustomers();
                    await GetUnplannedCustomers(_appUser.SelectedRoute?.UID);
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

        }
        await Task.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                await InitializeMap();
            }
            catch (Exception ex)
            {
                // implement later getting error sometimes beacuase map div Not initized.
                //fix later
            }
        }

        if (firstRender && !_journeyplanviewmodel.IsInitialized)
        {
            _journeyplanviewmodel.IsInitialized = true;
            // Load data if needed
        }
        await Task.CompletedTask;
    }
    private async Task OnRouteChangedHandler(string routeUID)
    {
        // Update the unplanned customers based on the new routeUID
        await GetUnplannedCustomers(routeUID);
        StateHasChanged();
    }
    protected async Task GetVisitPercentageData()
    {
        TotalCustomers = Customerlist
                        .Count;
        VisitedCustomers = Customerlist
                        .Count(e => e.IsPlanned && string.Equals(e.CurrentVisitStatus, StoryHistoryStatus.VISITED, StringComparison.OrdinalIgnoreCase));

        if (TotalCustomers > 0)
        {
            VisitPercentage = (VisitedCustomers / TotalCustomers) * 100;
        }
        else
        {
            VisitPercentage = 0;
        }
        await Task.CompletedTask;
    }
    protected async Task InitializeMap()
    {
        await Task.Delay(100);
        //var mapElement = await _jSRuntime.InvokeAsync<IJSObjectReference>("document.getElementById", new object?[] { "map" });

        //if (mapElement == null)
        //{
        //    return;
        //}

        List<IStoreItemView> customersToShow;

        switch (SelectedTab?.Label)
        {
            case StoryHistoryStatus.VISITED:
                customersToShow = Customerlist.Where(c => c.CurrentVisitStatus == StoryHistoryStatus.VISITED).ToList();
                break;
            case StoryHistoryStatus.PENDING:
                customersToShow = Customerlist.Where(c => c.CurrentVisitStatus == StoryHistoryStatus.PENDING).ToList();
                break;
            case StoryHistoryStatus.SKIPPED:
                customersToShow = Customerlist.Where(c => c.CurrentVisitStatus == StoryHistoryStatus.SKIPPED).ToList();
                break;
            case StoryHistoryStatus.ZERO_SALES:
                customersToShow = Customerlist.Where(c => c.CurrentVisitStatus == StoryHistoryStatus.ZERO_SALES).ToList();
                break;
            case StoryHistoryStatus.PLANNED:
                customersToShow = Customerlist.Where(c => c.IsPlanned).ToList();
                break;
            case StoryHistoryStatus.UNPLANNED:
                customersToShow = Customerlist.Where(c => c.CurrentVisitStatus == StoryHistoryStatus.UNPLANNED).ToList();
                break;
            case StoryHistoryStatus.ALL:
            default:
                customersToShow = Customerlist;
                break;
        }
        if (customersToShow == null || customersToShow.Count == 0)
        {
            return;
        }

        List<WINITMobile.State.CustomerLatlng> customersWithLatLng = customersToShow
                .Where(c => !string.IsNullOrEmpty(c.Latitude) && !string.IsNullOrEmpty(c.Longitude))
                .Select(c =>
                        {
                            bool isLatValid = decimal.TryParse(c.Latitude, out decimal latitude);
                            bool isLonValid = decimal.TryParse(c.Longitude, out decimal longitude);

                            if (isLatValid && isLonValid)
                            {
                                return new WINITMobile.State.CustomerLatlng
                                {
                                    Code = c.Code,
                                    Name = c.Name,
                                    Description = c.Address,
                                    Latitude = latitude,
                                    Longitude = longitude
                                };
                            }
                            return null;
                        })
                .Where(customerLatlng => customerLatlng != null)
                .ToList();

        if(customersWithLatLng != null && customersWithLatLng.Any())
        {
        string serializedCustomers = JsonConvert.SerializeObject(customersWithLatLng);
        await _jSRuntime.InvokeVoidAsync("initialize", serializedCustomers);

        }
        else
        {
            return;
        }

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

    public async Task OnTabSelect(ISelectionItem selectionItemTab)
    {
        if (Customerlist == null)
        {
            // Handle null Customerlist
            return;
        }

        SelectedTab = selectionItemTab;

        await TabFilter();
        foreach (var tabitem in tabItems)
        {
            tabitem.IsSelected = (tabitem == selectionItemTab);
        }
        StateHasChanged();
        await Task.CompletedTask;
    }

    public async Task SetTabCount()
    {
        foreach (var tab in tabItems)
        {
            switch (tab.Label)
            {
                case StoryHistoryStatus.PLANNED:
                    ((SelectionItemTab)tab).Count = Customerlist
                        .Count(e => e.IsPlanned);
                    break;
                case StoryHistoryStatus.PENDING:
                    ((SelectionItemTab)tab).Count = Customerlist
                        .Count(e => e.IsPlanned && string.Equals(e.CurrentVisitStatus, StoryHistoryStatus.PENDING, StringComparison.OrdinalIgnoreCase));
                    break;
                case StoryHistoryStatus.VISITED:
                    ((SelectionItemTab)tab).Count = Customerlist
                        .Count(e => e.IsPlanned && string.Equals(e.CurrentVisitStatus, StoryHistoryStatus.VISITED, StringComparison.OrdinalIgnoreCase));
                    break;
                case StoryHistoryStatus.SKIPPED:
                    ((SelectionItemTab)tab).Count = Customerlist
                        .Count(e => e.IsPlanned && string.Equals(e.CurrentVisitStatus, StoryHistoryStatus.SKIPPED, StringComparison.OrdinalIgnoreCase));
                    break;
                case StoryHistoryStatus.ZERO_SALES:
                    ((SelectionItemTab)tab).Count = Customerlist
                        .Count(e => e.IsPlanned && string.Equals(e.CurrentVisitStatus, StoryHistoryStatus.ZERO_SALES, StringComparison.OrdinalIgnoreCase) && e.ActualLines == 0);
                    break;
                case StoryHistoryStatus.UNPLANNED:
                    ((SelectionItemTab)tab).Count = Customerlist
                        .Count(e => !e.IsPlanned && string.Equals(e.CurrentVisitStatus, StoryHistoryStatus.UNPLANNED, StringComparison.OrdinalIgnoreCase));
                    break;
                case StoryHistoryStatus.ALL:
                    ((SelectionItemTab)tab).Count = Customerlist.Count;
                    break;
                default:
                    ((SelectionItemTab)tab).Count = 0;
                    break;
            }
        }
        await Task.CompletedTask;
    }

    public async Task TabFilter()
    {
        await SetTabCount();

        string selectedTabLabel = SelectedTab.Label;

        switch (selectedTabLabel)
        {
            case StoryHistoryStatus.PLANNED:
                CustomerlisttoShow = Customerlist
                    .Where(e => e.IsPlanned)
                    .OrderBy(e => e.SerialNo).ThenBy(e => e.Name)
                    .ToList<Winit.Modules.Store.Model.Interfaces.IStoreItemView>();
                break;
            case StoryHistoryStatus.PENDING:
                CustomerlisttoShow = Customerlist
                    .Where(e => e.IsPlanned && string.Equals(e.CurrentVisitStatus, StoryHistoryStatus.PENDING, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(e => e.SerialNo).ThenBy(e => e.Name)
                    .ToList<Winit.Modules.Store.Model.Interfaces.IStoreItemView>();
                break;
            case StoryHistoryStatus.VISITED:
                CustomerlisttoShow = Customerlist
                    .Where(e => e.IsPlanned && string.Equals(e.CurrentVisitStatus, StoryHistoryStatus.VISITED, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(e => e.SerialNo).ThenBy(e => e.Name)
                    .ToList<Winit.Modules.Store.Model.Interfaces.IStoreItemView>();
                break;
            case StoryHistoryStatus.SKIPPED:
                CustomerlisttoShow = Customerlist
                    .Where(e => e.IsPlanned && string.Equals(e.CurrentVisitStatus, StoryHistoryStatus.SKIPPED, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(e => e.SerialNo).ThenBy(e => e.Name)
                    .ToList<Winit.Modules.Store.Model.Interfaces.IStoreItemView>();
                break;
            case StoryHistoryStatus.ZERO_SALES:
                CustomerlisttoShow = Customerlist
                    .Where(e => e.IsPlanned && string.Equals(e.CurrentVisitStatus, StoryHistoryStatus.ZERO_SALES, StringComparison.OrdinalIgnoreCase) && e.ActualLines == 0)
                    .OrderBy(e => e.SerialNo).ThenBy(e => e.Name)
                    .ToList<Winit.Modules.Store.Model.Interfaces.IStoreItemView>();
                break;
            case StoryHistoryStatus.UNPLANNED:
                CustomerlisttoShow = Customerlist
                    .Where(e => !e.IsPlanned && string.Equals(e.CurrentVisitStatus, StoryHistoryStatus.UNPLANNED, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(e => e.SerialNo).ThenBy(e => e.Name)
                    .ToList<Winit.Modules.Store.Model.Interfaces.IStoreItemView>();
                break;
            case StoryHistoryStatus.ALL:
                CustomerlisttoShow = Customerlist;
                break;
            default:
                CustomerlisttoShow = Customerlist
                    .Where(e => string.Equals(e.CurrentVisitStatus, StoryHistoryStatus.PENDING, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(e => e.SerialNo).ThenBy(e => e.Name)
                    .ToList<Winit.Modules.Store.Model.Interfaces.IStoreItemView>();
                break;
        }
        await Task.CompletedTask;
    }

    public async Task SetTopBar()
    {
        WINITMobile.Models.TopBar.MainButtons buttons = new WINITMobile.Models.TopBar.MainButtons()
        {
            TopLabel = @Localizer["journey_plan(jp)"],
            UIButton1 = new Models.TopBar.Buttons
            {
                Action = async () => await ShowDates(),
                ButtonText = @Localizer["date"],
                ButtonType = Models.TopBar.ButtonType.Text,
                IsVisible = true

            },

        };
        await Btnname.InvokeAsync(buttons);
        await Task.CompletedTask;
    }

    // here this for dates 
    protected async Task ShowDates()
    {
        showDates = !showDates;
        await Task.CompletedTask;
    }

    private async Task SelectDate(DateTime selectedDate)
    {
        await UpdateCustomers(selectedDate);
        showDates = false;
        await Task.CompletedTask;
        // Winit.Shared.CommonUtilities.Common.CommonFunctions.GetDateTimeInFormat(selectedDate);
    }

    private async Task UpdateCustomers(DateTime selectedDate)
    {
        if (_journeyplanviewmodel != null)
        {
            if (_journeyplanviewmodel is Winit.Modules.JourneyPlan.BL.Classes.JourneyPlanViewModel journeyplanviewmodel)
            {
                IEnumerable<IStoreItemView> SelectedCustomers = await journeyplanviewmodel.GetCustomersForSelectedDate(selectedDate);
                CustomerlisttoShow = SelectedCustomers.ToList();
                _journeyplanviewmodel.IsCurrentDayJP = false;
                if (_journeyplanviewmodel.SelectedBeatHistory.VisitDate == selectedDate) { _journeyplanviewmodel.IsCurrentDayJP = true; }
                SelectedDate = selectedDate;
                StateHasChanged();
            }
        }
        await Task.CompletedTask;
    }

    public async Task Handel_VisitedClick(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeItemView)
    {
        storeItemView.CurrentVisitStatus = "Visited";
        ((SelectionItemTab)tabItems[1]).Count = Customerlist.Count(item => item.CurrentVisitStatus == "Visited");
        //await InvokeAsync(() => StateHasChanged());

        await InvokeAsync(async () =>
        {
            TabsRefference.Refresh();

        });
    }

    public async Task Handel_Skiped(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeItemView)
    {
        storeItemView.CurrentVisitStatus = "Skipped";
        ((SelectionItemTab)tabItems[2]).Count = Customerlist.Count(item => item.CurrentVisitStatus == "Skipped");
        await InvokeAsync(async () =>
        {
            TabsRefference.Refresh();

        });
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
            //await _alertService.ShowErrorAlert("", $"The selected customer is blocked with comment: {storeItemView.BlockedReasonDescription}.");
            await _alertService.ShowErrorAlert("", string.Format(Localizer["the_selected_customer_is_blocked_with_comment"], storeItemView.BlockedReasonDescription));

            return;
        }
        await Task.CompletedTask;
    }
    public async Task Handel_CardClick(IStoreItemView storeItemView)
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
                    await SetPromotion();
                    int count = storeMaster.storeCredits.Where(e => e.IsActive).Count();
                    if (count == 1)
                    {
                        // storeItemView.SelectedOrgUID = storeMaster.storeCredits.FirstOrDefault().OrgUID;

                        // here set to  SetSelectedCustomer to view model


                        // here set to viewmodel SelectedStoreCredit to Selected Customer
                        _journeyplanviewmodel.SelectedCustomer.SelectedStoreCredit = storeMaster.storeCredits.FirstOrDefault();
                        _journeyplanviewmodel.SelectedCustomer.SelectedOrgUID = _journeyplanviewmodel.SelectedCustomer.SelectedStoreCredit.OrgUID;
                        _journeyplanviewmodel.SelectedCustomer.SelectedDistributionChannelUID = _journeyplanviewmodel.SelectedCustomer.SelectedStoreCredit.DistributionChannelUID;

                        //_journeyplanviewmodel.SelectedCustomer.AvailableCreditLimit = storeMaster.storeCredits.FirstOrDefault().TemporaryCredit;
                        // Here we set the OrgHierarchy
                        await _journeyplanviewmodel.SetOrgHierarchy();
                        //_dataManager.SetData("SelectedStoreViewModel", storeItemView);
                        _dataManager.SetData(nameof(Winit.Modules.Store.Model.Interfaces.IStoreItemView), _journeyplanviewmodel.SelectedCustomer);
                        _navigationManager.NavigateTo("Customertemview");
                        //NavigateTo("Customertemview");

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
                        await _alertService.ShowErrorAlert(@Localizer["an_error_occurred"], @Localizer["there_is_no_active_store"]);
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
    private async Task SetPromotion()
    {
        if (_journeyplanviewmodel.SelectedCustomer != null && !_journeyplanviewmodel.SelectedCustomer.IsPromotionsBlock)
        {
            /*
            if (_appUser.StorePromotionMapDictionary != null &&
                _appUser.StorePromotionMapDictionary.ContainsKey(_journeyplanviewmodel.SelectedCustomer.UID))
            {
            _journeyplanviewmodel.SelectedCustomer.ApplicablePromotionList = _appUser.StorePromotionMapDictionary[_journeyplanviewmodel.SelectedCustomer.UID];
                */
            _journeyplanviewmodel.SelectedCustomer.ApplicablePromotionList = _appUser.ApplicablePromotionUIDs;

            if (_journeyplanviewmodel.SelectedCustomer.ApplicablePromotionList == null ||
                _journeyplanviewmodel.SelectedCustomer.ApplicablePromotionList.Count == 0)
            {
                return;
            }

            _journeyplanviewmodel.SelectedCustomer.DMSPromotionDictionary = new Dictionary<string, Winit.Modules.Promotion.Model.Classes.DmsPromotion>();
            foreach (string promotionUID in _journeyplanviewmodel.SelectedCustomer.ApplicablePromotionList)
            {
                if (_appUser.DMSPromotionDictionary != null &&
                    _appUser.DMSPromotionDictionary.ContainsKey(promotionUID))
                {
                    _journeyplanviewmodel.SelectedCustomer.DMSPromotionDictionary[promotionUID] = _appUser.DMSPromotionDictionary[promotionUID];
                }
            }
            _journeyplanviewmodel.SelectedCustomer.ItemPromotionMapList = (List<IItemPromotionMap>)await _promotionBL.SelectItemPromotionMapByPromotionUIDs(_journeyplanviewmodel.SelectedCustomer.ApplicablePromotionList);
            /*
            }
            */
        }
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

    // this is for handle unplanned customers 
    protected async Task GetUnplannedCustomers(string routeUID)
    {
        if (_journeyplanviewmodel != null)
        {
            if (_journeyplanviewmodel is Winit.Modules.JourneyPlan.BL.Classes.JourneyPlanViewModel journeyPlanViewModel)
            {
                if (!string.IsNullOrEmpty(routeUID))
                {

                    IEnumerable<IStoreItemView> Customers = await journeyPlanViewModel.GetCustomersUnplanned(routeUID, _appUser?.SelectedBeatHistory?.UID,true);
                    UnplanedCustomers = Customers.ToList();
                }
            }
        }
    }
    protected async Task OpenUnplannedCustomers()
    {
        showCustomerListpopup = true;
        IsVisible = true;
        await Task.CompletedTask;
    }
    protected async Task HandleSelectedCustomers(List<IStoreItemView> selectedCustomers)
    {
        foreach (var item in selectedCustomers)
        {
            await CheckCustomerAndAddToJp(item);

        }
    }
    protected async Task HandleStoreCreditSelected(Winit.Modules.Store.Model.Interfaces.IStoreCredit StoreCredit)
    {
        _journeyplanviewmodel.SelectedCustomer.SelectedStoreCredit = StoreCredit;

        //_journeyplanviewmodel.SelectedCustomer.AvailableCreditLimit = iStoreCredit.TemporaryCredit;
        _journeyplanviewmodel.SelectedCustomer.SelectedOrgUID = StoreCredit.OrgUID;
        _journeyplanviewmodel.SelectedCustomer.SelectedDistributionChannelUID = StoreCredit.DistributionChannelUID;
        _dataManager.SetData(nameof(Winit.Modules.Store.Model.Interfaces.IStoreItemView), _journeyplanviewmodel.SelectedCustomer);
        NavigateTo("Customertemview");
        await Task.CompletedTask;
    }
    private void HandlePopupVisibilityChange(bool IsOpned)
    {
        IsOpnedScoreCredit = IsOpned;
    }
    private void HandlePopupVisibilityChanges(bool isVisible)
    {
        IsVisible = isVisible;
    }
    private async Task CheckCustomerAndAddToJp(IStoreItemView storeItemView)
    {
        // for validation
        await ValidadateCustomer(storeItemView);

        if (_journeyplanviewmodel != null)
        {
            if (_journeyplanviewmodel is Winit.Modules.JourneyPlan.BL.Classes.JourneyPlanViewModel JourneyPlanViewModel)
            {
                // Now you can access the CheckCustomerExistsInJourneyPlan method
                if (storeItemView != null && storeItemView != null)
                {
                    int res = await JourneyPlanViewModel.CheckCustomerExistsInJourneyPlan(storeItemView.StoreUID, _journeyplanviewmodel.SelectedBeatHistory.UID);
                    if (res == 0)
                    {
                        string GUIDForStoreHistory = Guid.NewGuid().ToString();
                        int result = await JourneyPlanViewModel.AddCustomerInJourneyPlan(GUIDForStoreHistory, storeItemView.StoreUID, _journeyplanviewmodel.SelectedBeatHistory.UID);
                        if (result >= 1)
                        {

                            storeItemView.StoreHistoryUID = GUIDForStoreHistory;
                            storeItemView.CurrentVisitStatus = StoryHistoryStatus.UNPLANNED;
                            storeItemView.IsAddedInJP = true;
                            storeItemView.SerialNo = 9999;
                            storeItemView.IsPlanned = false;
                            CustomerlisttoShow.Add(storeItemView);
                            await TabFilter();
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
    public override async Task OnBackClick()
    {
        _backbuttonhandler.ClearCurrentPage();
        _navigationManager.NavigateTo("DashBoard");
        await Task.CompletedTask;
    }
}
