using Microsoft.AspNetCore.Components.Routing;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Logging;
using Winit.Modules.Common.BL;
using Winit.Modules.Common.UIState.Classes;
using Winit.Modules.PurchaseOrder.Model.Constatnts;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.Tax.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;

namespace WinIt.Pages.PurchaseOrder;

public partial class ViewPurchaseOrderSatus : IDisposable
{
    private List<FilterModel> FilterColumns = [];
    private Dictionary<string, string> _currentFilters = new Dictionary<string, string>();
    private EventHandler<LocationChangedEventArgs> _locationChangedHandler;
    private string PreviousUrl = string.Empty;
    private readonly List<ISelectionItem> TabSelectionItems =
    [
        new SelectionItemTab
        {
            Label = "ALL",
            Code = "",
            UID = "1"
        },
        new SelectionItemTab
        {
            Label = "Saved/Draft",
            Code = PurchaseOrderStatusConst.Draft,
            UID = "2"
        },
        new SelectionItemTab
        {
            Label = "Approval Pending-DMS",
            Code = PurchaseOrderStatusConst.PendingForApproval,
            UID = "3"
        },
        new SelectionItemTab
        {
            Label = "Oracle Order Status",
            Code = PurchaseOrderStatusConst.InProcessERP,
            UID = "4"
        },
        new SelectionItemTab
        {
            Label = "Cancelled",
            Code = PurchaseOrderStatusConst.CancelledByCMI,
            UID = "5"
        },
    ];

    private SelectionManager TabSelectionManager =>
        new(TabSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Single);

    private List<DataGridColumn> GridColumns = [];

    private readonly IDataService DataService = new DataServiceModel()
    {
        HeaderText = "View Purchase Order Status",
        BreadcrumList =
        [
            new BreadCrumModel()
            {
                SlNo = 1, Text = "View Purchase Order Status"
            },
        ]
    };

    protected override async Task OnInitializedAsync()
    {
        ShowLoader();
        //PreviousUrl = _navigationhistory.PreviousUrl;
        ////_locationChangedHandler = (sender, args) => CheckAndClearReferrerIfNeeded();
        //// Subscribe to navigation events to save state when navigating away
        //_locationChangedHandler = (sender, args) => SaveCurrentState();
        //NavigationManager.LocationChanged += _locationChangedHandler;
        GridColumns =
        [
            new DataGridColumn
            {
                Header = "Channel Partner",
                IsSortable = true,
                GetValue = s =>
                    $"[{((IPurchaseOrderHeaderItem)s).ChannelPartnerCode}]{((IPurchaseOrderHeaderItem)s).ChannelPartnerName}",
                SortField = "channelpartnercode"
            },
            new DataGridColumn
            {
                Header = "DMS Purchase Order No",
                IsSortable = true,
                GetValue = s => ((IPurchaseOrderHeaderItem)s).OrderNumber,
                SortField = "ordernumber",
                Style = "he"
            },
            new DataGridColumn
            {
                Header = "DMS Creation Date",
                IsSortable = true,
                GetValue = s => ((IPurchaseOrderHeaderItem)s).OrderDate.ToString("dd MMM yyyy HH:mm:ss"),
                SortField = "orderdate"
            },
            new DataGridColumn
            {
                Header = "DMS Approval Date",
                IsSortable = true,
                GetValue = s =>
                    CommonFunctions.IsDateNull(((IPurchaseOrderHeaderItem)s).CPEConfirmDateTime)
                        ? "N/A"
                        : ((IPurchaseOrderHeaderItem)s).CPEConfirmDateTime!.ToString("dd MMM yyyy HH:mm:ss"),
                SortField = "cpeconfirmdatetime"
            },
            new DataGridColumn
            {
                Header = "Oracle Order Number",
                IsSortable = true,
                GetValue = s => ((IPurchaseOrderHeaderItem)s).OracleNo ?? "N/A",
                SortField = "OracleNo"
            },
            new DataGridColumn
            {
                Header = "Oracle Order Date",
                IsSortable = true,
                GetValue = s =>
                    CommonFunctions.IsDateNull(((IPurchaseOrderHeaderItem)s).OracleOrderdate)
                        ? "N/A"
                        : ((IPurchaseOrderHeaderItem)s).OracleOrderdate.Value.ToString("dd MMM yyyy"),
                SortField = "oracleorderdate"
            },
            new DataGridColumn
            {
                Header = "Net Amount",
                IsSortable = true,
                GetValue = s => CommonFunctions.FormatNumberInIndianStyle(((IPurchaseOrderHeaderItem)s).NetAmount),
                SortField = "netamount",
                Class = "cls_purchase_amount"
            },
            new DataGridColumn
            {
                Header = "ASM Name",
                IsSortable = true,
                GetValue = s => ((IPurchaseOrderHeaderItem)s).ReportingEmpName,
                SortField = "ReportingEmpName"
            },
            new DataGridColumn
            {
                Header = "Oracle Order Status",
                IsSortable = true,
                GetValue = s => ((IPurchaseOrderHeaderItem)s).OracleOrderStatus ?? "N/A",
                SortField = "OracleOrderStatus"
            },
            new DataGridColumn
            {
                IsButtonColumn = true,
                Header = "Action",
                IsSortable = false,
                ButtonActions =
                [
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "Images/edit.png",
                        Action = e => OnEditbtnClick((IPurchaseOrderHeaderItem)e)
                    }
                ],
            }
        ];

        _viewModel.PageNumber = 1;
        _viewModel.PageSize = 50;
        PrepareFilterData();

        // Try to restore state before initializing the page
        //bool stateRestored = RestoreState();

        //if (!stateRestored)
        //{
        //    // Only perform default initialization if state wasn't restored
        //    await OnTabSelect(TabSelectionItems.FirstOrDefault()!);
        //}


        //await OnTabSelect(TabSelectionItems.FirstOrDefault()!);
        //await GetTabItemsCount();
        await _viewModel.LoadChannelPartner();

        await StateChageHandler();
        await GetTabItemsCount();
        HideLoader();
    }
    private async Task StateChageHandler()
    {
        _navigationManager.LocationChanged += (sender, args) => SavePageState();
        bool stateRestored = _pageStateHandler.RestoreState("ViewPurchaseOrderStatus", ref FilterColumns, out PageState pageState);
        if (stateRestored && pageState != null && pageState.SelectedTabUID != null)
        {
            ///only work with filters
            await OnFilterClick(_pageStateHandler._currentFilters);
            //if tabs also there should use this also
            TabSelectionItems.ForEach(p => p.IsSelected = (p.UID == pageState.SelectedTabUID));
        }
        //if tabs also there should use this also
        await OnTabSelect(pageState != null && pageState.SelectedTabUID != null ?
            TabSelectionItems.FirstOrDefault(p => p.IsSelected) : TabSelectionItems.FirstOrDefault());


    }
    private void SavePageState()
    {
        _navigationManager.LocationChanged -= (sender, args) => SavePageState();
        _pageStateHandler.SaveCurrentState("ViewPurchaseOrderStatus", TabSelectionManager.GetSelectedSelectionItems().FirstOrDefault()?.UID ?? "");
    }
    //private void CheckAndClearReferrerIfNeeded()
    //{
    //    // Get the current URL
    //    string currentUrl = NavigationManager.Uri;

    //    // If we're not on the ViewPurchaseOrderStatus page and not on the details page,
    //    // we should clear the referrer to prevent incorrect state restoration later
    //    if (!currentUrl.ToLower().Contains("viewpurchaseorderstatus") &&
    //        !currentUrl.ToLower().Contains("purchaseorder/"))
    //    {
    //        _stateService.ClearPageState("referrer");
    //    }
    //}
    //private bool RestoreState()
    //{
    //    if (string.IsNullOrEmpty(PreviousUrl))
    //    {
    //        _stateService.ClearPageState("referrer");
    //        return false;
    //    }

    //    // Check if any of the acceptable previous pages is contained in the PreviousUrl
    //    bool isFromAcceptablePage = _stateService.GetAcceptablePreviousPages()
    //        .Any(page => PreviousUrl.Contains($"/{page}/", StringComparison.OrdinalIgnoreCase));

    //    if (!isFromAcceptablePage)
    //    {
    //        _stateService.ClearPageState("referrer");
    //        return false;
    //    }
    //    // Get the current route
    //    string currentRoute = "viewpurchaseorderstatus";
    //    // Check if we came back from the specific page
    //    var referrerState = _stateService.GetReferedPageState("referrer");
    //    if (referrerState?.ExtraData != "viewpurchaseorderstatus")
    //        return false;
    //    // Check if there's saved state for this page
    //    var savedState = _stateService.GetPageState(currentRoute);
    //    if (savedState != null)
    //    {
    //        // Restore filters
    //        if (savedState.Filters != null && savedState.Filters.Count > 0)
    //        {
    //            _currentFilters = savedState.Filters.ToDictionary(entry => entry.Key, entry => entry.Value.ToString());

    //            BindFiltersToUI(_currentFilters);
    //            // Apply the saved filters
    //            _ = OnFilterClick(_currentFilters);
    //        }

    //        // Restore selected tab
    //        if (!string.IsNullOrEmpty(savedState.SelectedTabUID))
    //        {
    //            var selectedTab = TabSelectionItems.FirstOrDefault(t => t.UID == savedState.SelectedTabUID);
    //            if (selectedTab != null)
    //            {
    //                _ = OnTabSelect(selectedTab);
    //            }
    //        }

    //        // Clear state after restoring
    //        _stateService.ClearPageState(currentRoute);
    //        _stateService.ClearPageState("referrer");
    //        return true;
    //    }

    //    return false;
    //}
    //private void SaveCurrentState()
    //{
    //    // Check if we're navigating away from our page
    //    string currentUrl = NavigationManager.Uri;
    //    if (!currentUrl.ToLower().Contains("viewpurchaseorderstatus"))
    //    {
    //        var state = new Winit.Modules.Common.UIState.Classes.PageState
    //        {
    //            Filters = _currentFilters,
    //            SelectedTabUID = TabSelectionManager.GetSelectedSelectionItems().FirstOrDefault()?.UID
    //        };

    //        _stateService.SavePageState("viewpurchaseorderstatus", state);
    //    }
    //}
    //private void BindFiltersToUI(Dictionary<string, string> filters)
    //{
    //    foreach (var filter in FilterColumns)
    //    {
    //        if (filters.TryGetValue(filter.ColumnName, out var value))
    //        {
    //            if (filter.FilterType == FilterConst.TextBox || filter.FilterType == FilterConst.Date)
    //            {
    //                filter.SelectedValue = value;
    //            }
    //            else if (filter.FilterType == FilterConst.DropDown)
    //            {
    //                var selectedValues = value.Split(',').ToList();

    //                // Match the selected values with the actual dropdown items
    //                var matchedItems = filter.DropDownValues
    //                    .Where(item => selectedValues.Contains(item.UID))
    //                    .Cast<ISelectionItem>()
    //                    .ToList();

    //                filter.SelectedValues = matchedItems;

    //                //_viewModel.ChannelPartnerSelectionItems.ForEach(p =>
    //                //{
    //                //    p.IsSelected = filter.SelectedValues.Equals(QtyCaptureMode=);
    //                //});
    //            }

    //        }
    //    }
    //}

    private void PrepareFilterData()
    {
        FilterColumns.AddRange(
        new List<FilterModel>
        {
            new()
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                SelectionMode = SelectionMode.Multiple,
                Label = "Channel Partner",
                ColumnName = "OrgUID",
                DropDownValues = _viewModel.ChannelPartnerSelectionItems
            },
            new()
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                Label = "DMS Purchase Order No",
                ColumnName = "OrderNumber"
            },
            new()
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                Label = "Oracle Order Number",
                ColumnName = "OracleNo"
            },
            new()
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                Label = "DMS Creation Start Date",
                ColumnName = "startdate"
            },
            new()
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                Label = "DMS Creation End Date",
                ColumnName = "enddate"
            },
            new()
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                DropDownValues = _viewModel.OracleOrderStatusSelectionItems,
                SelectionMode = SelectionMode.Multiple,
                Label = "Oracle Order Status",
                ColumnName = "OracleOrderStatus"
            },
        });
    }

    private void CreateNewPurchaseOrder()
    {
        _navigationManager.NavigateTo("purchaseorder");
    }

    private async Task GetTabItemsCount()
    {
        IDictionary<string, int> statusDict = await _viewModel
            .GetTabItemsCount(_viewModel.FilterCriterias);
        int allCount = 0;
        // foreach (KeyValuePair<string, int> item in statusDict)
        // {
        //     ISelectionItem? tab = TabSelectionItems.Find(e => e.Code == item.Key);
        //     if (tab is SelectionItemTab selectionItemTab)
        //     {
        //         selectionItemTab.Count = item.Value;
        //     }
        //     allCount += item.Value;
        // }

        foreach (SelectionItemTab tab in TabSelectionItems)
        {
            if (statusDict.TryGetValue(tab.Code, out int count))
            {
                tab.Count = count;
                allCount += count;
            }
            else
            {
                tab.Count = 0;
            }
        }
        ISelectionItem? allTab = TabSelectionItems.Find(e => e.UID == "1");
        if (allTab is SelectionItemTab allSelectionItemTab)
        {
            allSelectionItemTab.Count = allCount;
        }
    }

    public async Task OnTabSelect(ISelectionItem selectionItem)
    {
        ShowLoader();
        if (selectionItem != null && !selectionItem.IsSelected)
        {
            TabSelectionManager.Select(selectionItem);
            _viewModel.PageNumber = 1;
            await FetchDataAsync(selectionItem.Code!);
            DataGridColumn? column = GridColumns.Find(e => e.Header == "ASM Name" || e.Header == "Created By" || e.Header == "ASM Name / Created By");
            if (selectionItem.UID == "1" && column != null)
            {
                column.Header = "ASM Name / Created By";
                column.GetValue = (item) =>
                {
                    var item1 = (IPurchaseOrderHeaderItem)item;
                    return item1.Status == PurchaseOrderStatusConst.Draft ? $"[{item1.CreatedByEmpCode}] {item1.CreatedByEmpName}" : $"[{item1.ReportingEmpCode}] {item1.ReportingEmpName}";
                };
                column.SortField = "ReportingEmpName";
            }
            else if (selectionItem.UID == "2" && column != null)
            {
                column.Header = "Created By";
                column.GetValue = (item) =>
                    $"[{(item as IPurchaseOrderHeaderItem).CreatedByEmpCode}] {(item as IPurchaseOrderHeaderItem).CreatedByEmpName}";
                column.SortField = "CreatedByEmpName";
            }
            else
            {
                column.Header = "ASM Name";
                column.GetValue = (item) =>
                    $"[{(item as IPurchaseOrderHeaderItem).ReportingEmpCode}] {(item as IPurchaseOrderHeaderItem).ReportingEmpName}";
                column.SortField = "ReportingEmpName";
            }
        }
        StateHasChanged();
        HideLoader();
    }

    private async Task FetchDataAsync(string status)
    {
        try
        {
            _viewModel.Status = status;
            await _viewModel.PopulateViewModel();
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert("Failed to fetch purchase order headers", ex.Message);
        }
    }

    private void OnEditbtnClick(IPurchaseOrderHeaderItem purchaseOrderHeaderItem)
    {
        // Save the current page as the referrer
        _stateService.SaveRefferedState("referrer", new Winit.Modules.Common.UIState.Classes.PageState
        {
            SelectedTabUID = null,
            Filters = null,
            ExtraData = "viewpurchaseorderstatus"
        });
        _navigationManager.NavigateTo($"purchaseorder/{purchaseOrderHeaderItem.UID}");
    }

    private async Task OnSortClick(SortCriteria sortCriteria)
    {
        await InvokeAsync(async () =>
        {
            ShowLoader();

            await _viewModel.OnSorting(sortCriteria);
            HideLoader();
        });
    }

    private async Task PageIndexChanged(int pageNumber)
    {
        await InvokeAsync(async () =>
        {
            ShowLoader();

            await _viewModel.PageIndexChanged(pageNumber);
            HideLoader();
        });
    }

    private static string GetStatus(string status)
    {
        return status switch
        {
            PurchaseOrderStatusConst.Draft => "Draft",
            PurchaseOrderStatusConst.PendingForApproval => "Pending For Approval",
            PurchaseOrderStatusConst.InProcessERP => "In Process ERP",
            PurchaseOrderStatusConst.Invoiced => "Inoviced",
            PurchaseOrderStatusConst.Shipped => "Shipped",
            PurchaseOrderStatusConst.CancelledByCMI => "Cancelled By CMI",
            _ => "Draft",
        };
    }

    private static string GetOracleOrderStatus(string status)
    {
        return status switch
        {
            _ => "N/A",
        };
    }
    //private void FilterConverter(Dictionary<string, string> filtersDict)
    //{
    //    _viewModel.FilterCriterias.Clear();
    //    _viewModel.FilterCriterias.AddRange();
    //}
    private async Task OnFilterClick(Dictionary<string, string> filtersDict)
    {
        try
        {
            ShowLoader();
            // Save the current filters
            _pageStateHandler._currentFilters = filtersDict;
            var filters = filtersDict.Where(_ => !string.IsNullOrEmpty(_.Value)).Select(e =>
            {
                if (e.Key == "startdate") return new FilterCriteria("OrderDate", e.Value, FilterType.GreaterThanOrEqual, typeof(DateTime));
                else if (e.Key == "enddate") return new FilterCriteria("OrderDate", e.Value, FilterType.LessThanOrEqual, typeof(DateTime));
                else if (e.Key == "OracleNo" || e.Key == "orderno") return new(e.Key, e.Value, FilterType.Like);
                else if (e.Key == "OrgUID") return new(e.Key, e.Value.Split(","), FilterType.In);
                else if (e.Key == "OracleOrderStatus") return new FilterCriteria(e.Key, e.Value.Split(","), FilterType.In);
                return new FilterCriteria(e.Key, e.Value, FilterType.Equal);
            }).ToList();
            await _viewModel.ApplyFilter(filters);
            await GetTabItemsCount();
        }
        catch (Exception e)
        {
        }
        finally
        {
            HideLoader();
        }
    }
    public void Dispose()
    {
        // Unsubscribe from navigation events when component is disposed
        if (_locationChangedHandler != null)
            NavigationManager.LocationChanged -= _locationChangedHandler;
    }
}
