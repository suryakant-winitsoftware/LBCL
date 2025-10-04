using Microsoft.AspNetCore.Components;
using Nest;
using System.Globalization;
using System.Resources;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Modules.SalesOrder.BL.Interfaces;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

namespace WinIt.Pages.UserJourney_AttendanceReport
{
    public partial class UserJourney_AttendanceReport
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public List<DataGridColumn> DataGridColumns { get; set; }
        public bool IsLoaded { get; set; }
        private bool showFilterComponent = false;
        private Winit.UIComponents.Web.Filter.Filter filterRef;
        public List<FilterModel> ColumnsForFilter;
        public List<ISelectionItem> EotStatusSelectionItems = new List<ISelectionItem>
        {
         new SelectionItem{ Label="Completed", Code="Completed", UID="Completed"},
         new SelectionItem{ Label="Pending", Code="Pending", UID="Pending"},
        };
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "User Journey & Attendance Report",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="User Journey & Attendance Report"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            FilterInitialized();
            _userJourneyViewModel.PageSize = 5;
            _userJourneyViewModel.OrgUID = _iAppUser.SelectedJobPosition.OrgUID;
           // _userJourneyViewModel.OrgUID = "FR001";
            await _userJourneyViewModel.GetSalesman(_userJourneyViewModel.OrgUID);
            await _userJourneyViewModel.PopulateViewModel();
            await GenerateGridColumnsForUserJourneyDetails();
            IsLoaded = true;
            //await SetHeaderName();
        }
       
        public async void ShowFilter()
        {
            showFilterComponent = !showFilterComponent;
            filterRef.ToggleFilter();
        }
        public void FilterInitialized()
        {
            ColumnsForFilter = new List<FilterModel>
        {
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label =  @Localizer["salesman"], DropDownValues=_userJourneyViewModel.EmpSelectionList,ColumnName = "User"},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label =  @Localizer["journey_start_date"],ColumnName = "StartTime"},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label =  @Localizer["journey_end_date"],ColumnName = "EndTime"},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["eot_status"] , DropDownValues=EotStatusSelectionItems,  ColumnName = "EOTStatus",SelectionMode=SelectionMode.Multiple},

        };
        }
        private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
            foreach (var keyValue in filterCriteria)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                    if (keyValue.Key == "EOTStatus")
                    {
                        if (keyValue.Value.Contains(","))
                        {
                            string[] values = keyValue.Value.Split(',');
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", values, FilterType.In));
                        }
                        else
                        {
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Equal));
                        }
                    }
                    else if (keyValue.Key == "User")
                    {
                        ISelectionItem? selectionItem = _userJourneyViewModel.EmpSelectionList.Find(e => e.UID == keyValue.Value);
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.Code, FilterType.Equal));
                    }
                    else
                    {
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Equal));
                    }
                }
            }
            await _userJourneyViewModel.ApplyFilter(filterCriterias);
            StateHasChanged();
        }
        //public async Task SetHeaderName()
        //{
        //    _IDataService.BreadcrumList = new();
        //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["user_journey_&_attendance_report"] , IsClickable = false });
        //    _IDataService.HeaderText = @Localizer["user_journey_&_attendance_report"];
        //    await CallbackService.InvokeAsync(_IDataService);
        //}
        private async Task GenerateGridColumnsForUserJourneyDetails()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = @Localizer["user"], GetValue = s => ((IUserJourneyGrid)s)?.User ?? "N/A",IsSortable = true, SortField = "User" },
                new DataGridColumn { Header = @Localizer["journey_date"], GetValue = s => Winit.Shared.CommonUtilities.Common.CommonFunctions.GetDateTimeInFormat(((IUserJourneyGrid)s)?.JourneyDate),IsSortable = true, SortField = "JourneyDate"},
                new DataGridColumn { Header = @Localizer["eot_status"], GetValue = s => ((IUserJourneyGrid)s)?.EOTStatus ?? "N/A",IsSortable = true, SortField = "EOTStatus" },
                new DataGridColumn { Header = @Localizer["start_time"], GetValue = s => Winit.Shared.CommonUtilities.Common.CommonFunctions.GetDateTimeInFormat(((IUserJourneyGrid)s)?.StartTime,"dd MMM, yyyy HH:mm"),IsSortable = true, SortField = "StartTime"},
                new DataGridColumn { Header = @Localizer["end_time"], GetValue = s => Winit.Shared.CommonUtilities.Common.CommonFunctions.GetDateTimeInFormat(((IUserJourneyGrid) s) ?.EndTime,"dd MMM, yyyy HH:mm"),IsSortable = true, SortField = "EndTime"},
                
            new DataGridColumn
             {
                Header =@Localizer["actions"],
                IsButtonColumn = true,
                //ButtonActions = this.buttonActions
                ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                             ButtonType = ButtonTypes.Image,
                             URL = "https://qa-fonterra.winitsoftware.com/assets/Images/view.png",
                            Action = item => OnViewClick((IUserJourneyGrid)item),
                        }
                     }
            }
             };
        }
        private void OnViewClick(IUserJourneyGrid userJourney)
        {
            _navigationManager.NavigateTo($"UserJourney_AttendanceReportDetails?UID={userJourney.UID}");
        }
        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _userJourneyViewModel.ApplySort(sortCriteria);
            StateHasChanged();
            HideLoader();
        }
    }
}
