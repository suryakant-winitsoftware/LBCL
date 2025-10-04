using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Winit.Modules.Common.Model.Constants;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Services;
using static Azure.Core.HttpHeader;

namespace WINITMobile.Shared;

partial class NavMenu
{
    private bool collapseNavMenu = true;
    private bool summarySubMenu = false;
    private bool poSubMenu = false;
    private bool invoiceSubMenu = false;
    private bool callSubMenu = false;
    private bool stockMenu = false;
    private bool IsWindows = false;
    [Parameter] public EventCallback<bool> CloseMenu { get; set; }

    protected async override Task OnInitializedAsync()
    {
        _sideBarService.RefreshSideBar = RefreshScreen;
        _sideBarService.OnOpenRouteDD = async () => await OnRouteDDClick();
        // _sideBarService.OnSwipeDirection = OnSwipeGesture;
        LoadResources(null, _languageService.SelectedCulture);
#if WINDOWS
        IsWindows = true;
#endif
    }

    /// <summary>
    /// Opens the Razorpay payroll dashboard in a new browser tab.
    /// </summary>
    private async Task OpenRazorpayDashboard()
    {
        try
        {
            await _jSRuntime.InvokeVoidAsync("window.open", "https://payroll.razorpay.com/dashboard", "_blank");
            await CloseMenu.InvokeAsync(false);
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert(@Localizer["alert"], "Unable to open the external link. Please try again.");
        }
    }

    private async Task HandleCheckOutClicked()
    {
        //await OnCheckOutClicked.InvokeAsync();
        _sideBarService.OnCheckOutClick.Invoke();
    }
    /* public void OnSwipeGesture(Winit.UIComponents.Mobile.Enums.SwipeDirection direction)
         {
         // if (direction == Winit.UIComponents.Mobile.Enums.SwipeDirection.LeftToRight){
         //     collapseNavMenu = false;
         // }
         // else if (direction == Winit.UIComponents.Mobile.Enums.SwipeDirection.LeftToRight){
         //     collapseNavMenu = true;
     // }
     }*/
    public void ToggleNavMenu()
    {
        collapseNavMenu = !collapseNavMenu;
    }

    public void ToggleStock()
    {
        stockMenu = !stockMenu;
    }
    public void ToggleSummary()
    {
        summarySubMenu = !summarySubMenu;
    }
    public void TogglePO()
    {
        poSubMenu = !poSubMenu;
    }
    public void ToggleInvoice()
    {
        invoiceSubMenu = !invoiceSubMenu;
    }
    public void ToggleCall()
    {
        callSubMenu = !callSubMenu;
    }

    public async Task returnpath(string path)
    {
        //_navigationManager.NavigateTo(path, true);
        _backbuttonhandler.ClearCurrentPage();
        NavigateTo(path);
        await CloseMenu.InvokeAsync(false);
    }
    private void RefreshScreen()
    {
        StateHasChanged();
    }

    private async Task OnLogoutClick()
    {
        if (await _alertService.ShowConfirmationReturnType(@Localizer["alert"], @Localizer["are_you_sure_you_want_to_logout?"], @Localizer["yes"], @Localizer["no"]))
        {
            _appUser.IsCheckedIn = false;
            _sideBarService.RefreshSideBar.Invoke();
            await _localStorageService.RemoveItem("token");
            await _authStateProvider.GetAuthenticationStateAsync();
            await returnpath("/");
        }
    }
    private async Task OnRouteDDClick()
    {
        if (_appUser.UserJourney == null)
        {
            await _alertService.ShowErrorAlert(@Localizer["alert"], @Localizer["route_selection_will_be_enabled_once_day_started."]);
        }
        else if (!_navigationManager.Uri.Equals(_navigationManager.BaseUri + "DashBoard", StringComparison.OrdinalIgnoreCase) &&
            !_navigationManager.Uri.Equals(_navigationManager.BaseUri + "MessageOfTheDay", StringComparison.OrdinalIgnoreCase))
        {
            await _alertService.ShowErrorAlert(@Localizer["alert"], @Localizer["please_goto_dashboard_then_only_you_can_change_the_route"]);
        }
        else if (_appUser.StartDayStatus.Equals(StartDayStatus.CONTINUE) && _appUser.IsAllRouteJPRCompleted)
        {
            await _alertService.ShowErrorAlert(@Localizer["alert"], @Localizer["jp_reconciliation_is_completed_for_all_available_route_for_selected_vehicle"]);
        }
        else if (_appUser.StartDayStatus.Equals(StartDayStatus.CONTINUE))
        {
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
    }
    private async Task HandelRouteSelect(DropDownEvent dropDownEvent)
    {
        try
        {
            await _beatHistoryViewModel.OnRouteDD_Select(dropDownEvent);
        }
        catch (CustomException ex)
        {
            await _alertService.ShowErrorAlert(Enum.GetName(ex.Status), ex.Message);
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert(@Localizer["alert"], @Localizer["there_is_some_error_while_processing_request."]);
        }
    }

    /// <summary>
    /// Navigates to the Escalation Matrix page.
    /// </summary>
    private void NavigateToEscalationMatrix()
    {
        _navigationManager.NavigateTo("/escalation-matrix");
    }
}

