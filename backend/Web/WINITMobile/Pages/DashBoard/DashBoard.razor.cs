using Microsoft.AspNetCore.Components;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Shared.Models.Common;
using WINITMobile.Models.TopBar;
using Microsoft.JSInterop;
using Microsoft.Maui.ApplicationModel;
using Winit.Modules.JobPosition.Model.Interfaces;
using Winit.Modules.JobPosition.Model.Classes;

namespace WINITMobile.Pages.DashBoard;

public partial class DashBoard
{
    [CascadingParameter] public EventCallback<MainButtons> Btnname { get; set; }
    [Parameter] public EventCallback<bool> CloseMenu { get; set; }
    private bool showPopup = false;
    void ClosePopup() => showPopup = false;
    void TogglePopup() => showPopup = !showPopup;
    private bool showPopupBrandTraining = false;
    void ClosePopupBrandTraining() => showPopupBrandTraining = false;
    void TogglePopupBrandTraining() => showPopupBrandTraining = !showPopupBrandTraining;
    public IJobPositionAttendance jobPositionAttendance { get; set; } = new JobPositionAttendance();
    protected override async Task OnInitializedAsync()
    {
       
        try
        {
            await InvokeAsync(async () =>
            {
                ShowLoader();
                await SetTopBar();
                _backbuttonhandler.SetCurrentPage(this);
                _appUser.CurrentUserDate = DateTime.Now;
                await _userConfig.SetUserJourney();
                await _appUser.SetStartDayAction();
                _dashBoardViewModel.StartDayBtnText = _appUser.StartDayStatus;
                await GetTotalAssignedAndVisitedStores();
                //await SetTopBar();
                //await LoadSettingMaster();
                //await GetUserJourney();
                //await GetSelectedRoute();
                //await GetSelectedBeatHistory();
                //await SelectedJobPosition();
                //await GetSetOrgCurrencyList();
                await GetReasonsFromListHeader();
                await _beatHistoryViewModel.Load();
                LoadResources(null, _languageService.SelectedCulture);
                //await GetUserJourney();
                await GetUserAttendence();
                Task t = _dashBoardViewModel.PopulateCacheData();
                HideLoader();
            });
        }
        catch (Exception)
        {
            HideLoader();
        }
    }
    protected async Task GetTotalAssignedAndVisitedStores()
    {
       jobPositionAttendance = await _dashBoardViewModel.GetTotalAssignedAndVisitedStores();
    }
    
    protected async Task GetUserAttendence()
    {
       await _dashBoardViewModel.GetUserAttendence(_appUser.SelectedJobPosition.UID,_appUser.Emp.UID);
    }
    public async Task returnpath(string path, bool isJourneyCheckRequired = false)
    {
        //_navigationManager.NavigateTo(path, true);
        _backbuttonhandler.ClearCurrentPage();

        if(isJourneyCheckRequired)
        {
            MoveAfterJourneyCheck(path);
        }
        else
        {
            NavigateTo(path);
        }            
    }

    private async Task OpenRazorpayDashboard()
    {
        try
        {
            await Launcher.OpenAsync(new Uri("https://payroll.razorpay.com/dashboard"));
        }
        catch (FeatureNotSupportedException ex)
        {
            await _alertService.ShowErrorAlert(@Localizer["alert"], "This feature is not supported on your device.");
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert(@Localizer["alert"], "Unable to open the external link. Please try again.");
        }
    }
    private async Task SetTopBar()
    {
        MainButtons buttons = new()
        {
            UIButton1 = new Buttons()
            {
               
            }
           
        };
        await Btnname.InvokeAsync(buttons);
    }
    public void OnStartDayClick()
    {
        bool validationStatus = ValidatePage();
        if (!validationStatus)
        {
            return;            
        }
        //_backbuttonhandler.ClearCurrentPage();
        _dashBoardViewModel.StartDayInitiate();
    }
    private bool ValidatePage()
    {
        // later handle based on role
        //if(_appUser.Role.Code == "Presales")
        //if (_appUser.Vehicle == null)
        //{
        //    // Vehicle data missing
        //    _alertService.ShowErrorAlert("Error", "Vehicle data is missing");
        //    return false;
        //}
        return true;
    }
    private string GetAttendanceColor(decimal attendancePercentage)
    {
        return attendancePercentage >= 75 ? "#4CAF50" : "#E74F48"; 
    }

    public async Task GetReasonsFromListHeader()
    {
      
            IEnumerable<IListItem> forceCheckInItems = await _listheaderBL.GetListItemsByHeaderUID("FORCE_CHECKIN");
            IEnumerable<IListItem> skipReasonItems = await _listheaderBL.GetListItemsByHeaderUID("SKIP_REASON");
            IEnumerable<IListItem> zerosalesReasonItems = await _listheaderBL.GetListItemsByHeaderUID("ZERO_SALES");
            IEnumerable<IListItem> remoteCollectionReasonItems = await _listheaderBL.GetListItemsByHeaderUID("REMOTE_COLLECTION_REASON");
            IEnumerable<IListItem> paymentCheckListReasonItems = await _listheaderBL.GetListItemsByHeaderUID("PAYMENT_CHECK_LIST");


            _appUser.ReasonDictionary = new Dictionary<string, List<ISelectionItem>>();

            // Iterate over the forceCheckInItems and convert each to an ISelectionItem
            foreach (IListItem listItem in forceCheckInItems)
            {
                AddSelectionItemToReasonDictionary(listItem);
            }

            // Iterate over the skipReasonItems and convert each to an ISelectionItem
            foreach (IListItem listItem in skipReasonItems)
            {
                AddSelectionItemToReasonDictionary(listItem);
            }

            // Iterate over the zerosalesReasonItems and convert each to an ISelectionItem
            foreach (IListItem listItem in zerosalesReasonItems)
            {
                AddSelectionItemToReasonDictionary(listItem);
            }

            // Iterate over the remoteCollectionReasonItems and convert each to an ISelectionItem
            foreach (IListItem listItem in remoteCollectionReasonItems)
            {
                AddSelectionItemToReasonDictionary(listItem);
            }

            // Iterate over the paymentCheckListReasonItems and convert each to an ISelectionItem
            foreach (IListItem listItem in paymentCheckListReasonItems)
            {
                AddSelectionItemToReasonDictionary(listItem);
            }
        
    }
    private void AddSelectionItemToReasonDictionary(IListItem listItem)
    {
        ISelectionItem selectionItem = new SelectionItem
        {
            UID = listItem.UID,
            Code = listItem.Code,
            Label = listItem.Name,
            IsSelected = false // You may need to adjust this based on your requirements
        };

        // Check if the ReasonDictionary already contains the ListHeaderUID
        if (_appUser.ReasonDictionary.ContainsKey(listItem.ListHeaderUID))
        {
            // Add the selectionItem to the existing list
            _appUser.ReasonDictionary[listItem.ListHeaderUID].Add(selectionItem);
            //_appUser.ReasonDictionary[listItem.ListHeaderUID] = _appUser.ReasonDictionary[listItem.ListHeaderUID]
            //                                                    .OrderBy(item => (ListItem)item.SerialNo)
            //                                                    .ToList();
        }
        else
        {
            // Create a new list and add the selectionItem
            _appUser.ReasonDictionary[listItem.ListHeaderUID] = new List<ISelectionItem> { selectionItem };
        }
    }
    public void TriggerClientSideError()
    {
        throw new Exception("This is test exception");
    }

    public override async Task OnBackClick()
    {
        if (await _alertService.ShowConfirmationReturnType(@Localizer["confirm?"], @Localizer["are_you_want_to_logout?"], @Localizer["yes"], @Localizer["no"]))
        {
            //_backbuttonhandler.ClearCurrentPage();
            _navigationManager.NavigateTo("/");
        }
    }
}
