using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Events;

using Winit.UIComponents.Common.Language;
using WinIt.Pages.Base;
namespace WinIt.Pages.RouteManagementMaster;

public partial class AddRoute : BaseComponentBase
{
    [Parameter]
    public string? RouteUID { get; set; }
    [CascadingParameter]
    public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
    // Ui display  lists and variables
    private readonly bool IsAddCutomersWithTime;
    private bool IsEditMode = false;
    public bool IsShowPreviousRouteDeatails { set; get; } = false;
    public bool showCustomerListpopup = false;
    public bool IsVisible = false;
    public RouteSchedule RouteSchedule = new();
    private readonly bool showCustomerList = false;

    //validation Message Variables
    public string? ValidationMessage { get; private set; }
    public string? ValidationShowCustomerListPopupMessage { get; private set; }
    public string? validationMessage { get; private set; }
    // this for to get the selcted customer uids 
    private readonly List<string> selectedCustomerUIDs = new();
    private readonly string Daily = "Daily";
    private readonly string Weekly = "Weekly";
    private readonly string Type1 = "Type1";
    private readonly string Type2 = "Type2";
    private readonly string MultiplePerWeek = "MultiplePerWeek";
    private readonly string Monthly = "Monthly";
    private readonly string Fortnightly = "Fortnightly";
    private bool IsLoaded = false;
    private readonly bool IsAddPage = false;
    public int CurrentSeq;
    private readonly List<string> DaysOfWeek = new() { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday",
        "Saturday", "Sunday" };
    private readonly List<string> DaysOfWeekFN = new() { "MondayFN", "TuesdayFN", "WednesdayFN", "ThursdayFN",
        "FridayFN", "SaturdayFN", "SundayFN" };
    private bool isRouteDetailsPopupVisible;
    private bool IsModalVisible { get; set; }
    private string ModalType { get; set; } = "";
    public List<string>? StoredTab;
    private string? CustomertxtInput { get; set; }
    private TimeOnly DayStarts;
    private TimeOnly DayEnds;
    private int Duration;
    private int TravelTime;
    private IRouteCustomerItemView? SelectedRouteCustomer;
    private bool ForAutoCutOffTime { get; set; }
    private Winit.UIComponents.Common.CustomControls.DropDown? VehicleDDRef;
    private Winit.UIComponents.Common.CustomControls.DropDown? WareHouseDDRef;
    private Winit.UIComponents.Common.CustomControls.DropDown? UserDDRef;
    private Winit.UIComponents.Common.CustomControls.DropDown? OtherUserDDRef;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            LoadResources(null, _languageService.SelectedCulture);
            ShowLoader();
            RouteUID = _commonFunctions.GetParameterValueFromURL("UID");
            if (RouteUID == null)
            {
                RouteUID = Guid.NewGuid().ToString();
                await _routeManagement.PopulateViewModel(RouteUID, false);
            }
            else
            {
                IsEditMode = true;
                await _routeManagement.PopulateViewModel(RouteUID, true);
                IsShowPreviousRouteDeatails = true;
                // UNCOMMENT TOMMORROW
                BindDaysFromPlannedDays(_routeManagement.RouteSchedule.PlannedDays);
                await _routeManagement.BindAllDropdownsForEditpage();
                if (_routeManagement.RouteChangeLog.VisitTime != null)
                {
                    DayStarts = TimeOnly.FromTimeSpan(TimeSpan.Parse(_routeManagement.RouteChangeLog.VisitTime));
                }
                if (_routeManagement.RouteChangeLog.EndTime != null)
                {
                    DayEnds = TimeOnly.FromTimeSpan(TimeSpan.Parse(_routeManagement.RouteChangeLog.EndTime));
                }
                Duration = _routeManagement.RouteChangeLog.VisitDuration;
                TravelTime = _routeManagement.RouteChangeLog.TravelTime;
            }
            await SetBreadCrumb();
            IsLoaded = true;
            HideLoader();
        }
        catch (Exception ex)
        {
            HideLoader();
            Console.WriteLine(ex);
        }
    }

    private async Task HandleVehicleSelection(DropDownEvent eventArgs)
    {
        if (eventArgs != null && eventArgs.SelectionItems != null && eventArgs.SelectionItems.Any())
        {
            // Assuming the UID property in SelectionItem corresponds to the UID of the Vehicle class
            Winit.Shared.Models.Common.ISelectionItem item = eventArgs.SelectionItems.First();
            _routeManagement.RouteChangeLog.VehicleUID = item.UID;
        }
        await Task.CompletedTask;
    }

    private async Task HandleWarehouseSelection(DropDownEvent eventArgs)
    {
        if (eventArgs != null && eventArgs.SelectionItems != null && eventArgs.SelectionItems.Count > 0)
        {
            // Assuming the UID property in SelectionItem corresponds to the UID of the Warehouse
            Winit.Shared.Models.Common.ISelectionItem? item = eventArgs.SelectionItems.First();
            _routeManagement.RouteChangeLog.WHOrgUID = item.UID;
        }
        await Task.CompletedTask;
    }
    private async Task HandleRoleSelection(DropDownEvent eventArgs)
    {
        if (eventArgs != null && eventArgs.SelectionItems != null && eventArgs.SelectionItems.Count > 0)
        {
            await _routeManagement.OnRoleSelect(eventArgs.SelectionItems.First());
        }
        WareHouseDDRef?.DeSelectAll();
        VehicleDDRef?.DeSelectAll();
        OtherUserDDRef?.DeSelectAll();
        UserDDRef?.DeSelectAll();
        await Task.CompletedTask;
    }

    private async Task HandleUserSelection(DropDownEvent eventArgs)
    {
        if (eventArgs != null && eventArgs.SelectionItems != null && eventArgs.SelectionItems.Any())
        {
            Winit.Shared.Models.Common.ISelectionItem? item = eventArgs.SelectionItems.First();
            _routeManagement.RouteChangeLog.JobPositionUID = item.UID;
            // Remove the selected user from the list of other users
            _routeManagement.OtherUserSelectionItems = _routeManagement.OtherUserSelectionItems.Where
                (user => user.UID != item?.UID).ToList();
        }
        await Task.CompletedTask;
    }

    private async Task HandleUserOtherSelection(DropDownEvent eventArgs)
    {
        await _routeManagement.OnOtherUserSelection(eventArgs);
    }

    public async Task SetBreadCrumb()
    {
        _IDataService.BreadcrumList = new()
        {
            new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text =@Localizer["route_management"] , IsClickable = true,
                URL = "routemanagement" },
            new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = $"{(IsEditMode ? @Localizer["edit"] : @Localizer["add"])} {@Localizer["route"]})",
                IsClickable = false }
        };
        _IDataService.HeaderText = $"{(IsEditMode ? @Localizer["edit"] : @Localizer["add"])} {@Localizer["route"]}";
        await CallbackService.InvokeAsync(_IDataService);
    }

    private async Task Update()
    {
        try
        {
            if (!await ValidateRoute())
            {
                // Show validation error
                ShowErrorSnackBar(@Localizer["validation_error"], ValidationMessage);
                return;
            }
            else if (!await _alertService.ShowConfirmationReturnType("", @Localizer["are_you_sure_you_want_to_update?"], @Localizer["yes"], @Localizer["no"]))
            {
                return;
            }
            else if (await _routeManagement.Update())
            {
                ShowSuccessSnackBar(@Localizer["success"], @Localizer["route_successfully_updated."]);
                await Task.Run(() => _navigationManager.NavigateTo("routemanagement"));
            }
            else
            {
                ShowErrorSnackBar(@Localizer["error"], @Localizer["update_failed:"]);
            }
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert(@Localizer["error"], $"{@Localizer["update_failed:"]} {ex.Message}");
        }
    }

    private void HandlePopupVisibilityChange(bool isVisible)
    {
        IsVisible = isVisible;
    }

    public void HandleSelectedCustomers(List<IStoreCustomer> selectedCustomers)
    {
        _routeManagement.HandleSelectedCustomers(selectedCustomers);
    }

    public async Task<bool> ValidateCustomerListShowPopup()
    {
        ValidationShowCustomerListPopupMessage = "";
        if (string.IsNullOrEmpty(_routeManagement.RouteSchedule.Type))
        {
            ValidationShowCustomerListPopupMessage = @Localizer["please_select_visit_frequency."];
            return false; // Validation failed
        }
        await Task.CompletedTask;
        return true; // Validation passed
    }

    private async Task ShowCustomerListPopup()
    {
        // Validate before showing the customer list
        if (!await ValidateCustomerListShowPopup())
        {
            // Show validation error
            ShowErrorSnackBar(@Localizer["validation_error"], ValidationShowCustomerListPopupMessage);
            return;
        }
        showCustomerListpopup = true;
        IsVisible = true;
    }

    private async Task Back()
    {
        if (await _alertService.ShowConfirmationReturnType(@Localizer["alert"], @Localizer["are_you_sure_you_want_to_go_back?"], @Localizer["yes"], @Localizer["no"]))
        {
            _navigationManager.NavigateTo("routemanagement");
        }
    }

    private async Task<bool> ValidateRoute()
    {
        ValidationMessage = "";
        if (_routeManagement.RouteChangeLog != null && _routeManagement.RouteSchedule != null)
        {
            if (string.IsNullOrEmpty(_routeManagement.RouteChangeLog.Code)
            || string.IsNullOrEmpty(_routeManagement.RouteChangeLog.Name)
            || _routeManagement.RouteChangeLog.ValidFrom == default ||
            _routeManagement.RouteChangeLog.ValidUpto == default ||
            string.IsNullOrEmpty(_routeManagement.RouteSchedule.Type) ||
            string.IsNullOrEmpty(_routeManagement.RouteChangeLog.JobPositionUID) ||
            _routeManagement.RouteUsers == null || !_routeManagement.RouteUsers.Any() ||
            string.IsNullOrEmpty(_routeManagement.RouteChangeLog.WHOrgUID)
            || _routeManagement.SelectedRole == null || (_routeManagement.SelectedRole.HaveVehicle &&
            string.IsNullOrEmpty(_routeManagement.RouteChangeLog.VehicleUID)) || (_routeManagement.SelectedRole.HaveWarehouse &&
            string.IsNullOrEmpty(_routeManagement.RouteChangeLog.WHOrgUID)))
            {
                ValidationMessage = @Localizer["the_following_field(s)_have_invalid_value(s):"];
                if (string.IsNullOrEmpty(_routeManagement.RouteChangeLog.Code))
                {
                    ValidationMessage += @Localizer["routecode,"];
                }
                if (string.IsNullOrEmpty(_routeManagement.RouteChangeLog.Name))
                {
                    ValidationMessage += @Localizer["routename,"];
                }
                if (string.IsNullOrEmpty(_routeManagement.RouteChangeLog.JobPositionUID))
                {
                    ValidationMessage += @Localizer["user,"];
                }
                if (_routeManagement.RouteUsers == null || !_routeManagement.RouteUsers.Any())
                {
                    ValidationMessage += @Localizer["other_user,"];
                }
                if (_routeManagement.SelectedRole == null)
                {
                    ValidationMessage += @Localizer["role,"];

                }
                if (_routeManagement.SelectedRole != null && _routeManagement.SelectedRole.HaveWarehouse &&
                    string.IsNullOrEmpty(_routeManagement.RouteChangeLog.WHOrgUID))
                {
                    ValidationMessage += @Localizer["warehouse,"];
                }
                if (_routeManagement.SelectedRole != null && _routeManagement.SelectedRole.HaveVehicle &&
                    string.IsNullOrEmpty(_routeManagement.RouteChangeLog.VehicleUID))
                {
                    ValidationMessage += @Localizer["vehicle,"];
                }
                if (_routeManagement.RouteChangeLog.ValidFrom == default)
                {
                    ValidationMessage += @Localizer["fromdate,"];
                }
                if (_routeManagement.RouteChangeLog.ValidUpto == default)
                {
                    ValidationMessage += @Localizer["todate,"];
                }
                if (string.IsNullOrEmpty(_routeManagement.RouteSchedule.Type))
                {
                    ValidationMessage += @Localizer["visit_frequency,"];
                }

                ValidationMessage = ValidationMessage.TrimEnd(' ', ',');

                return false; // Validation failed
            }
        }
        await Task.CompletedTask;
        return true; // Validation passed
    }

    public async Task Save()
    {
        try
        {
            // Validate before saving
            if (!await ValidateRoute())
            {
                // Show validation error
                ShowErrorSnackBar(@Localizer["validation_error"], ValidationMessage);
                return;
            }

            if (!await _alertService.ShowConfirmationReturnType("", @Localizer["are_you_sure_you_want_to_save?"], @Localizer["yes"], @Localizer["no"]))
            {
                return;
            }
            if (IsAddCutomersWithTime && _routeManagement.RouteCustomerItemViews.Any() &&
                !await _routeManagement.ValidateCustomerVistTimes())
            {
                if (!await _alertService.ShowConfirmationReturnType("",
                   @Localizer["customer_time_exceeding_the_route_end_time._do_you_want_to_continue?"], @Localizer["yes"], @Localizer["no"]))
                {
                    return;
                }
            }
            bool saveResult = await _routeManagement.Save();
            if (saveResult)
            {
                ShowSuccessSnackBar(@Localizer["success"], @Localizer["save_route_successfully."]);
                _navigationManager.NavigateTo("routemanagement");
            }
            else
            {
                // Show error alert with the error message
                ShowErrorSnackBar(@Localizer["error"], @Localizer["failed_to_save_route._error:"]);
            }
        }
        catch (Exception)
        {
            ShowErrorSnackBar(@Localizer["error"], @Localizer["an_unexpected_error_occurred_while_saving_the_route."]);
        }
    }


    // this is for customer list seq changing logic


    //void HandleBlur(StoreCustomer changedRow)
    //{

    //    //  var tempDataList = _routeManagement.RouteCustomersList
    //    //.Where(r => r.SeqType == ActiveTab)
    //    //.OrderBy(r => r.SerialNo)
    //    //.ToList();
    //    //  _routeManagement.RouteCustomersList.RemoveAll(r => r.SeqType == ActiveTab);

    //    var seq = changedRow.SeqNo;
    //    int index = _routeManagement.DisplayCustomersList.IndexOf(changedRow);
    //    _routeManagement.DisplayCustomersList[index].SeqNo = changedRow.SeqNo;
    //    for (int i = 0; i < _routeManagement.DisplayCustomersList.Count; i++)
    //    {
    //        if (CurrentSeq > seq)
    //        {
    //            if (i != index && _routeManagement.DisplayCustomersList[i].SeqNo == seq)
    //            {
    //                _routeManagement.DisplayCustomersList[i].SeqNo = _routeManagement.DisplayCustomersList[i].SeqNo + 1;

    //            }
    //        }
    //        else
    //        {
    //            if (i != index && _routeManagement.DisplayCustomersList[i].SeqNo == seq)
    //            {
    //                _routeManagement.DisplayCustomersList[i].SeqNo = _routeManagement.DisplayCustomersList[i].SeqNo - 1;

    //            }
    //        }
    //    }
    //    _routeManagement.DisplayCustomersList = _routeManagement.DisplayCustomersList.OrderBy(r => r.SeqNo).ToList();
    //    for (int i = 0; i < _routeManagement.DisplayCustomersList.Count; i++)
    //    {
    //        _routeManagement.DisplayCustomersList[i].SeqNo = i + 1;
    //    }
    //    //_routeManagement.RouteCustomersList.AddRange(tempDataList);
    //    StateHasChanged();
    //}

    public async Task Delete()
    {
        try
        {
            // Check if any customers are selected
            List<IRouteCustomerItemView> selectedStoreCustomers = _routeManagement.DisplayRouteCustomerItemViews.FindAll
                (e => e.IsSelected);
            if (selectedStoreCustomers != null && selectedStoreCustomers.Any())
            {
                // Show confirmation alert before proceeding with delete
                await _alertService.ShowConfirmationAlert(@Localizer["confirmation"], @Localizer["are_you_sure_you_want_to_delete?"],
                    async (param) => await Handel_ConfirmDelete(selectedStoreCustomers), @Localizer["yes"], @Localizer["no"]);
            }
            else
            {
                // Show a message that no customers are selected for deletion
                // You can add your logic or notification here
            }

        }
        catch (Exception)
        {
            // Handle unexpected exceptions
            // You can log or display an error message here
        }
    }

    private async Task Handel_ConfirmDelete(List<IRouteCustomerItemView> selectedRouteCustomers)
    {
        if (!IsEditMode)
        {
            selectedRouteCustomers.ForEach(e =>
            {
                _ = _routeManagement.DisplayRouteCustomerItemViews.Remove(e);
                _ = _routeManagement.FilteredRouteCustomerItemViews.Remove(e);
                _ = _routeManagement.RouteCustomerItemViews.Remove(e);
            });
        }
        else
        {
            bool isConfirmDeleteSuccessful = await _routeManagement.ConfirmDelete
            (selectedRouteCustomers.Select(e => e.UID).ToList());

            if (isConfirmDeleteSuccessful)
            {
                selectedRouteCustomers.ForEach(e => e.IsDeleted = true);
                // Successfully deleted customers, you can proceed with further logic
                StateHasChanged();
            }
            else
            {
                ShowErrorSnackBar(@Localizer["failed"], @Localizer["failed_to_delete_cutomers"]);
            }
        }
        StateHasChanged();
    }
    private void ApplyTimes()
    {
        try
        {
            _routeManagement.ApplyTimes(DayStarts, DayEnds);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ShowErrorSnackBar(@Localizer["error"], ex.Message);
        }
    }

    private async Task ApplyDuratinonAndTravelTime()
    {
        try
        {
            if (await _alertService.ShowConfirmationReturnType
                (@Localizer["alert"], @Localizer["values_will_be_overriden_by_this._are_you_sure_to_continue?"], @Localizer["yes"], @Localizer["no"]))
            {
                _routeManagement.ApplyDuratinonAndTravelTime(Duration, TravelTime);
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            ShowErrorSnackBar(@Localizer["error"], ex.Message);
        }
    }
    private void OnTravelTimeChange(string travelTime)
    {
        if (SelectedRouteCustomer != null)
        {
            SelectedRouteCustomer.TravelTime = int.Parse(travelTime);
            _routeManagement.CalculateTimes();
            StateHasChanged();
        }
    }
    private void OnDurationChange(string duration)
    {
        if (SelectedRouteCustomer != null)
        {
            SelectedRouteCustomer.VisitDuration = int.Parse(duration);
            _routeManagement.CalculateTimes();
            StateHasChanged();
        }
    }
    private void AddTxtEnteredCustomerToGrid()
    {
        if (CustomertxtInput is not null)
        {
            CustomertxtInput = CustomertxtInput.Trim();
            List<string> codesAndLabels = _routeManagement.RouteCustomerItemViews.SelectMany
                (e => new List<string> { e.StoreCode, e.StoreLabel }).ToList();
            IStoreCustomer? storeCustomer = _routeManagement.StoreCustomers.Find
                (e =>
               e.Code.Equals(CustomertxtInput, StringComparison.OrdinalIgnoreCase) ||
                 e.Label.Equals(CustomertxtInput, StringComparison.OrdinalIgnoreCase));
            if (storeCustomer != null)
            {
                if (codesAndLabels.Contains(storeCustomer.Code))
                {
                    ShowErrorSnackBar(@Localizer["error"], @Localizer["customer_already_added"]);
                    return;
                }
                HandleSelectedCustomers(new List<IStoreCustomer> { storeCustomer });
                ShowSuccessSnackBar(@Localizer["success"], @Localizer["customer_added_successfully"]);
                StateHasChanged();
            }
            else
            {
                ShowErrorSnackBar(@Localizer["error"], @Localizer["customer_does_not_exist"]);
            }
        }
        else
        {
            ShowErrorSnackBar(@Localizer["error"], @Localizer["enter_customer_code_or_name"]);
        }
    }

    private async Task WinitTextBox_OnSearch(string searchString)
    {
        await _routeManagement.ApplySearch(searchString);
        StateHasChanged();
    }
    private void ShowModal(string modalType)
    {
        IsModalVisible = true;
        ModalType = modalType;
    }

    private void CloseModal()
    {
        IsModalVisible = false;
        ModalType = "";
    }

    public async Task ShowErrorPopforTest()
    {
        await _alertService.ShowErrorAlert(@Localizer["add_customer_by_number"], @Localizer["this_service_coming_soon"]);
    }

    // this is for type variable 

    private void CloseRouteDetailsPopup()
    {
        isRouteDetailsPopupVisible = false;
    }

    private void ToggleRouteDetailsPopup()
    {
        // Toggle the visibility of the popup
        isRouteDetailsPopupVisible = !isRouteDetailsPopupVisible;
    }

    private void UpdateSelectedDays(string day)
    {
        if (_routeManagement.RouteScheduleDaywise == null)
        {
            _routeManagement.RouteScheduleDaywise = new RouteScheduleDaywise();
        }

        System.Reflection.PropertyInfo? property = _routeManagement.RouteScheduleDaywise.GetType().GetProperty(day);
        if (property != null)
        {
            int value = (int)property.GetValue(_routeManagement.RouteScheduleDaywise)!;
            property.SetValue(_routeManagement.RouteScheduleDaywise, value == 1 ? 0 : 1);
        }
    }

    private void UpdateSelectedFortnights(string day)
    {
        if (_routeManagement.RouteScheduleFortnight == null)
        {
            _routeManagement.RouteScheduleFortnight = new RouteScheduleFortnight();
        }

        System.Reflection.PropertyInfo? property = _routeManagement.RouteScheduleFortnight.GetType().GetProperty(day);
        if (property != null)
        {
            int value = (int)property.GetValue(_routeManagement.RouteScheduleFortnight)!;
            property.SetValue(_routeManagement.RouteScheduleFortnight, value == 1 ? 0 : 1);
        }
    }

    private readonly HashSet<int> _selectedMonthlyDays = new();

    private bool IsDaySelected(int day)
    {
        return _selectedMonthlyDays.Contains(day);
    }

    private void UpdateMonthlyDays(int day)
    {
        _ = _selectedMonthlyDays.Contains(day) ? _selectedMonthlyDays.Remove(day) : _selectedMonthlyDays.Add(day);

        // Convert the selected days to a comma-separated string
        _routeManagement.RouteSchedule.PlannedDays = string.Join(",", _selectedMonthlyDays.OrderBy(d => d));
    }
    private void BindDaysFromPlannedDays(string plannedDays)
    {
        _selectedMonthlyDays.Clear();
        if (!string.IsNullOrEmpty(plannedDays))
        {
            IEnumerable<int> selectedDays = plannedDays.Split(',').Select(int.Parse);
            foreach (int day in selectedDays)
            {
                _ = _selectedMonthlyDays.Add(day);
            }
        }
    }

    private void CheckAllDays()
    {
        if (_routeManagement.RouteScheduleDaywise == null)
        {
            _routeManagement.RouteScheduleDaywise = new RouteScheduleDaywise();
        }

        foreach (string day in DaysOfWeek)
        {
            System.Reflection.PropertyInfo? property = _routeManagement.RouteScheduleDaywise.GetType().GetProperty(day);
            property?.SetValue(_routeManagement.RouteScheduleDaywise, 1);
        }
    }

    private async Task UncheckAllDays()
    {
        if (_routeManagement.RouteScheduleDaywise == null)
        {
            _routeManagement.RouteScheduleDaywise = new RouteScheduleDaywise();
        }
        if (_routeManagement.RouteScheduleDaywise != null || _routeManagement.RouteScheduleFortnight != null)
        {
            bool result = await _alertService.ShowConfirmationReturnType(@Localizer["alert"], @Localizer["if_you_change_visit_frequency_all_data_will_be_gone"], @Localizer["confirm"], @Localizer["cancel"]);

            if (result)
            {

                foreach (string day in DaysOfWeek)
                {
                    System.Reflection.PropertyInfo? property = _routeManagement?.RouteScheduleDaywise?.GetType().GetProperty(day);
                    property?.SetValue(_routeManagement?.RouteScheduleDaywise, 0);
                }
            }
        }

    }
}



