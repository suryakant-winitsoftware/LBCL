using Elasticsearch.Net;
using Microsoft.AspNetCore.Components;
using Nest;
using System.Globalization;
using System.Resources;
using Winit.Modules.Common.BL;
using Winit.Modules.JourneyPlan.BL.Interfaces;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.SalesOrder.BL.Interfaces;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

namespace WinIt.Pages.ViewToday_sJourneyPlan
{
    public partial class ViewTodayJourneyPlan
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public bool IsLoaded { get; set; }
        public string SelectedTab = "Assigned";
        public ISelectionItem SelectionItem { get; set; }
        //public List<ISelectionItem> TabSelectionItems = new List<ISelectionItem>
        //{
        // new Winit.Shared.Models.Common.SelectionItem{ Label="Assigned", Code="Assigned", UID="1"},
        // new Winit.Shared.Models.Common.SelectionItem{ Label="UnAssigned", Code="UnAssigned", UID="2"},
        //};
        private List<ISelectionItem> _tabSelectionItems;
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "View Today Journey Plan",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="View Today Journey Plan"},
            }
        };
        public List<ISelectionItem> TabSelectionItems
        {
            get
            {
                if (_tabSelectionItems == null)
                {
                    _tabSelectionItems = new List<ISelectionItem>
        {
            new SelectionItem { Label = @Localizer["assigned"], Code = "Assigned", UID = "1" },
            new SelectionItem { Label = @Localizer["unassigned"], Code = "UnAssigned", UID = "2" },
           
        };
                }
                return _tabSelectionItems;
            }
        }
        private SelectionManager TabSM;
        private bool showFilterComponent = false;
        private Winit.UIComponents.Web.Filter.Filter filterRef;
        public List<FilterModel> ColumnsForFilter;
        public string  OrgUID {get;set;}
        public List<DataGridColumn> DataGridColumns { get; set; }
        public List<DataGridColumn> DataGridColumns1 { get; set; }

        private string SortColumn { get; set; }
        private SortDirection SortDirection1 { get; set; }
        public enum SortDirection
        {
            Ascending,
            Descending
        }
        private void SortByColumn(string columnName)
        {
            if (SortColumn == columnName)
                SortDirection1 = SortDirection1 == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
            else
            {
                SortColumn = columnName;
                SortDirection1 = SortDirection.Ascending;
            }
            SortData();
        }
        private void SortData()
        {
            // Implement sorting logic based on SortColumn and SortDirection
            // You can use LINQ to sort your data
            if (_viewTodayJourneyPlanViewModel.AssignedJourneyPlanList != null)
            {
                var sortedList = _viewTodayJourneyPlanViewModel.AssignedJourneyPlanList;

                switch (SortColumn)
                {
                    case "SalesmanLoginId":
                        sortedList = SortDirection1 == SortDirection.Ascending ?
                            sortedList.OrderBy(item => item.SalesmanLoginId).ToList() :
                            sortedList.OrderByDescending(item => item.SalesmanLoginId).ToList();
                        break;
                    case "SalesmanName":
                        sortedList = SortDirection1 == SortDirection.Ascending ?
                            sortedList.OrderBy(item => item.SalesmanName).ToList() :
                            sortedList.OrderByDescending(item => item.SalesmanName).ToList();
                        break;
                    case "RouteName":
                        sortedList = SortDirection1 == SortDirection.Ascending ?
                            sortedList.OrderBy(item => item.RouteName).ToList() :
                            sortedList.OrderByDescending(item => item.RouteName).ToList();
                        break;
                    case "VehicleName":
                        sortedList = SortDirection1 == SortDirection.Ascending ?
                            sortedList.OrderBy(item => item.VehicleName).ToList() :
                            sortedList.OrderByDescending(item => item.VehicleName).ToList();
                        break;
                    case "ScheduleCall":
                        sortedList = SortDirection1 == SortDirection.Ascending ?
                            sortedList.OrderBy(item => item.ScheduleCall).ToList() :
                            sortedList.OrderByDescending(item => item.ScheduleCall).ToList();
                        break;
                    case "ActualStoreVisits":
                        sortedList = SortDirection1 == SortDirection.Ascending ?
                            sortedList.OrderBy(item => item.ActualStoreVisits).ToList() :
                            sortedList.OrderByDescending(item => item.ActualStoreVisits).ToList();
                        break;
                    case "PendingVisits":
                        sortedList = SortDirection1 == SortDirection.Ascending ?
                            sortedList.OrderBy(item => item.PendingVisits).ToList() :
                            sortedList.OrderByDescending(item => item.PendingVisits).ToList();
                        break;
                }

                _viewTodayJourneyPlanViewModel.AssignedJourneyPlanList = sortedList;
            }
        }
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            FilterInitialized();
            TabSM = new SelectionManager(TabSelectionItems, SelectionMode.Single);
            TabSelectionItems[0].IsSelected = true;
             OrgUID = _iAppUser.SelectedJobPosition.OrgUID;
           // OrgUID = "FR001";
            await _viewTodayJourneyPlanViewModel.GetSalesman(OrgUID);
            await _viewTodayJourneyPlanViewModel.GetRoute(OrgUID);
            await _viewTodayJourneyPlanViewModel.GetVehicle(OrgUID);
            await GetDataLoadAsync();
            IsLoaded = true;

            //await GenerateGridColumnsAssigned();
            //await GenerateGridColumnsUnAssigned();
           // await SetHeaderName();
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
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["salesman"], DropDownValues=_viewTodayJourneyPlanViewModel.EmpSelectionList,   ColumnName="EmpUID",SelectionMode=SelectionMode.Multiple},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["route"] ,DropDownValues=_viewTodayJourneyPlanViewModel.RouteSelectionList, ColumnName="RouteName",SelectionMode=SelectionMode.Multiple},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["vehicle"],DropDownValues=_viewTodayJourneyPlanViewModel.VehicleSelectionList, ColumnName="VehicleName",SelectionMode=SelectionMode.Multiple}
        };
        }
       
        private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
            foreach (var keyValue in filterCriteria)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                    if(keyValue.Key.Equals("EmpUID"))
                    {
                        if (keyValue.Value.Contains(","))
                        {
                            string[] values = keyValue.Value.Split(',');
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", values, FilterType.In));
                        }
                        else
                        {
                            ISelectionItem? selectionItem = _viewTodayJourneyPlanViewModel.EmpSelectionList.Find(e => e.UID == keyValue.Value);
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.UID, FilterType.Equal));
                        }
                    }
                    else if (keyValue.Key == "RouteName")
                    {
                        List<string> selectedUids = keyValue.Value.Split(",").ToList();
                        List<string> seletedLabels = _viewTodayJourneyPlanViewModel.RouteSelectionList.Where(e => selectedUids.Contains(e.UID)).Select(_ => _.Label).ToList();
                        filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", seletedLabels, FilterType.In));
                    }
                    else
                    {
                        
                            List<string> selectedUids = keyValue.Value.Split(",").ToList();
                            List<string> seletedLabels = _viewTodayJourneyPlanViewModel.VehicleSelectionList.Where(e => selectedUids.Contains(e.UID)).Select(_ => _.Label).ToList();
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", seletedLabels, FilterType.In));
                        
                        //else
                        //{
                        //    filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Equal));
                        //}
                    }
                }
            }
            await _viewTodayJourneyPlanViewModel.ApplyFilter(filterCriterias,SelectedTab);
        }
        public async Task OnViewClick(IAssignedJourneyPlan assignedJourneyPlan)
        {
            if (!assignedJourneyPlan.IsChildgridOpen)
            {
                await _viewTodayJourneyPlanViewModel.GetInnerGridviewData(assignedJourneyPlan);
            }
            assignedJourneyPlan.IsChildgridOpen = !assignedJourneyPlan.IsChildgridOpen;
            // Refresh the UI
            StateHasChanged();
        }
        //public async Task SetHeaderName()
        //{
        //    _IDataService.BreadcrumList = new();
        //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["view_today's_journey_plan"] , IsClickable = false });
        //    _IDataService.HeaderText = @Localizer["view_today's_journey_plan"];
        //    await CallbackService.InvokeAsync(_IDataService);
        //}
        //public async void OnTabSelect(ISelectionItem selectionItem)
        //{
        //    ShowLoader();
        //        if (!selectionItem.IsSelected)
        //        {
        //            TabSM.Select(selectionItem);
        //            SelectedTab = selectionItem.Code;
        //            InvokeAsync(async () =>
        //            {
        //                await GetDataLoadAsync();
        //                StateHasChanged();
        //            });
        //        }
        //    // await GenerateOuterGridColumns();
        //    HideLoader();
        //}

        public async Task OnTabSelect(ISelectionItem selectionItem)
        {
            try
            {
                ShowLoader();
                if (!selectionItem.IsSelected)
                {
                    TabSM.Select(selectionItem);
                    SelectedTab = selectionItem.Code;

                    // Clear the existing data before loading new data
                    _viewTodayJourneyPlanViewModel.AssignedJourneyPlanList = new List<IAssignedJourneyPlan>();
                    StateHasChanged(); // This will clear the grid immediately

                    await GetDataLoadAsync();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                await _AlertMessgae.ShowErrorAlert("Error", ex.Message);
            }
            finally
            {
                HideLoader();
            }
        }
        public async Task GetDataLoadAsync()
        {
            await _viewTodayJourneyPlanViewModel.PopulateViewModel(SelectedTab);
        }
        private async Task AddJourneyPlan()
        {
            _navigationManager.NavigateTo($"routemanagement");
        }
      

        //private async Task GenerateGridColumnsAssigned()
        //{
          
        //        DataGridColumns = new List<DataGridColumn>
        //    {
        //        new DataGridColumn { Header = "Sales Rep Login ID", GetValue = s => ((IAssignedJourneyPlan)s)?.SalesmanLoginId ?? "N/A" },
        //        new DataGridColumn { Header = "Sales Rep Name", GetValue = s => ((IAssignedJourneyPlan)s)?.SalesmanName?? "N/A" },
        //        new DataGridColumn { Header = "Route", GetValue = s => ((IAssignedJourneyPlan)s)?.RouteName ?? "N/A" },
        //        new DataGridColumn { Header = "Vehicle", GetValue = s => ((IAssignedJourneyPlan)s)?.VehicleName ?? "N/A" },
        //        new DataGridColumn { Header = "Schedule Call", GetValue = s => ((IAssignedJourneyPlan)s)?.ScheduleCall ?? 0 },
        //        new DataGridColumn { Header = "Visited", GetValue = s => ((IAssignedJourneyPlan)s)?.ActualStoreVisits ?? 0 },
        //        new DataGridColumn { Header = "Pending", GetValue = s => ((IAssignedJourneyPlan)s)?.PendingVisits ?? 0 },

        //     new DataGridColumn
        //     {
        //        Header = "Actions",
        //        IsButtonColumn = true,
        //        ButtonActions = new List<ButtonAction>
        //        {

        //               new ButtonAction
        //              {
        //                ButtonType = ButtonTypes.Image,
        //                URL = "https://qa-fonterra.winitsoftware.com/assets/Images/view.png",
        //                Action = s =>  OnViewClick((IAssignedJourneyPlan)s)
        //              }
        //        }
        //    }
        //     };
        //    }
        //private async Task GenerateGridColumnsUnAssigned()
        //{

        //    DataGridColumns1 = new List<DataGridColumn>
        //    {
        //        new DataGridColumn { Header = "Sales Rep Login ID", GetValue = s => ((IAssignedJourneyPlan)s)?.SalesmanLoginId ?? "N/A" },
        //        new DataGridColumn { Header = "Sales Rep Name", GetValue = s => ((IAssignedJourneyPlan)s)?.SalesmanName?? "N/A" },
        //        new DataGridColumn { Header = "Route", GetValue = s => ((IAssignedJourneyPlan)s)?.RouteName ?? "N/A" },
        //        new DataGridColumn { Header = "Vehicle", GetValue = s => ((IAssignedJourneyPlan)s)?.VehicleName ?? "N/A" },
        //     };
        //}
    }
}
    


