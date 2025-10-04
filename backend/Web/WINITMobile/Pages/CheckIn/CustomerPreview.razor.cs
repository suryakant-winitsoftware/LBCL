using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Winit.UIComponents.Common.Services;
using Microsoft.AspNetCore.Components;
namespace WINITMobile.Pages.CheckIn;

public partial class CustomerPreview
{
    [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }
    public Winit.Modules.JourneyPlan.BL.Interfaces.IJourneyPlanViewModel _journeyplanviewmodel { get; set; }
    private IStoreItemView SelectedStoreItemView;
    // public Winit.Modules.Store.Model.Classes.StoreItemView SelectedStoreItemView = new Winit.Modules.Store.Model.Classes.StoreItemView();
    private bool ShowPopup = false;
    private string selectedReason;
    private bool ShowPopupReasons = false;
    // for map
    public WINITMobile.State.CustomerLatlng CustomersWithLatLon { get; set; } = null;

    // for location
    private double NewLatitude { set; get; } = 0;
    private double NewLongitude { set; get; } = 0;
    public double Diffdistance { set; get; } = 0;
    public double RoadDistance { set; get; } = 0;
    public string DisplayDistanceCordinate { set; get; }
    public string DisplayDistance { set; get; }
    public bool IsRemoteCollection { get; set; } = false;
    private DotNetObjectReference<CustomerPreview>? objRef;

    protected override async Task OnInitializedAsync()
    {
        //_journeyplanviewmodel = _viewmodel._viewmodelJp;
        _journeyplanviewmodel = _viewmodel.CreateJourneyPlanViewModel("DefaultObject");
        _backbuttonhandler.ClearCurrentPage();
        objRef = DotNetObjectReference.Create(this);
        //SelectedStoreItemView = (Winit.Modules.Store.Model.Interfaces.IStoreItemView)_dataManager.GetData("SelectedStoreViewModel");
        // await SetJourneyPlanViewModel(_appUser.ScreenType);
        // if (_journeyplanviewmodel != null)
        // {
        //     SelectedStoreItemView = SelectedStoreItemView;
        //     _appUser.SelectedCustomer = SelectedStoreItemView;
        //     _appUser.SelectedCustomer.BeatHistoryUID = _journeyplanviewmodel.SelectedBeatHistory.UID;
        // }
        dynamic obj = _dataManager.GetData(nameof(Winit.Modules.Store.Model.Interfaces.IStoreItemView));
        if (obj != null)
        {
            SelectedStoreItemView = obj;
            _appUser.SelectedCustomer = SelectedStoreItemView;
        }
        if (CustomersWithLatLon == null) { CustomersWithLatLon = await GetCustomersLatlng(SelectedStoreItemView); }
        // Assuming Latitude and Longitude are strings that represent decimal values
        // CurrentLatitude = (double)(decimal.TryParse(SelectedStoreItemView?.Latitude, out decimal latitude) ? latitude : 0M);
        // CurrentLongitude = (double)(decimal.TryParse(SelectedStoreItemView?.Longitude, out decimal longitude) ? longitude : 0M);

        await SetTopBar();
        // If UserJourneyUID is empty, assign it
        if (string.IsNullOrEmpty(_journeyplanviewmodel.UserJourneyUID))
        {
            _journeyplanviewmodel.UserJourneyUID = _appUser.UserJourney?.UID;
        }

        await CheckGPSANDGetlatitude();

        await InitializeMap();
        await Task.CompletedTask;
        LoadResources(null, _languageService.SelectedCulture);


    }
    private async Task ShowPopupAction()
    {
        ShowLoader(@Localizer["fetching_gps..."]);
        HideLoader();

        Diffdistance = await CalculateDistance(Convert.ToDouble(SelectedStoreItemView.Latitude),
            Convert.ToDouble(SelectedStoreItemView.Longitude), NewLatitude, NewLongitude);

        DisplayDistanceCordinate = FormatDistance(Diffdistance);



        //if (string.IsNullOrWhiteSpace(SelectedStoreItemView?.Latitude) ||
        //    string.IsNullOrWhiteSpace(SelectedStoreItemView?.Longitude) ||
        //    !double.TryParse(SelectedStoreItemView?.Latitude, out var lat) ||
        //    !double.TryParse(SelectedStoreItemView?.Longitude, out var lon) ||
        //    lat == 0 || lon == 0)
        //{
        //    Console.WriteLine("Latitude or Longitude is null, invalid, or zero. Cannot calculate road distance.");
        //    await _alertService.ShowErrorAlert(@Localizer["error"], "Latitude or Longitude is null, invalid, or zero. Cannot calculate road distance.");
        //    return;
        //}



        if (objRef == null)
        {
            objRef = DotNetObjectReference.Create(this);
        }

        //    try
        //    {
        //        await _jSRuntime.InvokeVoidAsync("calculateRoadDistance", Convert.ToDouble(SelectedStoreItemView.Latitude),
        //Convert.ToDouble(SelectedStoreItemView.Longitude), NewLatitude, NewLongitude, objRef);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"JS Interop Error: {ex.Message}");
        //        await _alertService.ShowErrorAlert(@Localizer["error"], $"{ex.Message}");
        //    }

        ShowPopup = true;
        await Task.CompletedTask;
    }

    // Method to calculate distance between two GPS coordinates using Haversine formula
    private async Task<double> CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadius = 6371000; // Earth's radius in meters

        // Convert latitude and longitude from degrees to radians
        double dLat = ToRadian(lat2 - lat1);
        double dLon = ToRadian(lon2 - lon1);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadian(lat1)) * Math.Cos(ToRadian(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // Calculate the distance
        double distance = earthRadius * c;
        return distance;
    }

    // Method to convert degrees to radians
    private double ToRadian(double value)
    {
        return (Math.PI / 180) * value;
    }
    [JSInvokable]
    public async Task GetRoadDistance(string distance)
    {
        RoadDistance = double.Parse(distance);
        DisplayDistance = FormatDistance(RoadDistance);
        await Task.CompletedTask;
    }
    private string FormatDistance(double distance)
    {
        if (distance >= 1000)
        {
            double distanceInKm = distance / 1000;
            return $"{distanceInKm:F2} km";
        }
        else
        {
            return $"{distance:F2} m";
        }
    }

    [JSInvokable]
    public async Task UpdateNewLocation(string latitude, string longitude)
    {
        NewLatitude = double.Parse(latitude);
        NewLongitude = double.Parse(longitude);
        //ShowPopup = true;
        //CalculateRoadDistance(SelectedStoreItemView.Latitude, SelectedStoreItemView.Longitude, NewLatitude, NewLongitude);
        StateHasChanged();
        await Task.CompletedTask;
    }
    private async Task UpdateLocation()
    {
        //if (string.IsNullOrWhiteSpace(SelectedStoreItemView?.AddressUID))
        //{
        //    await _alertService.ShowConfirmationAlert(
        //        "location update",
        //        "This store does not have an address assigned. Please update the address from the backend system.",
        //        null, null, null
        //    );
        //    return;
        //}
        /*
        if (NewLatitude == 0 || NewLongitude == 0)
        {
            await CheckGPSANDGetlatitude();

            // Still not available after checking GPS
            if (NewLatitude == 0 || NewLongitude == 0)
            {
                await _alertService.ShowErrorAlert(
                   "location_update",
                    "unable_to_get_valid_location_coordinates",
                    null, null, null
                );
                return;
            }
        }
        */
        int res = await _addressbl.UpdateAddressDetails(SelectedStoreItemView.AddressUID,
            NewLatitude.ToString(), NewLongitude.ToString()
        );
        

        if (res >= 1)
        {
            SelectedStoreItemView.Longitude = NewLongitude.ToString();
            SelectedStoreItemView.Latitude = NewLatitude.ToString();
            //await HidePopup();
            if (CustomersWithLatLon == null)
            {
                CustomersWithLatLon = await GetCustomersLatlng(SelectedStoreItemView);
            }



            // Reinitialize the map and update the UI
            await InitializeMap();
            StateHasChanged();

            await _alertService.ShowSuccessAlert(@Localizer["location_update"], @Localizer["location_updated_successfully"]);
            await HidePopup();
        }
        else
        {
            await HidePopup();
            await _alertService.ShowConfirmationAlert(@Localizer["location_update"], @Localizer["failed_to_update_location"], null, null, null);
        }
        await Task.CompletedTask;
    }


    private async Task HidePopup()
    {
        ShowPopup = false;
        await Task.CompletedTask;
    }

    private void HideReasonsPopup()
    {
        // Hide the ReasonsModal component
        ShowPopupReasons = false;
    }
    protected async Task OpenDirections()
    {
        await _jSRuntime.InvokeVoidAsync("openDirections", SelectedStoreItemView.Latitude, SelectedStoreItemView.Longitude);
    }
    public async Task<WINITMobile.State.CustomerLatlng> GetCustomersLatlng(Winit.Modules.Store.Model.Interfaces.IStoreItemView customer)
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
    }
    protected async Task InitializeMap()
    {
        // Wait for a short delay to ensure the DOM elements are rendered
        await Task.Delay(100);

        // Retrieve the latitude and longitude values
        var latitude = SelectedStoreItemView?.Latitude;
        var longitude = SelectedStoreItemView?.Longitude;

        if (latitude != null && longitude != null)
        {
            string serializedCustomers = JsonConvert.SerializeObject(CustomersWithLatLon);

            await _jSRuntime.InvokeVoidAsync("initializeCustomer", serializedCustomers);
        }
        var location = new
        {
            Latitude = NewLatitude,
            Longitude = NewLongitude
        };
        string serializedCurrentLocation = JsonConvert.SerializeObject(location);
        await _jSRuntime.InvokeVoidAsync("initializeCurrentLocation", serializedCurrentLocation, objRef);

        base.StateHasChanged();
        await Task.CompletedTask;
    }
    protected async Task SetTopBar()
    {
        WINITMobile.Models.TopBar.MainButtons buttons = new WINITMobile.Models.TopBar.MainButtons()
        {
            TopLabel = @Localizer["plan_for_the_call"],

            UIButton1 = null,

        };
        await Btnname.InvokeAsync(buttons);
    }
    private async Task ShowReasonsPopup()
    {
        try
        {
            // Show loader while fetching GPS data
            ShowLoader(@Localizer["fetching_gps..."]);
            Diffdistance = 31;
            // // Fetch GPS data asynchronously
            await CheckGPSANDGetlatitude();

            // Calculate distance asynchronously
            Diffdistance = await CalculateDistance(
                Convert.ToDouble(SelectedStoreItemView.Latitude),
                Convert.ToDouble(SelectedStoreItemView.Longitude),
                NewLatitude,
                NewLongitude
            );

            // Hide loader after GPS data and distance calculation are completed
            HideLoader();

            if (Diffdistance > 30)
            {
                // If the difference is more than 30 meters, show the popup
                ///ShowPopupReasons = true;
                /// TempSKIP_REASON = false;
                if (await _alertService.ShowConfirmationReturnType(@Localizer["alert"], @Localizer["it_seems_you_are_not_in_customer_location,do_you_want_to_do_force_check_in"], @Localizer["yes"], @Localizer["no"]))
                {
                    List<ISelectionItem> Reasonsforcecheck_in = _appUser.ReasonDictionary["FORCE_CHECKIN"];
                    await _dropdownService.ShowMobilePopUpDropDown(new DropDownOptions
                    {
                        DataSource = Reasonsforcecheck_in,
                        OnSelect = async (eventArgs) =>
                        {
                            await SelectForForce_Check_In(eventArgs);
                        },
                        OkBtnTxt = @Localizer["submit"],
                        Title = @Localizer["please_select_force_chekin_reason"]
                    });
                }
                else
                {
                    return;
                }
            }
            else
            {
                // If the difference is within 30 meters, proceed with navigation
                await ProceedWithNavigation();
            }
        }
        catch (Exception ex)
        {
            HideLoader();
            Console.WriteLine(ex);
        }
        await Task.CompletedTask;

    }

    protected async Task RemoteCollection()
    {
        List<ISelectionItem> ReasonsforRemote_collection = _appUser.ReasonDictionary["REMOTE_COLLECTION_REASON"];
        await _dropdownService.ShowMobilePopUpDropDown(new DropDownOptions
        {
            DataSource = ReasonsforRemote_collection,
            OnSelect = async (eventArgs) =>
            {
                await SelectForRemote_Collection(eventArgs);
            },
            OkBtnTxt = @Localizer["submit"],
            Title = @Localizer["select_remote_collection_reason"]
        });
    }

    protected async Task OpenAlert()
    {
        try
        {
            await _alertService.ShowErrorAlert(@Localizer["upcoming"], @Localizer["this_feature_coming_in_phase2"], null, @Localizer["ok"]);
        }
        catch (Exception ex)
        {

        }
    }

    public async Task SelectForRemote_Collection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        try
        {
            if (dropDownEvent.SelectionItems.Count != 0)
            {
                _appUser.IsRemoteCollection = true;
                _appUser.RemoteCollectionReason = dropDownEvent.SelectionItems.First().Label;
                NavigateTo("Payment");
                await Task.CompletedTask;
            }
            else
            {
                await _alertService.ShowErrorAlert(@Localizer["alert"], @Localizer["please_select_a_reason_to_continue"]);
            }
        }
        catch (Exception ex)
        {

        }
    }


    public async Task SelectForForce_Check_In(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        try
        {
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null)
                {
                    // here it is for unselect the select one But later it will go the main component
                    foreach (var item in dropDownEvent.SelectionItems)
                    {
                        item.IsSelected = false;
                    }
                    ISelectionItem selectionItem = dropDownEvent.SelectionItems.FirstOrDefault();
                    _journeyplanviewmodel.SelectedCheckInReason = selectionItem.Label;


                    // call base view model
                    bool IsCheckIn = await _journeyplanviewmodel.PrepareDBForCheckIn(SelectedStoreItemView);

                    if (IsCheckIn)
                    {
                        //niranjan it is for checking the navigation
                        //_navigationManager.NavigateTo("CustomerCall");
                        _appUser.IsCheckedIn = true;
                        SelectedStoreItemView.CheckInTime = DateTime.Now;
                        _sideBarService.RefreshSideBar.Invoke();
                        SelectedStoreItemView.IsSkipped = false;

                        SelectedStoreItemView.CurrentVisitStatus = Winit.Shared.Models.Constants.StoryHistoryStatus.VISITED;
                        if (string.IsNullOrEmpty(_journeyplanviewmodel.UserJourneyUID))
                        {
                            _journeyplanviewmodel.UserJourneyUID = _appUser.UserJourney?.UID;
                        }
                        _dataManager.SetData(nameof(Winit.Modules.Store.Model.Interfaces.IStoreItemView), SelectedStoreItemView);
                        NavigateTo("CustomerCall");

                    }
                }
            }

        }
        catch (Exception ex)
        {
            HideLoader();
            _alertService.ShowErrorAlert("Error", ex.Message);
        }
        await Task.CompletedTask;
    }

    private async Task ProceedWithNavigation()
    {
        // Update user journey status
        _appUser.IsCheckedIn = true;
        SelectedStoreItemView.CheckInTime = DateTime.Now;
        _sideBarService.RefreshSideBar.Invoke();
        SelectedStoreItemView.IsSkipped = false;
        SelectedStoreItemView.CurrentVisitStatus = Winit.Shared.Models.Constants.StoryHistoryStatus.VISITED;

        // If UserJourneyUID is empty, assign it
        if (string.IsNullOrEmpty(_journeyplanviewmodel.UserJourneyUID))
        {
            _journeyplanviewmodel.UserJourneyUID = _appUser.UserJourney?.UID;
        }

        // Update status for StoreHistory and navigate to CustomerCall if check-in is successful
        bool isCheckIn = await _journeyplanviewmodel.PrepareDBForCheckIn(SelectedStoreItemView);
        if (isCheckIn)
        {
            _dataManager.SetData(nameof(Winit.Modules.Store.Model.Interfaces.IStoreItemView), SelectedStoreItemView);
            _navigationManager.NavigateTo("CustomerCall");
        }
        await Task.CompletedTask;
    }


    private async Task CheckGPSANDGetlatitude()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
#if WINDOWS
            status = PermissionStatus.Granted;
#endif
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    await Application.Current.MainPage.DisplayAlert(@Localizer["permission_denied"], @Localizer["location_permission_is_required_to_use_this_feature"], @Localizer["ok"]);
                    return;
                }
            }

            var location = await Geolocation.GetLocationAsync();
            var isGPSEnabled = location != null;

            if (isGPSEnabled)
            {
                NewLatitude = location.Latitude;
                NewLongitude = location.Longitude;

                // ShowPopup = true;
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(@Localizer["gps"], @Localizer["gps_is_not_enabled._do_you_want_to_enable_it?"], @Localizer["yes"], @Localizer["no"]);
            }
        }
        catch (FeatureNotSupportedException)
        {
            await Application.Current.MainPage.DisplayAlert(@Localizer["gps"], @Localizer["gps_is_not_supported_on_this_device"], @Localizer["ok"]);
        }
        catch (PermissionException)
        {
            await Application.Current.MainPage.DisplayAlert(@Localizer["gps"], @Localizer["location_permission_is_not_granted"], @Localizer["ok"]);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(@Localizer["gps"], $"Error: {ex.Message}", @Localizer["ok"]);
        }
        await Task.CompletedTask;
    }
    private async Task HandleSkipClicked()
    {
        List<ISelectionItem> ReasonsfoRSkip_Customer = _appUser.ReasonDictionary["SKIP_REASON"];
        if (!ReasonsfoRSkip_Customer.Any(item => item.UID == "other"))
        {
            ReasonsfoRSkip_Customer.Add(new SelectionItem { UID = "other", Label = "Other" });
        }
        await _dropdownService.ShowMobilePopUpDropDown(new DropDownOptions
        {
            DataSource = ReasonsfoRSkip_Customer,
            OnSelect = async (eventArgs) =>
            {
                await SelectForSkip_Customer(eventArgs);
            },
            OkBtnTxt = @Localizer["done"],
            Title = @Localizer["skip_reason"],
            ShowOtherOption = true
        });

        await Task.CompletedTask;
    }

    public async Task SelectForSkip_Customer(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        try
        {
            if (dropDownEvent != null && dropDownEvent.SelectionItems != null)
            {
                // here it is for unselect the select one But later it will go the main component
                foreach (var item in dropDownEvent.SelectionItems)
                {
                    item.IsSelected = false;
                }
                ISelectionItem selectionItem = dropDownEvent.SelectionItems.FirstOrDefault();
                if (selectionItem != null)
                {
                    _journeyplanviewmodel.SKIP_REASON = selectionItem.Label;
                    int res = await _journeyplanviewmodel.UpdateStatusInStoreHistory(SelectedStoreItemView.StoreHistoryUID, Winit.Shared.Models.Constants.StoryHistoryStatus.SKIPPED);
                    if (res >= 1)
                    {
                        SelectedStoreItemView.CurrentVisitStatus = Winit.Shared.Models.Constants.StoryHistoryStatus.SKIPPED;
                        _navigationManager.NavigateTo("JourneyPlan");
                    }
                    else
                    {
                        // handle error
                    }
                }
            }
        }
        catch (Exception ex)
        {
            HideLoader();
            Console.WriteLine(ex);
        }
        await Task.CompletedTask;
    }
    protected async Task Dispose()
    {
        _viewmodel?.Dispose();
        objRef?.Dispose();
        await Task.CompletedTask;
    }
}
