using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.StoreActivity.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.UIComponents.Common.Services;
using WINITMobile.Models.TopBar;
namespace WINITMobile.Pages.CheckIn;

public partial class CustomerCall
{
    // this flag for checking this page is opened or not
    public bool IsVisible { get; private set; }
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    TimeSpan RunningTime = TimeSpan.Zero;
    public bool isRunning { get; set; }
    [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }
    public string RoleUID { get; set; }
    public string StoreHistoryUID { get; set; }
    protected string StoreActivityHistoryUid { get; set; } = "";
    public List<IStoreActivityItem> storeActivityItems { get; set; }

    private Winit.Modules.Store.Model.Interfaces.IStoreItemView SelectedStoreItemView;
    public Winit.Modules.JourneyPlan.BL.Interfaces.IJourneyPlanViewModel _journeyplanviewmodel { get; set; }
    protected override async void OnInitialized()
    {
        _journeyplanviewmodel = _journeyplanBaseViewMobile.CreateJourneyPlanViewModel("DefaultObject");
        //CleanDataManager("salesOrderViewModel");
        if (_appUser != null && _appUser.SelectedCustomer != null)
        {
            _appUser.IsCustomerCallPage = true;
            RoleUID = _appUser.Role?.UID;
        }
        //await ShowingTime();
        _stopwatchService.OnTimeUpdated += StateHasChanged;
        StartTimer();

    }
    protected async override Task OnInitializedAsync()
    {
        await SetTopBar();
        _backbuttonhandler.SetCurrentPage(this);
        //SelectedStoreItemView = (Winit.Modules.Store.Model.Interfaces.IStoreItemView)_dataManager.GetData("SelectedStoreViewModel");
        //if (_appUser != null && _appUser.SelectedCustomer != null)
        //{
        //    SelectedStoreItemView = _appUser.SelectedCustomer;
        //    _dataManager.SetData(nameof(Winit.Modules.Store.Model.Interfaces.IStoreItemView),SelectedStoreItemView);
        //}
        dynamic obj = _dataManager.GetData(nameof(Winit.Modules.Store.Model.Interfaces.IStoreItemView));
        if (obj != null)
        {
            SelectedStoreItemView = obj;
            StoreHistoryUID = SelectedStoreItemView.StoreHistoryUID;
            _appUser.SelectedCustomer = SelectedStoreItemView;
        }
        _sideBarService.OnCheckOutClick = async () => await HandleCheckOutClicked();

        await GetStoreActivities();
        LoadResources(null, _languageService.SelectedCulture);
        StateHasChanged();
    }
    void StartTimer()
    {
        if (!_stopwatchService.IsRunning)
        {
            _stopwatchService.StartTimer();
        }
        //SelectedStoreItemView.CheckInTime = _stopwatchService.StartTime;
    }
    void StopTimer()
    {
        _stopwatchService.ResetTimer();
    }
    protected async Task ShowingTime()
    {
        isRunning = true;
        stopwatch.Start();
        await UpdateTime();
    }
    protected async Task UpdateTime()
    {
        while (isRunning)
        {
            RunningTime = stopwatch.Elapsed;
            await Task.Delay(1000);
            StateHasChanged();
        }
    }
    protected void OnDispose()
    {
        _stopwatchService.OnTimeUpdated -= StateHasChanged;
    }
    protected async Task SetTopBar()
    {
        WINITMobile.Models.TopBar.MainButtons buttons = new WINITMobile.Models.TopBar.MainButtons()
        {
            TopLabel = @Localizer["customer_call_procedure"],

            UIButton1 = null,

        };
        await Btnname.InvokeAsync(buttons);
    }
    protected async Task GetStoreActivities()
    {
        IEnumerable<IStoreActivityItem> items = await _Viewmodel.GetAllStoreActivities(RoleUID, StoreHistoryUID);
        storeActivityItems = items.OrderBy(item => item.SerialNo).ToList();
    }
    public async Task Handleback()
    {
        // 
    }
    protected async Task HandleOnClickStoreItem(string NavPath, string StoreActivityHistoryUid)
    {
        _appUser.IsCustomerCallPage = false;
        await SetStoreActivityHistoryUid(StoreActivityHistoryUid);
        NavigateTo(NavPath);
    }
    protected async Task SetStoreActivityHistoryUid(string StoreActivityHistoryUid)
    {
        _dataManager.SetData("StoreActivityHistoryUid", StoreActivityHistoryUid);
        await Task.CompletedTask;
    }
    public override async Task OnBackClick()
    {
        //_backbuttonhandler.ClearCurrentPage();
        await HandleCheckOutClicked();
    }

    // THIS FOR JOURNEY PLAN 
    public async Task HandleCheckOutClicked()
    {
        await OnUploadDataClick();
        //if (await _alertService.ShowConfirmationReturnType("Confirm", "Are you sure you want to do Upload data to Server?"))
        //{
        //    await OnUploadDataClick();
        //}
        //_journeyplanviewmodel = _viewmodel._viewmodelJp;
        //dynamic obj = _dataManager.GetData(nameof(Winit.Modules.Store.Model.Interfaces.IStoreItemView));
        //if (obj != null)
        //{
        //    SelectedStoreItemView = obj;
        //    //_appUser.SelectedCustomer = SelectedStoreItemView;
        //}
        if (_journeyplanviewmodel.UserJourney == null)
        {
            _journeyplanviewmodel.UserJourneyUID = _appUser.UserJourney.UID;
        }
        // Show confirmation dialog
        bool result = await ShowCheckOutConfirmationDialog();

        if (result)
        {
            //await OnUploadDataClick();
            // Update data for checkout
            await UpdateDataForCheckOut();

            // Update beat history
            await UpdateBeatHistory();
            StopTimer();

            //commented as for now
            //NavigateToJourneyPlan();


            NavigateToCustomersList();

            // commited on jan 5th 2025 by niranjan
            //if (SelectedStoreItemView != null && SelectedStoreItemView.ActualLines == 0)
            //{
            //    // Handle zero sales scenario
            //    await HandleZeroSales();
            //}
            //else
            //{
            //    // Navigate to journey plan
            //    NavigateToJourneyPlan();
            //}
        }
        else
        {
            // for Handle cancellation
            HandleCancellation();
        }
    }
    /// <summary>
    /// Handles the upload data click event during checkout flow.
    /// Uses the reusable HandleCurrentUserDataUpload method.
    /// </summary>
    private async Task OnUploadDataClick()
    {
        var tableGroups = new List<string>
        {
            DbTableGroup.Master,
            DbTableGroup.SurveyResponse,
            DbTableGroup.FileSys
        };
        
        // Use the reusable method from base class (no alerts since this is part of checkout flow)
        var result = await UploadDataSilent(tableGroups);

        // Handle errors in checkout context
        if (!result.Success)
        {
            await _alertService.ShowErrorAlert("Upload Error", 
                $"Failed to upload data during checkout: {result.ErrorMessage}");
        }
    }
    private async Task<bool> ShowCheckOutConfirmationDialog()
    {
        return await _alertService.ShowConfirmationReturnType("Check_Out", "Are you sure you want to do checkout?");
    }

    private async Task UpdateDataForCheckOut()
    {
        if (_journeyplanviewmodel != null)
        {
            SelectedStoreItemView.CheckOutTime = DateTime.Now;
            await _journeyplanviewmodel.PrepareDBForCheckout(SelectedStoreItemView);
        }
    }

    private async Task UpdateBeatHistory()
    {
        if (_journeyplanviewmodel != null)
        {
            await _journeyplanviewmodel.UpdateBeathistory(_appUser.SelectedBeatHistory.UID);
        }
    }

    private async Task HandleZeroSales()
    {
        List<ISelectionItem> reasonsForZeroSales = _appUser.ReasonDictionary["ZERO_SALES"];
        await _dropdownService.ShowDropDown(new DropDownOptions
        {
            DataSource = reasonsForZeroSales,
            OnSelect = async (eventArgs) =>
            {
                await SelectForZero_Sales(eventArgs);
            },
            Title = "Zero Sales"
        });
    }

    private void NavigateToJourneyPlan()
    {
        _appUser.IsCheckedIn = false;
        _appUser.IsCustomerCallPage = false;
        _appUser.SelectedCustomer = new StoreItemView();
        _sideBarService.RefreshSideBar.Invoke();

        //_navigationManager.NavigateTo("JourneyPlan", replace: true);
        NavigateTo("JourneyPlan");
    }
    
    private void NavigateToCustomersList()
    {
        _appUser.IsCheckedIn = false;
        _appUser.IsCustomerCallPage = false;
        _appUser.SelectedCustomer = new StoreItemView();
        _sideBarService.RefreshSideBar.Invoke();

        //_navigationManager.NavigateTo("JourneyPlan", replace: true);
        NavigateTo("CustomerList");
    }

    private void HandleCancellation()
    {
        return;
    }

    public async Task SelectForZero_Sales(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
    {
        try
        {
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null)
                {
                    ISelectionItem selectionItem = dropDownEvent.SelectionItems.FirstOrDefault();
                    _journeyplanviewmodel.Zero_Sales_Reason = selectionItem.Label;

                    // call base view model
                    int res = await _journeyplanviewmodel.UpdateStatusInStoreHistory(SelectedStoreItemView.StoreHistoryUID, Winit.Shared.Models.Constants.StoryHistoryStatus.VISITED);
                    if (res >= 1)
                    {
                        SelectedStoreItemView.CurrentVisitStatus = Winit.Shared.Models.Constants.StoryHistoryStatus.VISITED;
                        // _navigationManager.NavigateTo("JourneyPlan");
                        NavigateToJourneyPlan();
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
    }
}
