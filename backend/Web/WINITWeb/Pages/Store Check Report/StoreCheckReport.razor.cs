using Microsoft.AspNetCore.Components;
using Nest;
using System.Globalization;
using System.Resources;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
using static System.Net.WebRequestMethods;
using Winit.Modules.Store.BL.Classes;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System;

namespace WinIt.Pages.Store_Check_Report
{
    public partial class StoreCheckReport
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public List<DataGridColumn> DataGridColumns { get; set; }
        public bool IsLoaded { get; set; }
        private bool showFilterComponent = false;
        private Winit.UIComponents.Web.Filter.Filter filterRef;
        public List<FilterModel> ColumnsForFilter;
        private bool showPopup = false;
        protected Winit.Modules.Store.Model.Classes.StoreCheckReport PopupReport { get; set; }
        protected string PopupSearchText { get; set; }

        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Store Check Report",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Store Check Report"},
            }
        };

        protected IEnumerable<IStoreCheckReportItem> FilteredStoreCheckReportItems
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PopupSearchText))
                    return _storeCheckReportViewModel?.StoreCheckReportsitem;

                return _storeCheckReportViewModel?.StoreCheckReportsitem
                    .Where(item =>
                        (!string.IsNullOrEmpty(item.SKU) && item.SKU.Contains(PopupSearchText, StringComparison.OrdinalIgnoreCase))
                    );
            }
        }

        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            FilterInitialized();
            _storeCheckReportViewModel.PageSize = 5;
            _storeCheckReportViewModel.OrgUID = _iAppUser.SelectedJobPosition.OrgUID;
            // _userJourneyViewModel.OrgUID = "FR001";
            await _storeCheckReportViewModel.GetSalesmanList(_storeCheckReportViewModel.OrgUID);
            await _storeCheckReportViewModel.GetCustomerList(_storeCheckReportViewModel.OrgUID);
            await _storeCheckReportViewModel.GetRouteList(_storeCheckReportViewModel.OrgUID);
            await _storeCheckReportViewModel.PopulateViewModel();
            await GenerateGridColumnsForStoreCheckReport();
            IsLoaded = true;
            HideLoader();
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
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = "Route", ColumnName = "RouteCode",DropDownValues=_storeCheckReportViewModel.RouteSelectionList },
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = "Salesman", ColumnName = "SalesmanCode",DropDownValues=_storeCheckReportViewModel.SalesmanSelectionList },
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = "Customer", ColumnName = "CustomerCode",DropDownValues=_storeCheckReportViewModel.CustomerSelectionList },
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label = "StartDate", ColumnName = "StartDate" },
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label = "EndDate", ColumnName = "EndDate" }
            };
        }

        public async Task OnFilterApply(IDictionary<string, string> filterCriteria)
        {
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();

            foreach (var keyValue in filterCriteria)
            {
                if (!string.IsNullOrEmpty(keyValue.Value))
                {
                    // Handle date range filtering
                    if (keyValue.Key == "StartDate")
                    {
                        if (DateTime.TryParse(keyValue.Value, out DateTime parsedStartDate))
                        {
                            filterCriterias.Add(new FilterCriteria("Date", parsedStartDate.ToString("yyyy-MM-dd"), FilterType.GreaterThanOrEqual));
                        }
                    }
                    else if (keyValue.Key == "EndDate")
                    {
                        if (DateTime.TryParse(keyValue.Value, out DateTime parsedEndDate))
                        {
                            filterCriterias.Add(new FilterCriteria("Date", parsedEndDate.ToString("yyyy-MM-dd"), FilterType.LessThanOrEqual));
                        }
                    }
                    else if (keyValue.Key == "CustomerCode")
                    {
                        ISelectionItem? selectionItem = _storeCheckReportViewModel.CustomerSelectionList
                            .Find(e => e.UID == keyValue.Value);

                        if (selectionItem != null)
                        {
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.Code, FilterType.Like));
                        }
                    }
                    else if (keyValue.Key == "SalesmanCode")
                    {
                        ISelectionItem? selectionItem = _storeCheckReportViewModel.SalesmanSelectionList
                            .Find(e => e.UID == keyValue.Value);

                        if (selectionItem != null)
                        {
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.Code, FilterType.Like));
                        }
                    }
                    else if (keyValue.Key == "RouteCode")
                    {
                        ISelectionItem? selectionItem = _storeCheckReportViewModel.RouteSelectionList
                            .Find(e => e.UID == keyValue.Value);

                        if (selectionItem != null)
                        {
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", selectionItem.Code, FilterType.Like));
                        }
                    }
                    else
                    {
                        filterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value, FilterType.Like));
                    }
                }
            }
            await _storeCheckReportViewModel.ApplyFilter(filterCriterias);
        }
        private async Task GenerateGridColumnsForStoreCheckReport()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Route", GetValue = s => ((IStoreCheckReport)s)?.RouteCode ?? "N/A", IsSortable = true, SortField = "RouteCode" },
                new DataGridColumn { Header = "Salesman", GetValue = s => ((IStoreCheckReport)s)?.SalesmanCode ?? "N/A", IsSortable = true, SortField = "SalesmanCode" },
                new DataGridColumn { Header = "Customer", GetValue = s => ((IStoreCheckReport)s)?.CustomerCode ?? "N/A", IsSortable = true, SortField = "CustomerCode" },
                new DataGridColumn {
    Header = "Image",
    GetValue = s => string.IsNullOrEmpty(((IStoreCheckReport)s)?.ImagePath) ? null : (_appConfigs.ApiDataBaseUrl + ((IStoreCheckReport)s)?.ImagePath),
    IsSortable = false,ColumnType=ColumnTypes.Image
},

                new DataGridColumn { Header = "Date", GetValue = s => ((IStoreCheckReport)s)?.Date , IsSortable = true, SortField = "Date" },
               // new DataGridColumn { Header = "StartDate", GetValue = s => Winit.Shared.CommonUtilities.Common.CommonFunctions.GetDateTimeInFormat(((IStoreCheckReport)s)?.Date), IsSortable = true, SortField = "Date" },
                new DataGridColumn
                {
                    Header = "Actions",
                    IsButtonColumn = true,
                    ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                            ButtonType = ButtonTypes.Image,
                            URL = "https://qa-fonterra.winitsoftware.com/assets/Images/view.png",
                            Action = item => OnViewItem((IStoreCheckReport)item),
                        }
                    }
                }
            };
        }
        private async Task OnViewItem(IStoreCheckReport storeCheck)
        {
            ShowLoader();
            showPopup = true;
            PopupReport = storeCheck as Winit.Modules.Store.Model.Classes.StoreCheckReport;
            await ((StoreCheckReportWebViewModel)_storeCheckReportViewModel).GetStoreCheckReportItemsAsync(PopupReport.UID);
            HideLoader();
            StateHasChanged();
        }

        protected void ClosePopup()
        {
            showPopup = false;
            PopupReport = null;
            PopupSearchText = null;
        }

        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _storeCheckReportViewModel.ApplySort(sortCriteria);
            StateHasChanged();
            HideLoader();
        }
    }
}

// Method to open popup and set data